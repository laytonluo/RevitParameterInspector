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
    public SelectableFieldRow(FieldRow row) => Row = row;

    public bool IsSelected { get; set; }
    public FieldRow Row { get; }

    public string Field => Row.Field;
    public string Value => Row.Value;
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
}
