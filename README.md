# SakuraTranslator 臺灣繁體版

本專案的目標是讓 Unity 遊戲的 **Sakura 1.0 翻譯使用臺灣繁體中文**，並保留原文的換行與遊戲文字標記。

專案基於 [fkiliver/SakuraTranslator](https://github.com/fkiliver/SakuraTranslator)，調整翻譯提示詞，並在模型回覆後加入 OpenCC 字詞轉換，讓譯文的用字與部分用語更符合臺灣繁體中文習慣。

## 主要功能

- **臺灣繁體提示詞**：調整 Sakura 1.0 的 system／user 提示詞，一般翻譯與術語表兩種流程皆套用。
- **回覆字詞轉換**：使用內嵌的 OpenCC 1.1.9 `s2twp` 字典，將簡體字與部分用語轉為臺灣繁體。
- **保留文字格式**：提示模型保留原文換行與空白行；回覆轉換時保留遊戲的文字顏色標記、玩家名字代號與分行。
- **支援 Mono 與 IL2CPP**：提供對應環境的翻譯端點版本。

## 相關專案

- [XUnity.AutoTranslator](https://github.com/bbepis/XUnity.AutoTranslator)：本端點搭配的遊戲文字擷取與翻譯流程。
- [TextSourceInspector](https://github.com/guanting916/TextSourceInspector)：另行提供對話前文、說話者與翻譯請求紀錄。

## 目前限制

- 本版新增的繁體提示與回覆轉換僅套用 Sakura 1.0，其他模型模板維持上游行為。
- 提示詞與字詞轉換無法保證翻譯正確，也無法修正所有人稱、語意或換行錯誤。
- 字典轉換可能影響專有名詞，目前沒有獨立關閉轉換的設定。
- 本端點不收集對話歷史，也不會自動重翻 XUnity 已有的翻譯快取。
- 翻譯模型與 API 服務需另行準備。

## 開發與驗證

程式透過 **GPT6.1 SOL** 協助修改、建置與驗證。改動範圍見 [FORK-CHANGES.md](FORK-CHANGES.md)，建置與測試方式見 [BUILDING.md](BUILDING.md)。

## 來源與第三方授權

- 上游：[fkiliver/SakuraTranslator](https://github.com/fkiliver/SakuraTranslator)。保留原專案歷史與[上游原始說明](UPSTREAM-README.md)。
- 字典：[OpenCC](https://github.com/BYVoid/OpenCC) 1.1.9。來源與原作者資訊見 [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt)，授權全文見 [OpenCC-LICENSE.txt](OpenCC-LICENSE.txt)。

OpenCC 字典按 Apache-2.0 保留授權；上游程式碼及其他依賴適用各自授權情況。
