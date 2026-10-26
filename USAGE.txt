# SakuraTranslator 臺灣繁體版

基於 [fkiliver/SakuraTranslator](https://github.com/fkiliver/SakuraTranslator) 的翻譯端點插件，為 **Sakura 1.0** 加入臺灣繁體中文提示與回覆轉換。使用者需另行安裝 XUnity.AutoTranslator，並啟動相容的翻譯 API 服務。

目前版本：**v0.3.10-tw.1 Pre-release**。本專案透過 **GPT6.1 SOL** 協助修改、建置與驗證。

## 修改內容

| 部分 | 功能 |
|---|---|
| Sakura 1.0 的 system／user | 要求模型翻譯成臺灣繁體中文，保留原文換行與空白行。一般翻譯及術語表兩種分支皆套用。 |
| 回覆後處理 | 使用 OpenCC 1.1.9 `s2twp` 字典，將回覆中的簡體字與部分用語轉成臺灣繁體。 |
| 文字格式 | 轉換時保留尖括號標記、花括號占位符與換行。 |
| 建置與測試 | 可指定 XUnity 參考組件位置，附有字詞、格式、提示詞及模擬 API 驗證程式。 |

提示詞是在生成前要求模型使用繁體；OpenCC 是收到回覆後補做字形與臺灣用語轉換。模型已輸出繁體時，多數文字會保持原樣，但部分用語仍可能調整。這項轉換不會修正人稱、主詞或原文理解錯誤。

**上述提示與轉換僅套用 Sakura 1.0。**其他 Sakura／GalTransl 模板、術語表機制與生成參數維持上游原有行為。本版本沒有獨立的 OpenCC 啟用／停用設定。

## 1. 下載前先確認環境

- 已安裝適用該遊戲的 BepInEx 與 XUnity.AutoTranslator。本次建置使用 XUnity **5.6.2** 的對應組件。
- 已啟動 Sakura 相容的 chat completions API；本專案不包含模型或伺服器。
- 確認遊戲是 Mono 或 IL2CPP，兩版 DLL 不可混用。

| 環境 | 下載檔案 |
|---|---|
| BepInEx 5 Mono | `SakuraTranslate.dll` |
| BepInEx 6 IL2CPP | `SakuraTranslate_IL2CPP.dll` |

## 2. 安裝

1. 到本 Fork 的 [v0.3.10-tw.1 Release](https://github.com/guanting916/SakuraTranslator/releases/tag/v0.3.10-tw.1)，從 **Assets** 下載對應的一份 DLL。一般安裝不需下載 Source code 或自行編譯。
2. 關閉遊戲，將舊版 SakuraTranslate DLL 移到遊戲目錄外備份，避免同時載入多份端點。
3. 把下載的 DLL 放入遊戲內 XUnity 的 `Translators` 資料夾。BepInEx 套件通常位於：

```text
遊戲資料夾/
├── Game.exe
└── BepInEx/
    ├── plugins/
    │   └── XUnity.AutoTranslator/
    │       └── Translators/
    │           └── 對應的 SakuraTranslate DLL
    └── config/
        └── AutoTranslatorConfig.ini
```

`Game.exe` 與「對應的 SakuraTranslate DLL」是示意名稱。若原套件資料夾結構不同，請使用其既有 `Translators` 位置，不要任意移動 XUnity 本身。

## 3. 設定

首次啟動遊戲產生設定後關閉，開啟 `BepInEx/config/AutoTranslatorConfig.ini`，修改既有檔案內的對應項目，不要只用以下片段覆蓋整份設定：

```ini
[Service]
Endpoint=SakuraTranslate

[General]
Language=zh-TW
FromLanguage=ja

[Sakura]
Endpoint=http://127.0.0.1:8080/v1/chat/completions
ModelName=Sakura
ModelVersion=1.0
```

API 位址需與你實際啟動的服務一致；上面的本機 8080 埠只是例子。儲存設定後重新啟動遊戲。`ModelName`／`ModelVersion` 需如上設定，才會使用本版修改的 Sakura 1.0 模板及繁體轉換。

OpenCC 字典已內嵌在 DLL，不需額外安裝 OpenCC，也不需複製字典到遊戲目錄。本專案不附字型檔；若有缺字，需另處理字型設定。

## 4. 對話上下文與 LOG

這個端點本身不選取文字元件或收集對話歷史。若要對話前文、姓名配對及詳細請求 LOG，可另外使用 [TextSourceInspector](https://github.com/guanting916/TextSourceInspector)。

單獨使用此端點時，可在 `BepInEx/LogOutput.log` 查看載入或連線錯誤。XUnity 已有的翻譯快取不會因更換 DLL 自動重翻。

## 常見問題

- **完全沒有翻譯**：確認 API 已啟動、網址正確、DLL 類型符合環境，以及 `[Service] Endpoint=SakuraTranslate`。
- **沒有使用繁體規則**：確認裝的是本 Fork 的 DLL，且 `ModelName=Sakura`、`ModelVersion=1.0`，並檢查是否讀到舊快取。
- **換行或人稱仍翻錯**：提示詞只是要求模型遵守；字詞轉換不能修正所有模型錯誤。
- **專有名詞被轉換**：OpenCC 採用字典的候選詞，可能影響名稱或特定用語；請檢查回覆。本版沒有獨立關閉轉換的設定。
- **更換版本或還原**：先關閉遊戲，只保留一份 Sakura 端點 DLL，換回備份後再啟動。

## 開發與驗證

修改位置與範圍見 [FORK-CHANGES.md](FORK-CHANGES.md)，自行建置與測試見 [BUILDING.md](BUILDING.md)。這是本機建置的 Pre-release，不代表所有 Unity、模型及端點環境都已驗證。

組件版本維持 `0.3.10.0`，檔案版本為 `0.3.10.1`，資訊版本含 `0.3.10-tw.1` 與來源 commit；相同組件版本號不代表 DLL 內容完全相同。

## 來源與第三方授權

- 上游：[fkiliver/SakuraTranslator](https://github.com/fkiliver/SakuraTranslator)。保留原專案歷史；[上游原始說明](UPSTREAM-README.md)僅作參考，其 Release 不包含本 Fork 修改。
- 字典：[OpenCC](https://github.com/BYVoid/OpenCC) 1.1.9，來源與原作者資訊見 [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt)，授權全文見 [OpenCC-LICENSE.txt](OpenCC-LICENSE.txt)。

OpenCC 字典按 Apache-2.0 保留授權；這不代表 SakuraTranslator 上游程式碼或其他依賴自動改採 Apache-2.0 或 MIT。
