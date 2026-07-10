# RevitParameterInspector

A read-only Revit add-in that inspects a selected element's parameters, geometry, location,
relationships, and view/sheet context, then exports the result as JSON, Markdown, Excel, or
AI-readable context. Optional file-based dictionary support maps common Revit API terms to
zh-TW terminology without ever touching raw API names or user-created parameters.

See `HANDOFF_RevitParameterInspector_V1_Full.md` for the full V1 specification driving this
implementation.

## What this is / is not

RevitParameterInspector is a structured element-context inspector, parameter browser, and
JSON/Markdown/Excel/AI-context exporter for Revit API developers, BIM automation engineers,
and AI-assisted coding workflows.

**RevitParameterInspector is not intended to replace [RevitLookup](https://github.com/jeremytammik/RevitLookup).**
RevitLookup is a mature, general-purpose Revit database exploration tool built for deep object
graph traversal. This project instead focuses on structured context extraction, dictionary-ready
terminology mapping, and AI-readable exports. It is not a model modification tool, a parameter
writer, a family editor, a QA/QC engine, or an AI chatbot.

## Supported Revit versions

| Target Framework | Revit Version | Notes |
|---|---|---|
| `net48` | 2024 | |
| `net8.0-windows` | 2025 (default) | |
| `net8.0-windows` | 2026 | build with `-p:RevitVersion=2026` |

Version differences are isolated in `RevitParameterInspector.Revit/Compatibility`. Revit
install location can be overridden with `-p:RevitInstallDir=...` if Revit is installed outside
`C:\Program Files\Autodesk\Revit <version>`.

## Implementation status

This is under active development against the HANDOFF spec. Current state:

### Core (`RevitParameterInspector.Core`)
- [x] `ElementContextSnapshot` and all sub-models (Identity, Classification, Parameters,
      Geometry, Location, Relationships, ViewContext, SheetContext, DictionaryTermInfo,
      ExportMetadata)
- [x] `ObjectInspector` reflection helper shared by the UI grids and Excel sheets

### Revit integration (`RevitParameterInspector.Revit`)
- [x] External command with current-selection / pick-element workflow
- [x] Identity, Classification, Parameter (instance + type), Geometry, Location,
      Relationship, View Context, and Sheet Context builders
- [x] Dictionary Engine wired into Identity (ClassName, BuiltInCategory), Classification
      (ElementKind), and Parameters (BuiltInParameter only - user-created parameters are
      never translated)
- [ ] Ribbon panel / button registration
- [ ] Dedicated "Pick Element" command distinct from the main inspect command

### UI (`RevitParameterInspector.UI`)
- [x] All 9 tabs: Summary, Parameters, Geometry, Location, Relationships, View/Sheet
      Context, Dictionary, Raw JSON, AI Context
- [x] Parameter search and instance/type scope filter

### Export (`RevitParameterInspector.Export`)
- [x] JSON export (full snapshot)
- [x] Markdown export (all sections, including View/Sheet Context and resolved/unresolved
      Dictionary terms)
- [x] Excel export (one sheet per section, including `View_Sheet_Context` and `Dictionary`)
- [x] Copy AI Context (concise Markdown for pasting into chat/AI tools)

### Dictionary (`RevitParameterInspector.Dictionary`)
- [x] File-based loader, multi-tier priority merge, resolver with raw-API-name fallback,
      unresolved-term tracking
- [ ] zh-TW dictionary content is a partial draft (`api_terms.json`,
      `view_sheet_terms.json` have example entries; `builtin_categories.json`,
      `builtin_parameters_common.json`, `family_terms.json`, `geometry_terms.json`,
      `parameter_terms.json` are still empty placeholders)

### Documentation (`docs/`)
- [x] getting-started, build-guide, roadmap, revit-version-support,
      ai-json-schema, markdown-export-format, excel-export-format,
      dictionary-contribution-guide

### Samples (`samples/`)
- [x] One representative example (a Viewport on a sheet) exercising every section, in
      `samples/json`, `samples/markdown`, and `samples/excel`

### Install packaging (`install/`)
- [x] Per-version `.addin` manifests (`install/addin/`) and a multi-version
      `.bundle` (`install/bundle/`, with a `build-bundle.ps1` packaging script) - see
      `install/README.md`

### Not started
- Verification against a real Revit installation (not available in the environment this was
  built in - Revit API references resolve but haven't been exercised inside Revit itself; see
  `docs/revit-version-support.md`)
- CI/release pipeline for the install packaging (currently a manual, local script)

## License

MIT - see `LICENSE`. See `NOTICE.md` for attribution notes relative to the original
RevitElementBipChecker concept this project modernizes.

這是一份針對該專案（RevitParameterInspector）開發說明的繁體中文（zh-TW）翻譯，維持了專業的技術術語與原有的格式結構：

---

# RevitParameterInspector

這是一款唯讀的 Revit 增益集（Add-in），用於檢查所選元素的參數、幾何、位置、關聯關係以及視圖/圖紙上下文，並將結果匯出為 JSON、Markdown、Excel 或 AI 可讀的上下文資訊。支援基於檔案的字典功能，可在完全不變動原始 API 名稱或使用者自訂參數的前提下，將常見的 Revit API 術語對應至繁體中文（zh-TW）術語。
完整的 V1 規格說明請參閱 `HANDOFF_RevitParameterInspector_V1_Full.md`，本實作即依此規格開發。

## 本工具的定位與非定位 (What this is / is not)

* **定位**：RevitParameterInspector 是一款專為 Revit API 開發人員、BIM 自動化工程師以及 AI 輔助編碼工作流設計的結構化元素上下文檢查器、參數瀏覽器，以及 JSON/Markdown/Excel/AI 上下文匯出工具。
* **非定位**：本工具並非為了取代 **RevitLookup**。RevitLookup 是一款成熟、通用的 Revit 資料庫探查工具，專為深層物件圖形走訪（object graph traversal）而生。而本專案的核心焦點在於**結構化的上下文擷取**、**支援字典的術語對應**，以及 **AI 可讀的匯出格式**。此外，它**不是**模型修改工具、參數寫入器、族群編輯器、QA/QC 檢查引擎，亦非 AI 聊天機器人。

## 支援的 Revit 版本

| 目標框架 (Target Framework) | Revit 版本 | 備註 |
| --- | --- | --- |
| `net48` | 2024 |  |
| `net8.0-windows` | 2025 | 預設預期版本 |
| `net8.0-windows` | 2026 | 建置命令：`-p:RevitVersion=2026` |

版本之間的差異已隔離在 `RevitParameterInspector.Revit/Compatibility` 中。如果您的 Revit 安裝路徑非預設的 `C:\Program Files\Autodesk\Revit <版本>`，可使用 `-p:RevitInstallDir=...` 來覆寫安裝路徑。

## 實作進度 (Implementation status)

目前正依據 HANDOFF 規格書進行積極開發中。當前狀態如下：

### 核心功能 (RevitParameterInspector.Core)

* [x] `ElementContextSnapshot` 及其所有子模型（包含 Identity、Classification、Parameters、Geometry、Location、Relationships、ViewContext、SheetContext、DictionaryTermInfo、ExportMetadata）
* [x] UI 網格與 Excel 表單共用的 `ObjectInspector` 反射（reflection）輔助程式

### Revit 整合 (RevitParameterInspector.Revit)

* [x] 具備「當前選取物件」/「點選元素」工作流的外部命令（External command）
* [x] 建立器（Builders）：Identity、Classification、Parameter（實體 + 類型）、Geometry、Location、Relationship、View Context 以及 Sheet Context
* [x] 字典引擎：已串接至 Identity（ClassName、BuiltInCategory）、Classification（ElementKind）與 Parameters（僅限 BuiltInParameter — 使用者自訂參數絕不進行翻譯）
* [x] 頁籤面板（Ribbon panel）與按鈕註冊
* [x] 獨立於主檢查命令之外的專用「點選元素（Pick Element）」命令

### 使用者介面 (RevitParameterInspector.UI)

* [x] 完整 9 個頁籤：摘要（Summary）、參數（Parameters）、幾何（Geometry）、位置（Location）、關聯關係（Relationships）、視圖/圖紙上下文（View/Sheet Context）、字典（Dictionary）、原始 JSON（Raw JSON）、AI 上下文（AI Context）
* [x] 參數搜尋功能與實體/類型（instance/type）領域篩選器

### 匯出功能 (RevitParameterInspector.Export)

* [x] JSON 匯出（完整快照）
* [x] Markdown 匯出（包含所有區段，以及視圖/圖紙上下文、已解析/未解析的字典術語）
* [x] Excel 匯出（每個區段獨立一頁工作表，包含 View_Sheet_Context 與 Dictionary）
* [x] 複製 AI 上下文（精簡的 Markdown 格式，便於貼入聊天室或 AI 工具中）

### 字典功能 (RevitParameterInspector.Dictionary)

* [x] 基於檔案的載入器、多層級優先權合併、具備原始 API 名稱備用方案（fallback）的解析器，以及未解析術語追蹤
* [x] 繁體中文（zh-TW）字典內容目前為部分草稿（`api_terms.json`、`view_sheet_terms.json` 含有範例項目；`builtin_categories.json`、`builtin_parameters_common.json`、`family_terms.json`、`geometry_terms.json`、`parameter_terms.json` 目前仍為空留位符）

### 說明文件 (docs/)

* [x] 入門指南（getting-started）、建置指南（build-guide）、開發藍圖（roadmap）、Revit版本支援（revit-version-support）、AI JSON 綱要（ai-json-schema）、Markdown匯出格式（markdown-export-format）、Excel匯出格式（excel-export-format）、字典貢獻指南（dictionary-contribution-guide）

### 範例 (samples/)

* [x] 提供一個具代表性的範例（圖紙上的視圖裁剪區 Viewport）用以測試所有區段，包含 `samples/json`、`samples/markdown` 與 `samples/excel`

### 安裝封裝 (install/)

* [x] 針對各版本的 `.addin` 指明檔（`install/addin/`）以及多版本相容的 `.bundle` 包（`install/bundle/`，內含 `build-bundle.ps1` 封裝腳本）— 詳見 `install/README.md`

### 尚未開始 (Not started)

* [ ] 於真實 Revit 環境中的實際驗證（由於目前建置環境限制，Revit API 參考雖可正常解析，但尚未在 Revit 軟體內實際執行測試；詳見 `docs/revit-version-support.md`）
* [ ] 用於安裝封裝的 CI/CD 發布流水線（目前仍為手動的本地端腳本）

## 授權條款 (License)

MIT 授權 — 詳見 `LICENSE`。關於本專案所現代化的原始專念（RevitElementBipChecker），其相關的貢獻與鳴謝註記請參閱 `NOTICE.md`。
