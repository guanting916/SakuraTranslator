# 此 Fork 的改動

上游：<https://github.com/fkiliver/SakuraTranslator>，基準 commit `08fc4677384a6772d9ed6052ad8f870a0fdc9df3`（v0.3.10 的來源）。

本次版本識別為 `0.3.10-tw.1`，檔案版本 `0.3.10.1`。為保留既有組件介面，AssemblyVersion 維持 `0.3.10.0`；請不要只用 AssemblyVersion 判定是否為相同 DLL。

## 提示詞

僅更新 Sakura 1.0 模板：

- system 改為臺灣繁體中文，加入逐行翻譯及保留換行、空白行的規則。
- user 的一般與術語表兩種分支，都改為臺灣繁體中文並保留換行規則。
- 保留術語表內容與原有生成參數。
- 其他 Sakura／GalTransl 模板保持上游原樣。

提示規則同時涵蓋一般翻譯與術語表分支，原文作為當前文本傳送。

## 繁體轉換

新增受版本管理的 `TaiwanChineseConverter.cs`，在模型回覆後執行字詞轉換。

- 依 OpenCC 1.1.9 `s2twp` 字典階段進行轉換，優先使用第一個字典候選。
- 字典以內嵌資源載入，不需額外啟動原生 OpenCC。
- 保留遊戲控制文字顏色、填入玩家名字等用途的代號，並保留原本的分行、表情符號及少見漢字。
- 只在 Sakura 1.0 的回覆流程中，於原有 Unescape／FixTranslationEnd 後、`context.Complete` 前執行。
- 六份 OpenCC 字典及授權文字以內嵌資源保存，跨平台取出原始碼時不自動改寫其換行位元組。

OpenCC 字典的 Apache-2.0 授權及來源資訊見 `OpenCC-LICENSE.txt`、`THIRD-PARTY-NOTICES.txt`。

## 建置依賴

新增 `XUnityReferenceDirectory` 專案參數與 `build.ps1`。本次重建明確使用對應環境的 XUnity 5.6.2 組件；未把目標遊戲的 DLL 加進儲存庫。未指定參數時，原 SDK 專案保留上游的 5.4.4 依賴路徑，不能將此預設視為重建本機 5.6.2 環境。

## 驗證

既有 Mono DLL 與重建 Mono DLL 分別通過 15 項檢查，報告一致：

- 繁體、臺灣用語、標記、占位符、換行、Unicode、空字串及 null。
- system／user 提示、當前原文與術語表。
- 經本機模擬 HTTP API 到實際 `Translate` 協程，再到 `Complete` 的轉換結果。

模擬 API 不需要遊戲、LLM 或對外網路，不代表模型翻譯品質或實機相容性已全面驗證。Mono 與 IL2CPP 兩種目標均已編譯。另以重建的 Mono DLL 執行 TextSourceInspector 的 77 項請求組裝、姓名配對、參數、UI 隔離與 LOG 整合檢查，全部通過。

## 本機比對基準

- 指定的現有 DLL：SHA-256 `7ad668c24da01344967ba660c2ce2bd76512674e316cfd30bede72b02654d17e`。
- 官方 v0.3.10 Mono DLL：SHA-256 `68146c5d08618fa2854af00f344ec6ba660b2b0487525b3528aedb14699d079b`。

本次整理的是可重建來源與已確認的行為，不宣稱新建置的 DLL 與舊 DLL 每個位元組完全相同。
