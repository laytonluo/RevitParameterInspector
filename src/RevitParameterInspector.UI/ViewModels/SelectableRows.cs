using System;
using System.Collections.Generic;
using System.Linq;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.Core.Support;

namespace RevitParameterInspector.UI.ViewModels;

/// <summary>
/// Wraps a <see cref="FieldRow"/> with an "Export" checkbox for the selective
/// Copy JSON / Copy AI Context actions. Checkbox state lives only in the UI layer;
/// Core models stay serialization-clean.
/// </summary>
public sealed class SelectableFieldRow
{
    public SelectableFieldRow(FieldRow row)
    {
        Row = row;
        IdTokens = ElementIdLinkParser.ParseIdTokens(row.Field, row.Value);
    }

    public bool IsSelected { get; set; }
    public FieldRow Row { get; }

    public string Field => Row.Field;
    public string Value => Row.Value;

    /// <summary>Element ids parsed from <see cref="Value"/>; empty when this is not an id field.</summary>
    public IReadOnlyList<string> IdTokens { get; }

    /// <summary>True when the value should render as blue "research by ID" hyperlinks.</summary>
    public bool IsIdField => IdTokens.Count > 0;
}

/// <summary>Wraps a <see cref="ParameterInfoRecord"/> with an "Export" checkbox.</summary>
public sealed class SelectableParameterRow
{
    public SelectableParameterRow(ParameterInfoRecord record) => Record = record;

    public bool IsSelected { get; set; }
    public ParameterInfoRecord Record { get; }
}

/// <summary>Wraps a <see cref="ViewSheetContextItem"/> with an "Export" checkbox.</summary>
public sealed class SelectableViewSheetContextRow
{
    public SelectableViewSheetContextRow(ViewSheetContextItem item) => Item = item;

    public bool IsSelected { get; set; }
    public ViewSheetContextItem Item { get; }

    public string? ContextType => Item.ContextType;
    public string? Name => Item.Name;
    public string? ElementId => Item.ElementId;
    public string? AdditionalInfo => Item.AdditionalInfo;

    /// <summary>True when the ElementId cell should render as a "research by ID" hyperlink.</summary>
    public bool IsElementIdLink => ElementIdLinkParser.IsValidElementId(Item.ElementId);
}

/// <summary>
/// Decides which Field/Value rows are element-id valued: the field name's last segment ends
/// with "Id"/"Ids" (UniqueId excluded — it is a GUID string, not an ElementId) and the value
/// parses as one or more positive integers (Revit's invalid id is -1, so those never link).
/// </summary>
internal static class ElementIdLinkParser
{
    public static IReadOnlyList<string> ParseIdTokens(string field, string value)
    {
        if (!IsIdFieldName(field) || string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        var tokens = value
            .Split(',')
            .Select(token => token.Trim())
            .Where(IsValidElementId)
            .ToList();

        return tokens.Count == 0 ? Array.Empty<string>() : tokens;
    }

    public static bool IsValidElementId(string? token) =>
        long.TryParse(token, out var id) && id > 0;

    private static bool IsIdFieldName(string field)
    {
        var lastSegment = field.Substring(field.LastIndexOf('.') + 1);
        if (lastSegment.Equals("UniqueId", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return lastSegment.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
            || lastSegment.EndsWith("Ids", StringComparison.OrdinalIgnoreCase);
    }
}
