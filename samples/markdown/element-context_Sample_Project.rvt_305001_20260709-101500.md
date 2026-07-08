# Element Context: Viewport (305001)

## Summary
- **Element Id**: 305001
- **Unique Id**: 8f2c1a90-1e3b-4b7a-9c2d-0a1b2c3d0001-000a1234
- **Class Name**: Viewport
- **Category**: Viewports (OST_Viewports)
- **Name**: _(none)_
- **Family**: _(none)_
- **Type**: _(none)_
- **Document**: Sample Project.rvt
- **Is Linked Element**: False

## Classification
- **Element Kind**: Viewport
- **Category Type**: Annotation
- **Discipline**: Unknown
- **Is Element Type**: False
- **Is Family Instance**: False
- **Is View / Sheet / Viewport / Title Block**: False / False / True / False
- **Is Model Element / Annotation / Datum**: False / True / False
- **Is System Family / Loadable Family**: False / False
- **Supported Inspection Groups**: Identity, Parameters, Geometry, Location, Relationships, ViewContext, SheetContext

## Parameters
### Writable Parameters (3)
| Name | Scope | Value | Storage Type | Built-in Parameter |
|---|---|---|---|---|
| Detail Number | Instance | 1 | String | VIEWPORT_DETAIL_NUMBER |
| Title on Sheet | Instance | FIRST FLOOR PLAN | String | VIEWPORT_VIEW_NAME_OVERRIDE |
| Sheet Discipline Code | Instance | A | String |  |

### Read-only Parameters (0)
_None._

## Geometry
- **Has Bounding Box**: True
- **Bounding Box Min**: (0.5, 0.3, 0)
- **Bounding Box Max**: (1.5, 1.1, 0)
- **Bounding Box Center**: (1, 0.7, 0)
- **Size (W x D x H)**: 1 x 0.8 x 0
- **Solid / Curve / Face / Edge / Mesh Count**: 0 / 0 / 0 / 0 / 0
- **Geometry Read Status**: Ok

## Location
- **Location Type**: LocationPoint
- **Point**: (1, 0.7, 0)
- **Rotation (deg)**: _(none)_
- **Curve Start -> End**:  -> 
- **Curve Length**: _(none)_
- **Level**: _(none)_

## Relationships
- **Type**: _(none)_
- **Family**: _(none)_
- **Host**: _(none)_
- **Level**: _(none)_
- **Room**: _(none)_
- **Space**: _(none)_
- **View Owner**: Level 1 - Floor Plan
- **Sheet**: A101 - First Floor Plan
- **Materials**: _(none)_

## View / Sheet Context
### View Context
- **View**: Level 1 - Floor Plan
- **View Type**: FloorPlan
- **Scale**: 100
- **Crop Box Active / Visible**: True / True
- **Crop Box Min -> Max**: (0, 0, 0) -> (150, 100, 0)
- **View Template**: Architectural Plan
- **Detail Level**: Fine
- **Display Style**: Shading
- **Discipline**: Architectural
- **Associated Level**: Level 1

### Sheet Context
- **Sheet**: A101 - First Floor Plan
- **Title Block**: A1 Metric - A1
- **Viewports**: 305001
- **Placed Views**: 250010
- **Schedule Instances**: _(none)_
- **Revisions**: 1, 2

## Dictionary Notes
- **Viewport**: 視埠 (Reviewed)
- **Unresolved Terms**: OST_Viewports, VIEWPORT_DETAIL_NUMBER, VIEWPORT_VIEW_NAME_OVERRIDE

## Suggested API Paths
- `Autodesk.Revit.DB.Viewport`
- `Element.GetOrderedParameters() / ElementType.GetOrderedParameters()`
- `Element.get_BoundingBox(null) / Element.get_Geometry(Options)`
- `Element.Location as LocationPoint`
