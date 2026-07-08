namespace RevitParameterInspector.Core.Support;

/// <summary>
/// A single Field/Value row for the generic reflection-based detail grids/sheets shared by
/// the UI and Export projects. A plain class (not a record) because record init-accessors
/// need a BCL shim type unavailable on net48.
/// </summary>
public sealed class FieldRow
{
    public string Field { get; }
    public string Value { get; }

    public FieldRow(string field, string value)
    {
        Field = field;
        Value = value;
    }
}
