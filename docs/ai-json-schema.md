# AI / JSON Export Schema

`JsonExporter.Serialize` / `ExportToFile` writes the entire `ElementContextSnapshot` (Core
model) as indented JSON via `System.Text.Json`, using default reflection-based serialization -
**PascalCase property names, and enums serialized as their underlying integer, not their name.**
Nullable fields are always emitted as `null` rather than omitted. See
`samples/json/element-context_Sample_Project.rvt_305001_20260709-101500.json` for a full worked
example (a Viewport placed on a sheet, showing every section populated).

This is the authoritative machine-readable format - the same `ElementContextSnapshot` object
backs the UI, Markdown export, Excel export, and Copy AI Context, so nothing here is
Markdown/Excel-specific. The Core model source (`src/RevitParameterInspector.Core/Models/`) is
the ground truth if this doc and the code ever disagree.

## Top level

| Field | Type | Notes |
|---|---|---|
| `SchemaVersion` | string | Currently `"1.0.0"` |
| `GeneratedAt` | ISO 8601 datetime | UTC |
| `RevitVersion` | string? | e.g. `"2025"` |
| `AddinVersion` | string? | Assembly version of `RevitParameterInspector.Revit` |
| `Document` | object? | See [Document](#document) |
| `Identity` | object? | See [Identity](#identity) |
| `Classification` | object? | See [Classification](#classification) |
| `Parameters` | array | See [Parameters](#parameters); instance + type parameters combined |
| `Geometry` | object? | See [Geometry](#geometry) |
| `Location` | object? | See [Location](#location) |
| `Relationships` | object? | See [Relationships](#relationships) |
| `ViewContext` | object? | Null unless the element is a View, Viewport, or placed in a view - see [View Context](#view-context) |
| `SheetContext` | object? | Null unless the element is a ViewSheet, Viewport, TitleBlock, or placed on a sheet - see [Sheet Context](#sheet-context) |
| `Dictionary` | array | Distinct dictionary terms resolved while building this snapshot - see [Dictionary Term](#dictionary-term) |
| `UnresolvedDictionaryTerms` | string array | Distinct API names looked up with no dictionary mapping |
| `Raw` | object (string→string) | Escape hatch for data that doesn't fit elsewhere yet; empty today |
| `ExportMetadata` | object? | Set by whichever exporter wrote the file - see [Export Metadata](#export-metadata) |

## Document

| Field | Type |
|---|---|
| `Title` | string? |
| `PathName` | string? |
| `IsWorkshared` | bool |
| `IsLinked` | bool |
| `RevitProductName` | string? |
| `RevitBuildNumber` | string? |

## Identity

| Field | Type | Notes |
|---|---|---|
| `ElementId` | long | Revit 2024+ 64-bit id |
| `ElementIdString` | string? | |
| `UniqueId` | string? | |
| `Name` | string? | |
| `ClassName` | string? | Raw .NET type name, e.g. `"Viewport"` |
| `CategoryName` | string? | |
| `CategoryId` | long? | |
| `BuiltInCategory` | string? | Raw `BuiltInCategory` enum name, e.g. `"OST_Doors"` |
| `FamilyName` | string? | |
| `TypeName` | string? | |
| `TypeId` | long? | |
| `DocumentTitle` | string? | |
| `DocumentPath` | string? | |
| `IsLinkedElement` | bool | Linked-element inspection is not implemented; always `false` today |
| `LinkedDocumentTitle` | string? | |
| `LinkedElementId` | long? | |
| `ApiPath` | string? | |
| `ClassNameLocalized` | string? | Dictionary lookup on `ClassName`; additive only |
| `ClassNameDescription` | string? | |
| `ClassNameDictionaryStatus` | int (`DictionaryStatus`) | |
| `BuiltInCategoryLocalized` | string? | Dictionary lookup on `BuiltInCategory`; additive only |
| `BuiltInCategoryDescription` | string? | |
| `BuiltInCategoryDictionaryStatus` | int (`DictionaryStatus`) | |

## Classification

| Field | Type | Notes |
|---|---|---|
| `ElementKind` | int (`ElementKind`) | See [enum: ElementKind](#elementkind) |
| `CategoryType` | int (`CategoryType`) | See [enum: CategoryType](#categorytype) |
| `IsElementType` | bool | |
| `IsFamilyInstance` | bool | |
| `IsView` | bool | |
| `IsSheet` | bool | |
| `IsViewport` | bool | |
| `IsTitleBlock` | bool | |
| `IsAnnotation` | bool | |
| `IsModelElement` | bool | |
| `IsDatumElement` | bool | |
| `IsSystemFamily` | bool | |
| `IsLoadableFamily` | bool | |
| `Discipline` | int (`Discipline`) | See [enum: Discipline](#discipline) |
| `SupportedInspectionGroups` | string array | Subset of `Identity`/`Parameters`/`Geometry`/`Location`/`Relationships`/`ViewContext`/`SheetContext` relevant to this element |
| `ElementKindLocalized` | string? | Dictionary lookup mapped from `ElementKind` (e.g. `Sheet` → `"ViewSheet"`); additive only |
| `ElementKindDescription` | string? | |
| `ElementKindDictionaryStatus` | int (`DictionaryStatus`) | |

## Parameters

Each array entry:

| Field | Type | Notes |
|---|---|---|
| `Name` | string? | Raw Revit parameter name; never overwritten by dictionary data |
| `LocalizedName` | string? | Only set for `BuiltInParameter` entries; Shared/Project/Family parameters are never translated |
| `Description` | string? | |
| `Keywords` | string array | |
| `ApiPath` | string? | |
| `Scope` | int (`ParameterScope`) | See [enum: ParameterScope](#parameterscope) |
| `ParameterKind` | int (`ParameterKind`) | See [enum: ParameterKind](#parameterkind) |
| `StorageType` | string? | Raw Revit `StorageType` name |
| `ValueRaw` | string? | |
| `ValueDisplay` | string? | |
| `UnitType` | string? | |
| `DataType` | string? | |
| `GroupName` | string? | |
| `GroupTypeId` | string? | |
| `BuiltInParameter` | string? | Raw `BuiltInParameter` enum name, e.g. `"ALL_MODEL_MARK"` |
| `BuiltInParameterId` | long? | |
| `IsShared` | bool | |
| `IsProjectParameter` | bool | |
| `IsFamilyParameter` | bool | |
| `IsBuiltIn` | bool | |
| `IsReadOnly` | bool | |
| `IsWritable` | bool | |
| `HasValue` | bool | |
| `SourceElementId` | long? | Instance element id, or the type element id for type-scope rows |
| `SourceElementKind` | int? (`ElementKind`) | |
| `DictionaryStatus` | int (`DictionaryStatus`) | |

## Geometry

| Field | Type |
|---|---|
| `HasBoundingBox` | bool |
| `BoundingBoxMin` / `BoundingBoxMax` / `BoundingBoxCenter` / `BoundingBoxSize` | [Point3D](#point3d)? |
| `WidthX` / `DepthY` / `HeightZ` | double? |
| `SolidCount` / `CurveCount` / `FaceCount` / `EdgeCount` / `MeshCount` | int |
| `GeometryAvailable` | bool |
| `GeometryReadStatus` | int (`GeometryReadStatus`) - see [enum](#geometryreadstatus) |
| `ApiPath` | string? |

## Location

| Field | Type |
|---|---|
| `LocationType` | int (`LocationType`) - see [enum](#locationtype) |
| `HasLocation` | bool |
| `Point` | [Point3D](#point3d)? |
| `RotationRadians` / `RotationDegrees` | double? |
| `CurveStartPoint` / `CurveEndPoint` | [Point3D](#point3d)? |
| `CurveLength` | double? |
| `CurveDirection` | [Point3D](#point3d)? |
| `LevelId` | long? |
| `LevelName` | string? |
| `Offset` | double? |
| `ApiPath` | string? |

## Relationships

| Field | Type |
|---|---|
| `TypeElementId` / `TypeName` | long? / string? |
| `FamilyId` / `FamilyName` | long? / string? |
| `HostElementId` / `HostName` | long? / string? |
| `LevelId` / `LevelName` | long? / string? |
| `RoomId` / `RoomName` | long? / string? |
| `SpaceId` / `SpaceName` | long? / string? |
| `MaterialIds` | long array |
| `ViewOwnerId` / `ViewOwnerName` | long? / string? |
| `SheetId` / `SheetNumber` / `SheetName` | long? / string? / string? |
| `GroupId` | long? |
| `AssemblyId` | long? |
| `DesignOptionId` | long? |
| `PhaseCreatedId` / `PhaseDemolishedId` | long? |
| `LinkedDocumentTitle` / `LinkedElementId` | string? / long? |

## View Context

Null unless the element is a View, a Viewport, or is owned by/placed in a view.

| Field | Type |
|---|---|
| `ViewId` | long? |
| `ViewName` | string? |
| `ViewType` | string? |
| `Scale` | double? |
| `CropBoxActive` / `CropBoxVisible` | bool |
| `CropBoxMin` / `CropBoxMax` / `CropBoxSize` | [Point3D](#point3d)? |
| `ViewTemplateId` / `ViewTemplateName` | long? / string? |
| `DetailLevel` | string? |
| `DisplayStyle` | string? |
| `Discipline` | int (`Discipline`) |
| `AssociatedLevelId` / `AssociatedLevelName` | long? / string? |

## Sheet Context

Null unless the element is a ViewSheet, a Viewport, a TitleBlock, or otherwise placed on a
sheet.

| Field | Type |
|---|---|
| `SheetId` / `SheetNumber` / `SheetName` | long? / string? / string? |
| `TitleBlockElementId` / `TitleBlockFamilyName` / `TitleBlockTypeName` | long? / string? / string? |
| `ViewportIds` / `PlacedViewIds` / `ScheduleSheetInstanceIds` / `RevisionIds` | long arrays |
| `PaperSizeName` / `PaperWidth` / `PaperHeight` | string? / double? / double? - not populated yet (deferred; see `SheetContextInfoBuilder`) |

## Dictionary Term

Each `Dictionary` array entry is a term the engine successfully resolved somewhere while
building this snapshot (Identity's ClassName/BuiltInCategory, Classification's ElementKind, or
a Parameter's BuiltInParameter):

| Field | Type |
|---|---|
| `TermKey` | string? |
| `ApiName` | string? |
| `LocalizedName` | string? |
| `Locale` | string? |
| `Description` | string? |
| `Keywords` | string array |
| `Category` | string? |
| `Priority` | int? |
| `Status` | int (`DictionaryStatus`) |
| `Source` | string? |
| `LastUpdated` | string? |
| `Contributor` | string? |
| `Notes` | string? |

## Export Metadata

Set by whichever exporter (`JsonExporter`/`MarkdownExporter`/`ExcelExporter`) wrote the file,
overwriting anything set by a previous export of the same in-memory snapshot:

| Field | Type |
|---|---|
| `ExportFormat` | string? (`"Json"`, `"Markdown"`, or `"Excel"`) |
| `FileName` | string? |
| `ExportedAt` | datetime? |
| `ExportedBy` | string? - not populated yet |
| `ToolName` | string? (`"RevitParameterInspector"`) |
| `ToolVersion` | string? |
| `Notes` | string? |

## Point3D

| Field | Type |
|---|---|
| `X` / `Y` / `Z` | double |

## Enums

Serialized as their zero-based declaration order, **not** as a string:

#### ElementKind
`0` ModelElement · `1` AnnotationElement · `2` View · `3` Sheet · `4` Viewport ·
`5` TitleBlock · `6` ElementType · `7` Datum · `8` Material · `9` Unknown

#### CategoryType
`0` Model · `1` Annotation · `2` Internal · `3` AnalyticalModel · `4` Invalid · `5` Unknown

#### Discipline
`0` Architectural · `1` Structural · `2` Mechanical · `3` Electrical · `4` Plumbing ·
`5` Coordination · `6` Unknown

#### ParameterScope
`0` Instance · `1` Type · `2` System · `3` Unknown

#### ParameterKind
`0` BuiltInParameter · `1` SharedParameter · `2` ProjectParameter · `3` FamilyParameter ·
`4` Unknown

#### LocationType
`0` LocationPoint · `1` LocationCurve · `2` None · `3` Unsupported · `4` Unknown

#### GeometryReadStatus
`0` Ok · `1` NotAttempted · `2` Unsupported · `3` Failed

#### DictionaryStatus
`0` Default · `1` Reviewed · `2` CommunitySuggested · `3` Deprecated · `4` NeedsReview ·
`5` NotFound

If you're consuming this JSON from an AI tool or MCP-style integration, decode these against
the tables above rather than assuming the numbers are stable across schema versions - they are
declaration order, so inserting a new enum value in the middle of `src/RevitParameterInspector.
Core/Models/Enums.cs` would renumber everything after it.
