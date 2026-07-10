using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using RevitParameterInspector.Core.Support;

namespace RevitParameterInspector.Export;

/// <summary>
/// Builds the reduced clipboard payloads for Copy JSON / Copy AI Context when the user has
/// checked "選定匯出" rows: basic identity (Name + ElementId) plus only the checked rows.
/// With no checked rows the callers fall back to the full snapshot output instead.
/// </summary>
public static class SelectiveCopyBuilder
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string BuildJson(SelectiveCopyRequest request)
    {
        var payload = new Dictionary<string, object?>
        {
            ["name"] = request.Name,
            ["elementId"] = request.ElementIdString,
        };

        AddFieldSection(payload, "summary", request.SummaryRows);

        if (request.Parameters.Count > 0)
        {
            payload["parameters"] = request.Parameters;
        }

        AddFieldSection(payload, "geometry", request.GeometryRows);
        AddFieldSection(payload, "location", request.LocationRows);
        AddFieldSection(payload, "relationships", request.RelationshipRows);

        if (request.ViewSheetContexts.Count > 0)
        {
            payload["viewSheetContexts"] = request.ViewSheetContexts;
        }

        return JsonSerializer.Serialize(payload, Options);
    }

    public static string BuildAiContext(SelectiveCopyRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# RevitParameterInspector AI Context (Selected Fields): {request.Name ?? "?"} ({request.ElementIdString ?? "?"})");
        sb.AppendLine();
        sb.AppendLine(MarkdownFormat.Bullet("Name", request.Name));
        sb.AppendLine(MarkdownFormat.Bullet("Element Id", request.ElementIdString));
        sb.AppendLine();

        AppendFieldSection(sb, "Summary", request.SummaryRows);

        if (request.Parameters.Count > 0)
        {
            sb.AppendLine("## Parameters");
            sb.AppendLine("| Name | Localized Name | Scope | Value | Built-in Parameter |");
            sb.AppendLine("|---|---|---|---|---|");
            foreach (var parameter in request.Parameters)
            {
                sb.AppendLine(
                    $"| {MarkdownFormat.EscapeCell(parameter.Name)} | {MarkdownFormat.EscapeCell(parameter.LocalizedName)} | " +
                    $"{parameter.Scope} | {MarkdownFormat.EscapeCell(parameter.ValueDisplay)} | {MarkdownFormat.EscapeCell(parameter.BuiltInParameter)} |");
            }

            sb.AppendLine();
        }

        AppendFieldSection(sb, "Geometry", request.GeometryRows);
        AppendFieldSection(sb, "Location", request.LocationRows);
        AppendFieldSection(sb, "Relationships", request.RelationshipRows);

        if (request.ViewSheetContexts.Count > 0)
        {
            sb.AppendLine("## View / Sheet Context");
            sb.AppendLine("| Context Type | Name | ElementId |");
            sb.AppendLine("|---|---|---|");
            foreach (var context in request.ViewSheetContexts)
            {
                sb.AppendLine(
                    $"| {MarkdownFormat.EscapeCell(context.ContextType)} | {MarkdownFormat.EscapeCell(context.Name)} | " +
                    $"{MarkdownFormat.EscapeCell(context.ElementId)} |");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static void AddFieldSection(Dictionary<string, object?> payload, string key, List<FieldRow> rows)
    {
        if (rows.Count > 0)
        {
            payload[key] = rows.ToDictionary(row => row.Field, row => row.Value);
        }
    }

    private static void AppendFieldSection(StringBuilder sb, string heading, List<FieldRow> rows)
    {
        if (rows.Count == 0)
        {
            return;
        }

        sb.AppendLine($"## {heading}");
        foreach (var row in rows)
        {
            sb.AppendLine(MarkdownFormat.Bullet(row.Field, row.Value));
        }

        sb.AppendLine();
    }
}
