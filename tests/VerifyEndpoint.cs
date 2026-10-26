using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

// Uses a local fake API. No model, game process or external API is required.
public static class VerifyEndpoint
{
    static int checks;
    static JavaScriptSerializer json = new JavaScriptSerializer();
    static void Check(bool value, string message)
    { if (!value) throw new Exception(message); checks++; }

    public static int Main(string[] args)
    {
        if (args.Length != 3) { Console.Error.WriteLine("DLL dependency-directory report-path"); return 2; }
        AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs e)
        {
            string path = Path.Combine(args[1], new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        try { return Run(args); } catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    static int Run(string[] args)
    {
        Assembly assembly = Assembly.LoadFrom(args[0]);
        MethodInfo convert = assembly.GetType("SakuraTranslate.Helpers.TaiwanChineseConverter", true)
            .GetMethod("Convert", BindingFlags.NonPublic | BindingFlags.Static);
        string[][] cases = {
            new[]{"今天天气很好。请进入图书馆，关上大门。", "今天天氣很好。請進入圖書館，關上大門。"},
            new[]{"头发发展，皇后在后面。", "頭髮發展，皇后在後面。"},
            new[]{"软件和鼠标，内存和硬盘。", "軟體和滑鼠，記憶體和硬碟。"},
            new[]{"<color=red>关闭</color> {player_name} <sprite name=\"简体图标\">", "<color=red>關閉</color> {player_name} <sprite name=\"简体图标\">"},
            new[]{"天气\n行动力\t信息", "天氣\n行動力\t資訊"},
            new[]{"ABC 123 https://localhost:8080 {0} 😀 𠮷", "ABC 123 https://localhost:8080 {0} 😀 𠮷"},
            new[]{"繁體中文：圖書館、天氣。", "繁體中文：圖書館、天氣。"},
            new[]{"", ""}
        };
        List<object> conversions = new List<object>();
        foreach (string[] pair in cases)
        {
            string result = (string)convert.Invoke(null, new object[]{pair[0]});
            Check(result == pair[1], "conversion: " + pair[0]);
            conversions.Add(result);
        }
        Check(convert.Invoke(null, new object[]{null}) == null, "null input");

        Type endpointType = assembly.GetType("SakuraTranslate.SakuraTranslateEndpoint", true);
        object endpoint = Activator.CreateInstance(endpointType);
        Set(endpointType, endpoint, "_staticMaxTokens", 512);
        Set(endpointType, endpoint, "_modelType", Enum.Parse(assembly.GetType("SakuraTranslate.Models.TranslationModel"), "SakuraV1_0"));
        MethodInfo builder = endpointType.GetMethod("MakeSakuraPromptV1_0", BindingFlags.Instance | BindingFlags.NonPublic);
        string source = "テスト\n\n次の行\\n";
        string request = (string)builder.Invoke(endpoint, new object[]{source, 0.0});
        Dictionary<string, object> parsed = json.Deserialize<Dictionary<string, object>>(request);
        IList messages = (IList)parsed["messages"];
        string system = (string)((Dictionary<string, object>)messages[0])["content"];
        string user = (string)((Dictionary<string, object>)messages[1])["content"];
        Check(system.Contains("臺灣繁體中文") && system.Contains("逐行"), "traditional system and line rules");
        Check(user.EndsWith(source, StringComparison.Ordinal), "source newlines and literal backslash-n preserved");

        Set(endpointType, endpoint, "_dictMode", Enum.Parse(assembly.GetType("SakuraTranslate.Models.DictMode"), "Full"));
        Set(endpointType, endpoint, "_fullDictStr", "太郎->太郎 #姓名");
        string dictionaryRequest = (string)builder.Invoke(endpoint, new object[]{source, 0.0});
        Check(dictionaryRequest.Contains("太郎->太郎 #姓名"), "dictionary retained");
        Set(endpointType, endpoint, "_dictMode", Enum.Parse(assembly.GetType("SakuraTranslate.Models.DictMode"), "None"));

        ServicePointManager.Expect100Continue = false;
        TcpListener server = new TcpListener(IPAddress.Loopback, 0);
        server.Start();
        int port = ((IPEndPoint)server.LocalEndpoint).Port;
        Exception serverError = null;
        string captured = null;
        Thread thread = new Thread(delegate()
        {
            try
            {
                using (TcpClient client = server.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                {
                    stream.ReadTimeout = 10000;
                    List<byte> header = new List<byte>();
                    while (true)
                    {
                        int value = stream.ReadByte(); if (value < 0) throw new EndOfStreamException();
                        header.Add((byte)value);
                        int n = header.Count;
                        if (n >= 4 && header[n-4] == 13 && header[n-3] == 10 && header[n-2] == 13 && header[n-1] == 10) break;
                    }
                    int length = 0;
                    foreach (string line in Encoding.ASCII.GetString(header.ToArray()).Split('\n'))
                        if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) length = int.Parse(line.Substring(15).Trim());
                    byte[] body = new byte[length];
                    for (int read = 0; read < length; ) { int n = stream.Read(body, read, length-read); if (n == 0) throw new EndOfStreamException(); read += n; }
                    captured = Encoding.UTF8.GetString(body);
                    string response = json.Serialize(new {usage=new {completion_tokens=10}, choices=new[]{new {message=new {content="软件和鼠标，天气很好。\n第二行"}}}});
                    byte[] bytes = Encoding.UTF8.GetBytes(response);
                    byte[] responseHeader = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: application/json; charset=utf-8\r\nContent-Length: " + bytes.Length + "\r\nConnection: close\r\n\r\n");
                    stream.Write(responseHeader,0,responseHeader.Length); stream.Write(bytes,0,bytes.Length);
                }
            } catch (Exception ex) { serverError=ex; }
        });
        thread.IsBackground = true;
        thread.Start();
        string translated;
        try
        {
            Set(endpointType, endpoint, "_endpoint", "http://127.0.0.1:"+port+"/v1/chat/completions");
            TestContext context = new TestContext();
            IEnumerator coroutine = (IEnumerator)endpointType.GetMethod("Translate").Invoke(endpoint,new object[]{context});
            DateTime until = DateTime.UtcNow.AddSeconds(15);
            while (coroutine.MoveNext()) { if(DateTime.UtcNow > until) throw new TimeoutException(); Thread.Sleep(5); }
            Check(thread.Join(2000), "fake API finished");
            if(serverError != null) throw serverError;
            translated = context.Result;
            Check(translated == "軟體和滑鼠，天氣很好。\n第二行", "response converted before Complete");
            Check(captured != null && captured.Contains("臺灣繁體中文"), "actual HTTP request uses traditional prompt");
        } finally { server.Stop(); }
        File.WriteAllText(args[2], json.Serialize(new {conversions, request, dictionaryRequest, translated}),new UTF8Encoding(false));
        Console.WriteLine("PASS: "+checks+" checks."); return 0;
    }
    static void Set(Type type, object obj, string name, object value)
    { type.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(obj,value); }
    sealed class TestContext : ITranslationContext
    {
        public string Result;
        public string UntranslatedText {get{return "テスト";}}
        public string[] UntranslatedTexts {get{return new[]{UntranslatedText};}}
        public UntranslatedTextInfo UntranslatedTextInfo {get{return null;}}
        public UntranslatedTextInfo[] UntranslatedTextInfos {get{return null;}}
        public string SourceLanguage {get{return "ja";}}
        public string DestinationLanguage {get{return "zh-TW";}}
        public object UserState {get;set;}
        public void Complete(string value){Result=value;}
        public void Complete(string[] values){Result=string.Join("\n",values);}
        public void Fail(string message){throw new Exception(message);}
        public void Fail(string message,Exception ex){throw new Exception(message,ex);}
    }
}
