using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Export;

/// <summary>
/// Markdown export: the primary AI-readable human format. Sections that aren't implemented
/// yet (Dictionary) are shown with an explicit "not available" note rather than omitted
/// silently or crashing. See HANDOFF Section 32.2.
/// </summary>
public static class MarkdownExporter
{
    public static string Build(ElementContextSnapshot snapshot)
    {
        var sb = new StringBuilder();
        var identity = snapshot.Identity;

        sb.AppendLine($"# Element Context: {identity?.ClassName ?? "Unknown"} ({identity?.ElementIdString ?? "?"})");
        sb.AppendLine();

        AppendSummary(sb, snapshot);
        AppendClassification(sb, snapshot);
        AppendParameters(sb, snapshot);
        AppendGeometry(sb, snapshot);
        AppendLocation(sb, snapshot);
        AppendRelationships(sb, snapshot);
        AppendViewSheetContext(sb, snapshot);
        AppendDictionaryNotes(sb, snapshot);
        AppendSuggestedApiPaths(sb, snapshot);

        return sb.ToString();
    }

    /// <summary>Writes the snapshot to the exact <paramref name="filePath"/> given (see <see cref="ExportFileNaming"/> for a suggested name/path).</summary>
    public static string ExportToFile(ElementContextSnapshot snapshot, string filePath)
    {
        var timestamp = DateTimeOffset.UtcNow;

        snapshot.ExportMetadata = new ExportMetadata
        {
            ExportFormat = "Markdown",
            FileName = Path.GetFileName(filePath),
            ExportedAt = timestamp,
            ToolName = "RevitParameterInspector",
            ToolVersion = snapshot.AddinVersion,
        };

        File.WriteAllText(filePath, Build(snapshot));
        return filePath;
    }

    private static void AppendSummary(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        var identity = snapshot.Identity;
        sb.AppendLine("## Summary");
        sb.AppendLine(MarkdownFormat.Bullet("Element Id", identity?.ElementIdString));
        sb.AppendLine(MarkdownFormat.Bullet("Unique Id", identity?.UniqueId));
        sb.AppendLine(MarkdownFormat.Bullet("Class Name", identity?.ClassName));
        sb.AppendLine(MarkdownFormat.Bullet("Category", CombineCategory(identity)));
        sb.AppendLine(MarkdownFormat.Bullet("Name", identity?.Name));
        sb.AppendLine(MarkdownFormat.Bullet("Family", identity?.FamilyName));
        sb.AppendLine(MarkdownFormat.Bullet("Type", identity?.TypeName));
        sb.AppendLine(MarkdownFormat.Bullet("Document", identity?.DocumentTitle));
        sb.AppendLine(MarkdownFormat.Bullet("Is Linked Element", identity?.IsLinkedElement.ToString()));
        sb.AppendLine();
    }

    private static string? CombineCategory(IdentityInfo? identity)
    {
        if (identity is null)
        {
            return null;
        }

        return identity.BuiltInCategory is null
            ? identity.CategoryName
            : $"{identity.CategoryName} ({identity.BuiltInCategory})";
    }

    private static void AppendClassification(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        var classification = snapshot.Classification;
        sb.AppendLine("## Classification");
        if (classification is null)
        {
            sb.AppendLine("_Not available._");
            sb.AppendLine();
            return;
        }

        sb.AppendLine(MarkdownFormat.Bullet("Element Kind", classification.ElementKind.ToString()));
        sb.AppendLine(MarkdownFormat.Bullet("Category Type", classification.CategoryType.ToString()));
        sb.AppendLine(MarkdownFormat.Bullet("Discipline", classification.Discipline.ToString()));
        sb.AppendLine(MarkdownFormat.Bullet("Is Element Type", classification.IsElementType.ToString()));
        sb.AppendLine(MarkdownFormat.Bullet("Is Family Instance", classification.IsFamilyInstance.ToString()));
        sb.AppendLine(MarkdownFormat.Bullet(
            "Is View / Sheet / Viewport / Title Block",
            $"{classification.IsView} / {classification.IsSheet} / {classification.IsViewport} / {classification.IsTitleBlock}"));
        sb.AppendLine(MarkdownFormat.Bullet(
            "Is Model Element / Annotation / Datum",
            $"{classification.IsModelElement} / {classification.IsAnnotation} / {classification.IsDatumElement}"));
        sb.AppendLine(MarkdownFormat.Bullet(
            "Is System Family / Loadable Family",
            $"{classification.IsSystemFamily} / {classification.IsLoadableFamily}"));
        sb.AppendLine(MarkdownFormat.Bullet("Supported Inspection Groups", string.Join(", ", classification.SupportedInspectionGroups)));
        sb.AppendLine();
    }

    private static void AppendParameters(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        sb.AppendLine("## Parameters");

        var writable = snapshot.Parameters.Where(p => p.IsWritable).ToList();
        var readOnly = snapshot.Parameters.Where(p => !p.IsWritable).ToList();

        sb.AppendLine($"### Writable Parameters ({writable.Count})");
        AppendParameterTable(sb, writable);
        sb.AppendLine();

        sb.AppendLine($"### Read-only Parameters ({readOnly.Count})");
        AppendParameterTable(sb, readOnly);
        sb.AppendLine();
    }

    private static void AppendParameterTable(StringBuilder sb, IReadOnlyList<ParameterInfoRecord> parameters)
    {
        if (parameters.Count == 0)
        {
            sb.AppendLine("_None._");
            return;
        }

        sb.AppendLine("| Name | Scope | Value | Storage Type | Built-in Parameter |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (var parameter in parameters)
        {
            sb.AppendLine(
                $"| {MarkdownFormat.EscapeCell(parameter.Name)} | {parameter.Scope} | {MarkdownFormat.EscapeCell(parameter.ValueDisplay)} | " +
                $"{parameter.StorageType} | {MarkdownFormat.EscapeCell(parameter.BuiltInParameter)} |");
        }
    }

    private static void AppendGeometry(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        var geometry = snapshot.Geometry;
        sb.AppendLine("## Geometry");
        if (geometry is null)
        {
            sb.AppendLine("_Not available._");
            sb.AppendLine();
            return;
        }

        sb.AppendLine(MarkdownFormat.Bullet("Has Bounding Box", geometry.HasBoundingBox.ToString()));
        sb.AppendLine(MarkdownFormat.Bullet("Bounding Box Min", FormatPoint(geometry.BoundingBoxMin)));
        sb.AppendLine(MarkdownFormat.Bullet("Bounding Box Max", FormatPoint(geometry.BoundingBoxMax)));
        sb.AppendLine(MarkdownFormat.Bullet("Bounding Box Center", FormatPoint(geometry.BoundingBoxCenter)));
        sb.AppendLine(MarkdownFormat.Bullet("Size (W x D x H)", $"{Round(geometry.WidthX)} x {Round(geometry.DepthY)} x {Round(geometry.HeightZ)}"));
        sb.AppendLine(MarkdownFormat.Bullet(
            "Solid / Curve / Face / Edge / Mesh Count",
            $"{geometry.SolidCount} / {geometry.CurveCount} / {geometry.FaceCount} / {geometry.EdgeCount} / {geometry.MeshCount}"));
        sb.AppendLine(MarkdownFormat.Bullet("Geometry Read Status", geometry.GeometryReadStatus.ToString()));
        sb.AppendLine();
    }

    private static void AppendLocation(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        var location = snapshot.Location;
        sb.AppendLine("## Location");
        if (location is null)
        {
            sb.AppendLine("_Not available._");
            sb.AppendLine();
            return;
        }

        sb.AppendLine(MarkdownFormat.Bullet("Location Type", location.LocationType.ToString()));
        sb.AppendLine(MarkdownFormat.Bullet("Point", FormatPoint(location.Point)));
        sb.AppendLine(MarkdownFormat.Bullet("Rotation (deg)", Round(location.RotationDegrees)));
        sb.AppendLine(MarkdownFormat.Bullet("Curve Start -> End", $"{FormatPoint(location.CurveStartPoint)} -> {FormatPoint(location.CurveEndPoint)}"));
        sb.AppendLine(MarkdownFormat.Bullet("Curve Length", Round(location.CurveLength)));
        sb.AppendLine(MarkdownFormat.Bullet("Level", location.LevelName));
        sb.AppendLine();
    }

    private static void AppendRelationships(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        var relationships = snapshot.Relationships;
        sb.AppendLine("## Relationships");
        if (relationships is null)
        {
            sb.AppendLine("_Not available._");
            sb.AppendLine();
            return;
        }

        sb.AppendLine(MarkdownFormat.Bullet("Type", relationships.TypeName));
        sb.AppendLine(MarkdownFormat.Bullet("Family", relationships.FamilyName));
        sb.AppendLine(MarkdownFormat.Bullet("Host", relationships.HostName));
        sb.AppendLine(MarkdownFormat.Bullet("Level", relationships.LevelName));
        sb.AppendLine(MarkdownFormat.Bullet("Room", relationships.RoomName));
        sb.AppendLine(MarkdownFormat.Bullet("Space", relationships.SpaceName));
        sb.AppendLine(MarkdownFormat.Bullet("View Owner", relationships.ViewOwnerName));
        sb.AppendLine(MarkdownFormat.Bullet(
            "Sheet",
            relationships.SheetNumber is null ? null : $"{relationships.SheetNumber} - {relationships.SheetName}"));
        sb.AppendLine(MarkdownFormat.Bullet(
            "Materials",
            relationships.MaterialIds.Count == 0 ? null : string.Join(", ", relationships.MaterialIds)));
        sb.AppendLine();
    }

    private static void AppendViewSheetContext(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        sb.AppendLine("## View / Sheet Context");

        var viewContext = snapshot.ViewContext;
        var sheetContext = snapshot.SheetContext;
        if (viewContext is null && sheetContext is null)
        {
            sb.AppendLine("_Not available._");
            sb.AppendLine();
            return;
        }

        if (viewContext is not null)
        {
            sb.AppendLine("### View Context");
            sb.AppendLine(MarkdownFormat.Bullet("View", viewContext.ViewName));
            sb.AppendLine(MarkdownFormat.Bullet("View Type", viewContext.ViewType));
            sb.AppendLine(MarkdownFormat.Bullet("Scale", viewContext.Scale?.ToString()));
            sb.AppendLine(MarkdownFormat.Bullet(
                "Crop Box Active / Visible",
                $"{viewContext.CropBoxActive} / {viewContext.CropBoxVisible}"));
            sb.AppendLine(MarkdownFormat.Bullet("Crop Box Min -> Max", $"{FormatPoint(viewContext.CropBoxMin)} -> {FormatPoint(viewContext.CropBoxMax)}"));
            sb.AppendLine(MarkdownFormat.Bullet("View Template", viewContext.ViewTemplateName));
            sb.AppendLine(MarkdownFormat.Bullet("Detail Level", viewContext.DetailLevel));
            sb.AppendLine(MarkdownFormat.Bullet("Display Style", viewContext.DisplayStyle));
            sb.AppendLine(MarkdownFormat.Bullet("Discipline", viewContext.Discipline.ToString()));
            sb.AppendLine(MarkdownFormat.Bullet("Associated Level", viewContext.AssociatedLevelName));
            sb.AppendLine();
        }

        if (sheetContext is not null)
        {
            sb.AppendLine("### Sheet Context");
            sb.AppendLine(MarkdownFormat.Bullet(
                "Sheet",
                sheetContext.SheetNumber is null ? sheetContext.SheetName : $"{sheetContext.SheetNumber} - {sheetContext.SheetName}"));
            sb.AppendLine(MarkdownFormat.Bullet(
                "Title Block",
                sheetContext.TitleBlockFamilyName is null ? null : $"{sheetContext.TitleBlockFamilyName} - {sheetContext.TitleBlockTypeName}"));
            sb.AppendLine(MarkdownFormat.Bullet(
                "Viewports",
                sheetContext.ViewportIds.Count == 0 ? null : string.Join(", ", sheetContext.ViewportIds)));
            sb.AppendLine(MarkdownFormat.Bullet(
                "Placed Views",
                sheetContext.PlacedViewIds.Count == 0 ? null : string.Join(", ", sheetContext.PlacedViewIds)));
            sb.AppendLine(MarkdownFormat.Bullet(
                "Schedule Instances",
                sheetContext.ScheduleSheetInstanceIds.Count == 0 ? null : string.Join(", ", sheetContext.ScheduleSheetInstanceIds)));
            sb.AppendLine(MarkdownFormat.Bullet(
                "Revisions",
                sheetContext.RevisionIds.Count == 0 ? null : string.Join(", ", sheetContext.RevisionIds)));
            sb.AppendLine();
        }
    }

    private static void AppendDictionaryNotes(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        sb.AppendLine("## Dictionary Notes");

        if (snapshot.Dictionary.Count == 0 && snapshot.UnresolvedDictionaryTerms.Count == 0)
        {
            sb.AppendLine("_No dictionary is loaded in this build; all names above are raw Revit API names._");
            sb.AppendLine();
            return;
        }

        foreach (var term in snapshot.Dictionary)
        {
            sb.AppendLine(MarkdownFormat.Bullet(
                term.ApiName ?? term.TermKey ?? "?",
                term.LocalizedName is null ? $"_(no translation, {term.Status})_" : $"{term.LocalizedName} ({term.Status})"));
        }

        if (snapshot.Dictionary.Count == 0)
        {
            sb.AppendLine("_No dictionary terms resolved for this element._");
        }

        sb.AppendLine(MarkdownFormat.Bullet(
            "Unresolved Terms",
            snapshot.UnresolvedDictionaryTerms.Count == 0 ? null : string.Join(", ", snapshot.UnresolvedDictionaryTerms)));
        sb.AppendLine();
    }

    private static void AppendSuggestedApiPaths(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        sb.AppendLine("## Suggested API Paths");
        var paths = new List<string?>
        {
            snapshot.Identity?.ApiPath,
            "Element.GetOrderedParameters() / ElementType.GetOrderedParameters()",
            snapshot.Geometry?.ApiPath,
            snapshot.Location?.ApiPath,
        };

        foreach (var path in paths.Where(p => !string.IsNullOrEmpty(p)).Distinct())
        {
            sb.AppendLine($"- `{path}`");
        }
    }

    private static string? FormatPoint(Point3D? point) =>
        point is null ? null : $"({Round(point.X)}, {Round(point.Y)}, {Round(point.Z)})";

    private static string? Round(double? value) => value?.ToString("0.###");
}
