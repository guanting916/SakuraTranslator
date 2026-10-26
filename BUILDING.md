# 建置與驗證

## 需要準備

- .NET SDK 8（只有 .NET runtime 不夠）。
- 符合目標環境的 XUnity 開發用組件，放在同一個目錄：
  - `XUnity.AutoTranslator.Plugin.Core.dll`
  - `XUnity.AutoTranslator.Plugin.ExtProtocol.dll`
  - `XUnity.Common.dll`
  - `XUnity.ResourceRedirector.dll`
- 本次驗證使用 XUnity 5.6.2。Mono 與 IL2CPP 組件需分開準備，不可混用。

## 建置

```powershell
./build.ps1 -Target Mono -XUnityReferenceDirectory 'D:\BuildDeps\XUnity-Mono-5.6.2'
./build.ps1 -Target IL2CPP -XUnityReferenceDirectory 'D:\BuildDeps\XUnity-IL2CPP-5.6.2'
```

如果 SDK 不在 PATH，可另傳 `-DotnetPath 'D:\Tools\dotnet\dotnet.exe'`。第一次還原可能需要網路下載 .NET Framework 3.5 參考組件及 .NET 6 targeting pack。

兩種目標請依序建置。輸出：

- Mono：`SakuraTranslate/bin/Release/net3.5/SakuraTranslate.dll`。
- IL2CPP：`SakuraTranslate/bin/IL2CPPRelease/net6.0/SakuraTranslate.dll`。

## Mono 行為驗證

需 Windows .NET Framework C# 編譯器。測試使用本機模擬 API，不需要啟動翻譯模型。

```powershell
./tests/verify.ps1 -DllPath './SakuraTranslate/bin/Release/net3.5/SakuraTranslate.dll' -XUnityReferenceDirectory 'D:\BuildDeps\XUnity-Mono-5.6.2'
```

若有原本的修改版 DLL，可另傳 `-GoldenDllPath 'D:\Backup\SakuraTranslate.dll'`，在兩個獨立程序中執行同一組檢查並比對結果，避免相同 AssemblyVersion 導致誤用已載入的組件。

測試程式、輸出及報告放在 `tests/bin`，不納入版本管理。不要把真實劇情 LOG、遊戲設定或模型權杖放進測試範本。

## 自動建置

目前保留上游 workflow，尚未為此 Fork 設定完整的依賴準備與發布流程。使用 `build.ps1` 建置，不應把保留的 workflow 視為已完成自動發布。
