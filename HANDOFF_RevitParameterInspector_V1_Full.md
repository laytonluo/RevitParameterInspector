# HANDOFF_RevitParameterInspector_V1_Full

## 0. Document Purpose

This HANDOFF document defines the full V1 implementation scope for an open-source Revit add-in named **RevitParameterInspector**.

The document is intended for AI-assisted implementation by Claude, Copilot, Cursor, or other AI coding agents.

The goal is to build a practical, structured, developer-oriented Revit element context inspection tool, not a full RevitLookup replacement, not an AI chatbot, and not a model automation system.

---

## 1. Project Name

Project Name: **RevitParameterInspector**

Alternative Future Name Candidates:

- RevitContextInspector
- RevitElementContextInspector
- RevitParameterInspector

For V1, use:

**RevitParameterInspector**

Reason:

The project begins as a parameter and element context inspection tool. The name is clear, developer-friendly, and not overly broad.

---

## 2. Project Vision

RevitParameterInspector is an open-source Revit add-in that helps developers, BIM automation engineers, and AI-assisted coding workflows inspect selected Revit elements in a structured, searchable, and AI-readable way.

The project should focus on:

- Selected element inspection
- Parameter inspection
- Type / Instance parameter separation
- Geometry summary
- Location summary
- Relationship summary
- View / Sheet context
- JSON export
- Markdown AI Context export
- Excel export
- Optional Dictionary Engine support

The project should not try to replace RevitLookup.

RevitLookup is already a mature and actively maintained Revit object exploration tool. It is designed for deep object graph exploration, while this project focuses on structured context extraction and AI-readable outputs.

---

## 3. Background

The original RevitElementBipChecker is a lightweight open-source Revit parameter inspection tool. It provides parameter search, parameter snooping, export features, and linked element support. However, its public repository appears to be updated only up to Revit 2023, and its project configuration includes Revit 2018 to Revit 2023 build configurations.

Revit 2024 introduced several important API changes, including ElementId 64-bit support and parameter API changes. `ElementId.IntegerValue` is obsolete in Revit 2024 and `ElementId.Value` should be considered for newer versions.

Revit 2025 moved the Revit API to .NET 8. Revit 2025 and future add-ins need to be rebuilt on .NET 8, so V1 must explicitly handle the net48 / net8.0-windows split.

This project should modernize the inspection concept for Revit 2024, 2025, and 2026, while adding a structured data model suitable for AI-assisted development.

---

## 4. Core Positioning

### 4.1 What This Project Is

RevitParameterInspector is:

- A Revit add-in
- A developer tool
- A structured element context inspector
- A parameter browser
- A JSON / Markdown / Excel exporter
- A dictionary-ready terminology mapping tool
- A future MCP-friendly context extraction foundation

### 4.2 What This Project Is Not

RevitParameterInspector is not:

- A RevitLookup replacement
- A full object graph explorer
- A chatbot
- An AI assistant inside Revit
- A model modification tool
- A QA/QC automation engine
- A project profile automation tool
- A family editor
- A parameter writer
- A cloud service
- A SaaS product
- A licensing system
- A batch model scanner in V1
- A full MCP server in V1

---

## 5. V1 Development Principle

### 5.1 Context-First, Not Tree-First

Do not build the UI around a raw Revit API tree.

Instead, build a normalized data object:

**ElementContextSnapshot**

The selected Revit element should first be converted into an organized context snapshot.

The UI, JSON export, Markdown export, Excel export, and future MCP tools should all use the same snapshot.

### 5.2 Inspector First, Dictionary Later

Dictionary Engine must be optional.

The Inspector must work even if:

- No dictionary exists
- Dictionary JSON is missing
- Dictionary JSON is invalid
- A term has no translation

Fallback rule:

If dictionary mapping is not found, show the raw API name.

### 5.3 Preserve Raw API Names

Never replace the raw API name.

Always preserve:

- API Name
- Revit class name
- BuiltInCategory
- BuiltInParameter
- Parameter name
- API path

Dictionary values should be additional metadata only.

Example:

API Name: `Viewport`  
Localized Name: `視埠`  
Description: `放置於圖紙上的視圖容器`  
Raw API Name must still remain `Viewport`.

### 5.4 Do Not Translate User-Created Parameters

The Dictionary Engine should not translate:

- User-created Shared Parameters
- User-created Project Parameters
- Company-specific parameters
- Project-specific parameters
- Family custom parameters
- Naming convention fields

These should remain as-is.

The Dictionary Engine only targets:

- Revit API terms
- Revit class names
- BuiltInCategory
- BuiltInParameter
- Common Revit concepts

---

## 6. Target Users

Primary users:

- Revit API developers
- BIM automation engineers
- Revit add-in developers
- AI-assisted coding users
- MCP tool builders
- BIM managers who need parameter inspection
- Users who need to understand Revit parameters but are not fully comfortable with English API terminology

Secondary users:

- BIM coordinators
- Advanced Revit users
- Family creators
- Technical BIM teams
- Open-source contributors

---

## 7. V1 Supported Revit Versions

V1 should support:

- Revit 2024
- Revit 2025
- Revit 2026

Recommended target framework strategy:

- Revit 2024: `net48`
- Revit 2025: `net8.0-windows`
- Revit 2026: `net8.0-windows`

Recommended build constants:

- `REVIT2024`
- `REVIT2025`
- `REVIT2026`
- `REVIT2024_OR_GREATER`
- `REVIT2025_OR_GREATER`

The implementation should isolate Revit version differences in compatibility helper classes.

---

## 8. Repository Structure

Recommended GitHub repository structure:

```text
RevitParameterInspector/
├─ src/
│  ├─ RevitParameterInspector.Core/
│  ├─ RevitParameterInspector.Revit/
│  ├─ RevitParameterInspector.UI/
│  ├─ RevitParameterInspector.Export/
│  └─ RevitParameterInspector.Dictionary/
│
├─ dictionary/
│  ├─ zh-TW/
│  │  ├─ api_terms.json
│  │  ├─ view_sheet_terms.json
│  │  ├─ geometry_terms.json
│  │  ├─ parameter_terms.json
│  │  ├─ family_terms.json
│  │  ├─ builtin_categories.json
│  │  └─ builtin_parameters_common.json
│  │
│  └─ en-US/
│     └─ api_terms.json
│
├─ docs/
│  ├─ getting-started.md
│  ├─ revit-version-support.md
│  ├─ ai-json-schema.md
│  ├─ markdown-export-format.md
│  ├─ excel-export-format.md
│  ├─ dictionary-contribution-guide.md
│  ├─ build-guide.md
│  └─ roadmap.md
│
├─ samples/
│  ├─ json/
│  ├─ markdown/
│  └─ excel/
│
├─ install/
│  ├─ addin/
│  └─ bundle/
│
├─ README.md
├─ CONTRIBUTING.md
├─ LICENSE
├─ NOTICE.md
└─ CHANGELOG.md
```

---

## 9. Solution Architecture

### 9.1 RevitParameterInspector.Core

Purpose:

Core data model and application-neutral logic.

Should avoid direct Revit API references if possible.

Contains:

- ElementContextSnapshot
- IdentityInfo
- ClassificationInfo
- ParameterInfoRecord
- GeometryInfo
- LocationInfo
- RelationshipInfo
- ViewContextInfo
- SheetContextInfo
- DictionaryTermInfo
- ExportMetadata
- Common enums
- Validation helpers

### 9.2 RevitParameterInspector.Revit

Purpose:

Revit API integration.

Contains:

- External command
- Ribbon setup
- Selection reader
- Pick element command
- Element context builder
- Parameter reader
- Type parameter reader
- Geometry reader
- Location reader
- Relationship reader
- View / Sheet context reader
- Revit compatibility helpers

This module may reference:

- RevitAPI.dll
- RevitAPIUI.dll

### 9.3 RevitParameterInspector.UI

Purpose:

WPF interface.

Contains:

- MainWindow
- ViewModels
- DataGrid views
- Search box
- Tab navigation
- Detail panel
- JSON preview
- Markdown preview
- Export buttons

UI should be practical, not over-designed.

### 9.4 RevitParameterInspector.Export

Purpose:

Export operations.

Contains:

- JSON exporter
- Markdown exporter
- Excel exporter
- Clipboard AI Context exporter

All export formats must use the same ElementContextSnapshot.

### 9.5 RevitParameterInspector.Dictionary

Purpose:

Optional dictionary mapping layer.

Contains:

- Dictionary loader
- Dictionary resolver
- Term matcher
- Locale support
- Fallback logic
- Unresolved term collector

Dictionary must be non-blocking.

If dictionary loading fails, the app still works.

---

## 10. Main Data Model

### 10.1 ElementContextSnapshot

ElementContextSnapshot is the central normalized object.

Required sections:

- schemaVersion
- generatedAt
- revitVersion
- addinVersion
- document
- identity
- classification
- parameters
- geometry
- location
- relationships
- viewContext
- sheetContext
- dictionary
- raw
- exportMetadata

Purpose:

This object is the single source for:

- UI display
- JSON export
- Markdown export
- Excel export
- Copy AI Context
- Future MCP read-only context

---

## 11. IdentityInfo

Purpose:

Identify the selected element.

Fields:

- elementId
- elementIdString
- uniqueId
- name
- className
- categoryName
- categoryId
- builtInCategory
- familyName
- typeName
- typeId
- documentTitle
- documentPath
- isLinkedElement
- linkedDocumentTitle
- linkedElementId
- apiPath

Notes:

For Revit 2024+, ElementId should be handled as long/string because ElementId storage moved to 64-bit.

---

## 12. ClassificationInfo

Purpose:

Classify what kind of Revit object this is.

Fields:

- elementKind
- categoryType
- isElementType
- isFamilyInstance
- isView
- isSheet
- isViewport
- isTitleBlock
- isAnnotation
- isModelElement
- isDatumElement
- isSystemFamily
- isLoadableFamily
- discipline
- supportedInspectionGroups

Allowed elementKind values:

- ModelElement
- AnnotationElement
- View
- Sheet
- Viewport
- TitleBlock
- ElementType
- Datum
- Material
- Unknown

---

## 13. ParameterInfoRecord

Purpose:

Represent one parameter row.

Fields:

- name
- localizedName
- description
- keywords
- apiPath
- scope
- parameterKind
- storageType
- valueRaw
- valueDisplay
- unitType
- dataType
- groupName
- groupTypeId
- builtInParameter
- builtInParameterId
- isShared
- isProjectParameter
- isFamilyParameter
- isBuiltIn
- isReadOnly
- isWritable
- hasValue
- sourceElementId
- sourceElementKind
- dictionaryStatus

Scope values:

- Instance
- Type
- System
- Unknown

ParameterKind values:

- BuiltInParameter
- SharedParameter
- ProjectParameter
- FamilyParameter
- Unknown

Important:

Do not translate custom user parameter names.

Dictionary fields are optional.

---

## 14. GeometryInfo

Purpose:

Provide lightweight geometry summary.

Fields:

- hasBoundingBox
- boundingBoxMin
- boundingBoxMax
- boundingBoxCenter
- boundingBoxSize
- widthX
- depthY
- heightZ
- solidCount
- curveCount
- faceCount
- edgeCount
- meshCount
- geometryAvailable
- geometryReadStatus
- apiPath

V1 performance rule:

Geometry extraction must be lightweight.

The tool should prioritize bounding box and simple counts.

Heavy geometry traversal should be optional or deferred.

---

## 15. LocationInfo

Purpose:

Store placement data.

Fields:

- locationType
- hasLocation
- point
- rotationRadians
- rotationDegrees
- curveStartPoint
- curveEndPoint
- curveLength
- curveDirection
- levelId
- levelName
- offset
- apiPath

LocationType values:

- LocationPoint
- LocationCurve
- None
- Unsupported
- Unknown

Important distinction:

Geometry and Location are different sections.

Geometry describes shape and range.

Location describes placement and reference.

---

## 16. RelationshipInfo

Purpose:

Store related Revit objects.

Fields:

- typeElementId
- typeName
- familyId
- familyName
- hostElementId
- hostName
- levelId
- levelName
- roomId
- roomName
- spaceId
- spaceName
- materialIds
- viewOwnerId
- viewOwnerName
- sheetId
- sheetNumber
- sheetName
- groupId
- assemblyId
- designOptionId
- phaseCreatedId
- phaseDemolishedId
- linkedDocumentTitle
- linkedElementId

V1 should display relationships.

V1 does not need to fully implement recursive inspection of related elements.

Future action:

- Inspect related element
- Copy related element id
- Export relationship graph

---

## 17. ViewContextInfo

Used when:

- selected element is a View
- selected element belongs to a View
- selected element is Viewport
- selected element is placed on Sheet

Fields:

- viewId
- viewName
- viewType
- scale
- cropBoxActive
- cropBoxVisible
- cropBoxMin
- cropBoxMax
- cropBoxSize
- viewTemplateId
- viewTemplateName
- detailLevel
- displayStyle
- discipline
- associatedLevelId
- associatedLevelName

This section is important for future drawing production workflows but should remain read-only in V1.

---

## 18. SheetContextInfo

Used when selected element is:

- ViewSheet
- Viewport
- TitleBlock
- ScheduleSheetInstance
- View placed on sheet

Fields:

- sheetId
- sheetNumber
- sheetName
- titleBlockElementId
- titleBlockFamilyName
- titleBlockTypeName
- viewportIds
- placedViewIds
- scheduleSheetInstanceIds
- revisionIds
- paperSizeName
- paperWidth
- paperHeight

Important terminology:

- ViewSheet = 圖紙
- Viewport = 視埠
- TitleBlock = 圖框

---

## 19. DictionaryTermInfo

Purpose:

Represent one localized dictionary entry.

Fields:

- termKey
- apiName
- localizedName
- locale
- description
- keywords
- category
- priority
- status
- source
- lastUpdated
- contributor
- notes

Status values:

- Default
- Reviewed
- CommunitySuggested
- Deprecated
- NeedsReview
- NotFound

---

## 20. Dictionary Engine Scope

### 20.1 V1 Dictionary Goals

The Dictionary Engine should provide optional terminology mapping.

V1 supports:

- API terms
- View / Sheet terms
- Geometry terms
- Parameter terms
- Family terms
- BuiltInCategory terms
- Common BuiltInParameter terms

V1 does not support automatic translation of user-defined parameters.

### 20.2 Dictionary File Strategy

Dictionary should be file-based.

Recommended format:

- JSON

Recommended folder:

```text
dictionary/zh-TW/
```

Initial files:

- api_terms.json
- view_sheet_terms.json
- geometry_terms.json
- parameter_terms.json
- family_terms.json
- builtin_categories.json
- builtin_parameters_common.json

### 20.3 Dictionary Loading Priority

Priority order:

1. User override dictionary
2. Community dictionary
3. Built-in default dictionary
4. Raw API fallback

### 20.4 Dictionary Fallback Rule

If no dictionary mapping exists:

- localizedName = null
- description = null
- keywords = empty
- dictionaryStatus = NotFound
- UI displays raw API name

### 20.5 Default zh-TW Dictionary Strategy

The default zh-TW dictionary is a draft.

It should be clearly documented:

- Default terms are not final.
- Contributors can submit corrections.
- Taiwan BIM/CAD industry terms may differ from Autodesk official translations.
- English API names must not be removed.
- zh-TW and zh-CN dictionaries should be separate.

---

## 21. Initial zh-TW Dictionary Terms

Initial terms should include at least:

| API Name | zh-TW | Note |
|---|---|---|
| Element | 元件 | General Revit element |
| Category | 類別 | Revit category |
| Document | 專案文件 | RVT/RFA document |
| Parameter | 參數 | Element parameter |
| Family | 族群 | Revit Family |
| FamilySymbol | 族群類型 | Type object |
| FamilyInstance | 族群實例 | Placed family instance |
| ElementType | 元件類型 | Type-level element |
| View | 視圖 | General view |
| ViewPlan | 平面視圖 | Floor plan / ceiling plan |
| ViewSection | 剖面視圖 | Section view |
| ViewElevation | 立面視圖 | Elevation view |
| View3D | 3D視圖 | 3D view |
| ViewSheet | 圖紙 | Sheet container |
| Viewport | 視埠 | Taiwan CAD/BIM usage |
| TitleBlock | 圖框 | Sheet title block |
| Schedule | 明細表 | Schedule view |
| Legend | 圖例 | Legend view |
| CropBox | 裁切框 | View crop region |
| ScopeBox | 範圍框 | Scope box |
| BoundingBox | 外包框 | Element bounding box |
| LocationPoint | 定位點 | Point-based location |
| LocationCurve | 定位線 | Curve-based location |
| XYZ | 座標點 | Revit XYZ |
| Host | 宿主元件 | Host relationship |
| Level | 樓層 | Level datum |
| Workset | 工作集 | Worksharing workset |
| Phase | 階段 | Phase data |
| Material | 材料 | Material |
| Room | 房間 | Architectural room |
| Space | 空間 | MEP space |

---

## 22. UI Specification

### 22.1 Main Layout

The main window should use this structure:

- Top: Element Summary Bar
- Left: Navigation List
- Center: Data Grid / Content Panel
- Right: Detail Panel
- Bottom: Export / Copy Actions

### 22.2 Required Navigation Sections

V1 fixed tabs:

1. Summary
2. Parameters
3. Geometry
4. Location
5. Relationships
6. View / Sheet Context
7. Dictionary
8. Raw JSON
9. AI Context

Do not add:

- Dashboard
- Analytics
- QA/QC
- Project Profile
- Model Automation
- Charts
- AI Chat

These are out of scope for V1.

---

## 23. Summary Section

Display:

- ElementId
- UniqueId
- ClassName
- Category
- BuiltInCategory
- Name
- Family
- Type
- Document
- Is Linked Element

Purpose:

Give the user immediate understanding of the selected element.

---

## 24. Parameters Section

### 24.1 Required Parameter Tabs

Tabs:

- All
- Instance
- Type
- BuiltIn
- Shared
- Project
- Writable
- ReadOnly

### 24.2 Required Columns

Columns:

- Name
- Localized Name
- Scope
- Value Display
- Value Raw
- Storage Type
- BuiltInParameter
- Parameter Kind
- Is ReadOnly
- Has Value
- API Path
- Description

### 24.3 Search

Search should support:

- Raw parameter name
- Localized name
- BuiltInParameter
- Value text
- Description
- Keyword
- API path

---

## 25. Geometry Section

Display:

- BoundingBox Minimum
- BoundingBox Maximum
- BoundingBox Center
- BoundingBox Size
- Width X
- Depth Y
- Height Z
- Geometry availability
- Solid count
- Curve count
- Face count
- Edge count

If detailed geometry cannot be read, show a graceful status message.

Do not crash Revit.

---

## 26. Location Section

Display:

- Location type
- Point
- Rotation
- Curve start
- Curve end
- Curve length
- Direction
- Level
- Offset
- API path

LocationPoint and LocationCurve should be visually separated.

---

## 27. Relationships Section

Display:

- Type
- Family
- Host
- Level
- Room
- Space
- View
- Sheet
- Phase
- Workset
- Design Option
- Link Document

V1 can display IDs and names only.

Recursive inspect is optional.

---

## 28. View / Sheet Context Section

This section should show view-related information when available.

For View:

- View name
- View type
- Scale
- CropBox
- View template
- Detail level
- Discipline
- Associated level

For Sheet:

- Sheet number
- Sheet name
- TitleBlock
- Viewports
- Placed views
- Schedule instances
- Revisions

For Viewport:

- Viewport id
- Related view
- Related sheet
- Viewport position if available

---

## 29. Dictionary Section

Display dictionary mapping for selected term or current row.

Fields:

- API Name
- Localized Name
- Locale
- Description
- Keywords
- Status
- Source
- Notes

Also provide unresolved terms list.

Purpose:

Help contributors identify missing dictionary entries.

---

## 30. Raw JSON Section

Display formatted ElementContextSnapshot JSON.

Actions:

- Copy JSON
- Export JSON

This view is for developers and AI workflows.

---

## 31. AI Context Section

Display AI-readable Markdown.

Actions:

- Copy AI Context
- Export Markdown

This should be optimized for pasting into:

- Claude
- Copilot
- Cursor
- ChatGPT
- MCP tools

---

## 32. Export Specification

### 32.1 JSON Export

Primary machine-readable format.

File name pattern:

```text
element-context_{documentName}_{elementId}_{timestamp}.json
```

JSON must include:

- Full ElementContextSnapshot
- Revit version
- Add-in version
- Schema version
- Dictionary fields
- Raw API names
- API paths

### 32.2 Markdown Export

Primary AI-readable human format.

File name pattern:

```text
element-context_{documentName}_{elementId}_{timestamp}.md
```

Markdown should include:

- Element Summary
- Classification
- Parameters
- Writable parameters
- Read-only parameters
- Geometry
- Location
- Relationships
- View / Sheet Context
- Dictionary Notes
- Suggested API Paths

### 32.3 Excel Export

BIM user-friendly table format.

File name pattern:

```text
element-context_{documentName}_{elementId}_{timestamp}.xlsx
```

Required sheets:

1. Summary
2. Parameters_All
3. Parameters_Instance
4. Parameters_Type
5. Geometry
6. Location
7. Relationships
8. Dictionary
9. Raw_Metadata

### 32.4 CSV Export

CSV is not required for V1.

Reason:

- CSV does not handle nested context well.
- Excel is better for BIM users.
- JSON is better for machines.
- Markdown is better for AI and humans.

CSV may be added later if users request it.

---

## 33. Copy AI Context Feature

The UI must provide one-click:

**Copy AI Context**

This copies Markdown to clipboard.

The copied content must be concise but complete enough for AI coding tools.

Required sections:

- Selected Element Summary
- Parameter Summary
- Geometry Summary
- Location Summary
- Relationship Summary
- API Path Notes
- Dictionary Notes

---

## 34. Selection Workflow

### 34.1 Current Selection

If one element is selected:

- Open inspector directly.

If multiple elements are selected:

- Show simple element list.
- Let user choose one element.

If no element is selected:

- Show message and provide Pick Element option.

### 34.2 Pick Element

Provide command:

- Pick Element
- Inspect picked element

### 34.3 Linked Element

Linked element support is desired but not required to be perfect in first implementation.

If linked inspection fails:

Show:

“Linked element inspection is not available or failed in this version.”

Do not crash.

---

## 35. Error Handling

Handle:

- No selection
- Invalid selection
- Unsupported element
- Null parameter
- Parameter read failure
- Type parameter read failure
- Geometry read failure
- Location read failure
- Relationship read failure
- Dictionary missing
- Dictionary invalid JSON
- Export path failure
- Clipboard failure

Error messages should be human-readable.

Revit must not crash.

---

## 36. Performance Requirements

V1 should prioritize:

- Fast parameter reading
- Fast summary reading
- Fast bounding box reading
- Lightweight UI

Avoid:

- Full geometry traversal by default
- Recursive relationship graph traversal
- Batch scanning
- Long UI blocking operation

If operation may take time, show loading status.

---

## 37. Logging

Provide simple local logging.

Log:

- Add-in start
- Revit version
- Add-in version
- Element inspected
- Export action
- Dictionary load result
- Exceptions

Do not log sensitive model content by default.

---

## 38. Open Source Strategy

### 38.1 License

Recommended license:

MIT License

If using or referencing old open-source code, preserve original license and attribution.

### 38.2 README Must Include

README should include:

- What this project is
- What this project is not
- Supported Revit versions
- Installation guide
- Build guide
- Export examples
- Dictionary contribution guide
- Relationship with RevitLookup
- License notice
- Original inspiration notice

### 38.3 Positioning Statement

Include this statement in README:

RevitParameterInspector is not intended to replace RevitLookup. RevitLookup is a powerful general-purpose Revit database exploration tool. RevitParameterInspector focuses on structured element context extraction, parameter inspection, AI-readable export, and dictionary-ready terminology mapping.

---

## 39. Dictionary Contribution Rules

Contributors should follow:

1. Do not translate user-created parameters.
2. Keep API names unchanged.
3. Prefer industry-accepted terminology.
4. Add aliases when terminology varies.
5. Use `NeedsReview` when unsure.
6. Include notes for region-specific usage.
7. Keep zh-TW separate from zh-CN.
8. Do not remove English API terms.
9. Do not force official translation if industry usage differs.
10. Use Pull Request for correction.

Example:

`Viewport` should be mapped to `視埠` for zh-TW because this is common Taiwan CAD/BIM usage.

---

## 40. Implementation Priority

Claude should implement in this order:

1. Create repo structure.
2. Create solution structure.
3. Set up Revit 2024 / 2025 / 2026 build configurations.
4. Create Core data models.
5. Create Revit external command.
6. Implement selected element reader.
7. Implement IdentityInfo builder.
8. Implement ClassificationInfo builder.
9. Implement Parameter reader.
10. Implement Type parameter reader.
11. Implement GeometryInfo builder.
12. Implement LocationInfo builder.
13. Implement RelationshipInfo builder.
14. Implement ViewContextInfo builder.
15. Implement SheetContextInfo builder.
16. Implement WPF MainWindow.
17. Implement parameter DataGrid.
18. Implement search.
19. Implement JSON export.
20. Implement Markdown export.
21. Implement Excel export.
22. Implement Copy AI Context.
23. Implement Dictionary loader.
24. Implement dictionary fallback.
25. Add default zh-TW dictionary.
26. Add README.
27. Add CONTRIBUTING.
28. Add sample exports.
29. Prepare V1 release.

---

## 41. V1 Acceptance Criteria

V1 is complete when:

1. Add-in loads in Revit 2024.
2. Add-in loads in Revit 2025.
3. Add-in loads in Revit 2026.
4. User can inspect selected element.
5. User can pick element and inspect it.
6. Summary section works.
7. Parameters section works.
8. Instance / Type parameters are separated.
9. Geometry section shows bounding box.
10. Location section shows LocationPoint or LocationCurve.
11. Relationships section shows basic related elements.
12. View / Sheet Context section works when relevant.
13. Search works.
14. JSON export works.
15. Markdown export works.
16. Excel export works.
17. Copy AI Context works.
18. Default zh-TW dictionary loads.
19. Missing dictionary terms fallback to raw API names.
20. User-created parameters are not translated.
21. Invalid dictionary file does not crash add-in.
22. README explains purpose.
23. CONTRIBUTING explains dictionary contributions.
24. MIT license exists.
25. Project is ready for GitHub open-source release.

---

## 42. V1 Non-Goals

Claude must not implement these in V1:

- Full RevitLookup clone
- Full MCP server
- AI chatbot
- OpenAI / Claude API integration
- Model modification
- Parameter writing
- Family editing
- QA/QC automation
- Project Profile Framework
- Drawing Production Matrix
- Cloud backend
- User login
- SaaS licensing
- Batch full-model scan
- Automatic translation of custom parameters
- Complex charts or analytics dashboard

---

## 43. Future Roadmap

### V1.1

- Improve dictionary coverage
- Add unresolved term export
- Add more BuiltInCategory mappings
- Add more BuiltInParameter mappings
- Add inspect related element action

### V1.2

- Batch inspect selected elements
- Category schema export
- Family type schema export
- Tag candidate parameter export
- Export unresolved dictionary terms

### V2.0

- Read-only MCP bridge
- Local context provider
- AI workflow integration
- Project Profile candidate extraction
- Drawing Production Matrix support
- Community dictionary package versioning

---

## 44. Claude Implementation Notes

Claude must follow these rules:

1. Build the Inspector first.
2. Keep Dictionary Engine optional.
3. Do not make Dictionary required for UI.
4. Do not translate custom user parameters.
5. Preserve raw API names.
6. Preserve API paths.
7. Use ElementContextSnapshot as the single source of truth.
8. Keep Revit API calls inside Revit-specific module.
9. Keep Core module clean.
10. Avoid scope creep.
11. Prioritize working V1 over perfect architecture.
12. Avoid implementing full MCP in V1.
13. Avoid implementing model modification.
14. Avoid implementing AI chat.
15. Avoid complex UI.
16. Keep export formats practical.
17. Ensure Revit does not crash on errors.
18. Write clear README and contribution docs.

---

## 45. Final V1 Definition

The V1 product should be summarized as:

**RevitParameterInspector V1 is a read-only, structured Revit element context inspector that helps developers inspect parameters, geometry, location, and relationships, then export the result as JSON, Markdown, Excel, or AI-readable context. Dictionary support is optional, file-based, community-editable, and non-blocking.**

---

## 46. Recommended First Prompt for Claude

Use the following prompt after providing this HANDOFF document:

```text
Please implement Step 1 only based on HANDOFF_RevitParameterInspector_V1_Full.

Step 1 scope:
- Create the GitHub repository structure.
- Create the Visual Studio solution structure.
- Create project skeletons for Core, Revit, UI, Export, and Dictionary.
- Create initial Core data model classes for ElementContextSnapshot, IdentityInfo, ClassificationInfo, ParameterInfoRecord, GeometryInfo, LocationInfo, RelationshipInfo, ViewContextInfo, SheetContextInfo, DictionaryTermInfo, and ExportMetadata.
- Do not implement full WPF UI yet.
- Do not implement Dictionary Engine yet.
- Do not implement MCP.
- Do not implement model modification.
- Keep the solution buildable as much as possible.
```
