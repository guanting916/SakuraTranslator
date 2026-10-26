// Managed .NET 3.5 conversion using OpenCC 1.1.9 s2twp dictionaries.
// OpenCC: Copyright 2010-2024 Carbo Kuo and contributors, Apache-2.0.
// This new implementation preserves markup and performs the s2twp stages
// within STPhrases longest-match segments, using the first dictionary candidate.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SakuraTranslate.Helpers
{
    internal static class TaiwanChineseConverter
    {
        private sealed class Node
        {
            internal Dictionary<char, Node> Children;
            internal string Value;

            internal void Add(string key, string value)
            {
                Node node = this;
                foreach (char c in key)
                {
                    if (node.Children == null) node.Children = new Dictionary<char, Node>();
                    Node next;
                    if (!node.Children.TryGetValue(c, out next))
                    {
                        next = new Node();
                        node.Children.Add(c, next);
                    }
                    node = next;
                }
                if (node.Value == null) node.Value = value;
            }

            internal string Match(string text, int start, out int length)
            {
                Node node = this;
                string value = null;
                length = 0;
                for (int i = start; i < text.Length; i++)
                {
                    Node next;
                    if (node.Children == null || !node.Children.TryGetValue(text[i], out next)) break;
                    node = next;
                    if (node.Value != null)
                    {
                        value = node.Value;
                        length = i - start + 1;
                    }
                }
                return value;
            }
        }

        // CLR static initialization loads dictionaries once, safely across threads.
        private static class Dictionaries
        {
            internal static readonly Node Phrases = Load("STPhrases");
            internal static readonly Node Characters = Load("STCharacters");
            internal static readonly Node TaiwanPhrases = Load("TWPhrasesIT", "TWPhrasesName", "TWPhrasesOther");
            internal static readonly Node TaiwanVariants = Load("TWVariants");
        }

        private static readonly Regex ProtectedTokens = new Regex(@"<[^>\r\n]*>|\{[^{}\r\n]*\}");

        private static Node Load(params string[] names)
        {
            Node root = new Node();
            foreach (string name in names)
            {
                string resource = "SakuraTranslate.Resources.OpenCC." + name + ".txt";
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    if (stream == null) throw new InvalidOperationException("Missing OpenCC dictionary: " + resource);
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Length == 0 || line[0] == '#') continue;
                            int tab = line.IndexOf('\t');
                            if (tab <= 0) throw new InvalidDataException("Invalid OpenCC dictionary: " + name);
                            string value = line.Substring(tab + 1);
                            int space = value.IndexOf(' ');
                            if (space >= 0) value = value.Substring(0, space);
                            root.Add(line.Substring(0, tab), value);
                        }
                    }
                }
            }
            return root;
        }

        internal static string Convert(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            StringBuilder result = new StringBuilder(text.Length);
            int start = 0;
            foreach (Match token in ProtectedTokens.Matches(text))
            {
                result.Append(ConvertText(text.Substring(start, token.Index - start)));
                result.Append(token.Value);
                start = token.Index + token.Length;
            }
            result.Append(ConvertText(text.Substring(start)));
            return result.ToString();
        }

        private static string ConvertText(string text)
        {
            StringBuilder result = new StringBuilder(text.Length);
            int pending = 0;
            int i = 0;
            while (i < text.Length)
            {
                int length;
                if (Dictionaries.Phrases.Match(text, i, out length) != null)
                {
                    if (i > pending) result.Append(ConvertSegment(text.Substring(pending, i - pending)));
                    result.Append(ConvertSegment(text.Substring(i, length)));
                    i += length;
                    pending = i;
                }
                else i += CharLength(text, i);
            }
            if (pending < text.Length) result.Append(ConvertSegment(text.Substring(pending)));
            return result.ToString();
        }

        private static string ConvertSegment(string text)
        {
            string traditional = Apply(text, Dictionaries.Phrases, Dictionaries.Characters);
            return Apply(Apply(traditional, Dictionaries.TaiwanPhrases, null), Dictionaries.TaiwanVariants, null);
        }

        private static int CharLength(string text, int index)
        {
            return char.IsHighSurrogate(text[index]) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]) ? 2 : 1;
        }

        private static string Apply(string text, Node primary, Node fallback)
        {
            StringBuilder result = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length;)
            {
                int length;
                string value = primary.Match(text, i, out length);
                if (value == null && fallback != null) value = fallback.Match(text, i, out length);
                if (value == null)
                {
                    length = CharLength(text, i);
                    result.Append(text, i, length);
                }
                else result.Append(value);
                i += length;
            }
            return result.ToString();
        }
    }
}
