using System.Linq;
using System.Text;
using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Export;

/// <summary>
/// Builds the concise AI-readable text copied by the one-click "Copy AI Context" action.
/// Unlike <see cref="MarkdownExporter"/>, this is meant to be pasted directly into a chat
/// window, so it stays short and skips the raw Classification dump. See HANDOFF Section 33.
/// </summary>
public static class AiContextComposer
{
    private const int MaxWritableParametersListed = 50;

    public static string Build(ElementContextSnapshot snapshot)
    {
        var sb = new StringBuilder();
        var identity = snapshot.Identity;

        sb.AppendLine($"# RevitParameterInspector AI Context: {identity?.ClassName ?? "Unknown"} ({identity?.ElementIdString ?? "?"})");
        sb.AppendLine();

        AppendElementSummary(sb, identity);
        AppendParameterSummary(sb, snapshot);
        AppendGeometrySummary(sb, snapshot.Geometry);
        AppendLocationSummary(sb, snapshot.Location);
        AppendRelationshipSummary(sb, snapshot.Relationships);
        AppendViewSheetSummary(sb, snapshot);
        AppendApiPathNotes(sb, snapshot);
        AppendDictionaryNotes(sb, snapshot);

        return sb.ToString();
    }

    private static void AppendElementSummary(StringBuilder sb, IdentityInfo? identity)
    {
        sb.AppendLine("## Selected Element Summary");
        sb.AppendLine(MarkdownFormat.Bullet("Name", identity?.Name));
        sb.AppendLine(MarkdownFormat.Bullet("Category", identity?.BuiltInCategory ?? identity?.CategoryName));
        sb.AppendLine(MarkdownFormat.Bullet("Family / Type", identity is null ? null : $"{identity.FamilyName} / {identity.TypeName}"));
        sb.AppendLine(MarkdownFormat.Bullet("Document", identity?.DocumentTitle));
        sb.AppendLine();
    }

    private static void AppendParameterSummary(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        sb.AppendLine("## Parameter Summary");

        var writable = snapshot.Parameters.Where(p => p.IsWritable).ToList();
        var readOnlyCount = snapshot.Parameters.Count - writable.Count;
        sb.AppendLine($"{snapshot.Parameters.Count} parameters read ({writable.Count} writable, {readOnlyCount} read-only).");

        foreach (var parameter in writable.Take(MaxWritableParametersListed))
        {
            sb.AppendLine($"- **{parameter.Name}** = {parameter.ValueDisplay} ({parameter.Scope}, {parameter.StorageType})");
        }

        if (writable.Count > MaxWritableParametersListed)
        {
            sb.AppendLine($"- _...and {writable.Count - MaxWritableParametersListed} more writable parameters (see full JSON/Markdown export)._");
        }

        sb.AppendLine();
    }

    private static void AppendGeometrySummary(StringBuilder sb, GeometryInfo? geometry)
    {
        sb.AppendLine("## Geometry Summary");
        sb.AppendLine(geometry is null
            ? "_Not available._"
            : $"Bounding box size {Round(geometry.WidthX)} x {Round(geometry.DepthY)} x {Round(geometry.HeightZ)}; " +
              $"{geometry.SolidCount} solids, {geometry.FaceCount} faces ({geometry.GeometryReadStatus}).");
        sb.AppendLine();
    }

    private static void AppendLocationSummary(StringBuilder sb, LocationInfo? location)
    {
        sb.AppendLine("## Location Summary");
        if (location is null)
        {
            sb.AppendLine("_Not available._");
        }
        else
        {
            var levelSuffix = location.LevelName is null ? string.Empty : $" on level {location.LevelName}";
            sb.AppendLine($"{location.LocationType}{levelSuffix}.");
        }

        sb.AppendLine();
    }

    private static void AppendRelationshipSummary(StringBuilder sb, RelationshipInfo? relationships)
    {
        sb.AppendLine("## Relationship Summary");
        if (relationships is null)
        {
            sb.AppendLine("_Not available._");
            sb.AppendLine();
            return;
        }

        var parts = new[]
        {
            relationships.TypeName is null ? null : $"type {relationships.TypeName}",
            relationships.HostName is null ? null : $"hosted by {relationships.HostName}",
            relationships.LevelName is null ? null : $"level {relationships.LevelName}",
            relationships.SheetNumber is null ? null : $"on sheet {relationships.SheetNumber}",
        }.Where(part => part is not null);

        var joined = string.Join(", ", parts);
        sb.AppendLine(string.IsNullOrEmpty(joined) ? "_No related objects detected._" : joined);
        sb.AppendLine();
    }

    private static void AppendViewSheetSummary(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        sb.AppendLine("## View / Sheet Summary");

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
            var scaleSuffix = viewContext.Scale is null ? string.Empty : $", scale 1:{viewContext.Scale}";
            sb.AppendLine($"View **{viewContext.ViewName}** ({viewContext.ViewType}){scaleSuffix}.");
        }

        if (sheetContext is not null)
        {
            var sheetLabel = sheetContext.SheetNumber is null ? sheetContext.SheetName : $"{sheetContext.SheetNumber} - {sheetContext.SheetName}";
            var titleBlockSuffix = sheetContext.TitleBlockFamilyName is null ? string.Empty : $" (title block {sheetContext.TitleBlockFamilyName})";
            sb.AppendLine($"Sheet **{sheetLabel}**{titleBlockSuffix}.");
        }

        sb.AppendLine();
    }

    private static void AppendApiPathNotes(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        sb.AppendLine("## API Path Notes");
        sb.AppendLine(MarkdownFormat.Bullet("Identity", snapshot.Identity?.ApiPath));
        sb.AppendLine(MarkdownFormat.Bullet("Parameters", "Element.GetOrderedParameters() / ElementType.GetOrderedParameters()"));
        sb.AppendLine(MarkdownFormat.Bullet("Geometry", snapshot.Geometry?.ApiPath));
        sb.AppendLine(MarkdownFormat.Bullet("Location", snapshot.Location?.ApiPath));
        sb.AppendLine();
    }

    private static void AppendDictionaryNotes(StringBuilder sb, ElementContextSnapshot snapshot)
    {
        sb.AppendLine("## Dictionary Notes");
        sb.AppendLine(snapshot.Dictionary.Count == 0
            ? "_No dictionary is loaded in this build; names above are raw Revit API names._"
            : $"{snapshot.Dictionary.Count} dictionary terms resolved.");
    }

    private static string? Round(double? value) => value?.ToString("0.###");
}
