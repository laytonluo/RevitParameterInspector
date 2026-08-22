namespace RevitParameterInspector.Core.Models;

/// <summary>
/// Backs the "Selection Ids List Ex" panel: the active view's id, plus the current selection's
/// element ids grouped by category. Built even when nothing is selected (Groups stays empty).
/// </summary>
public sealed class SelectionIdsSnapshot
{
    public long ActiveViewId { get; set; }

    public List<SelectionCategoryGroup> Groups { get; set; } = new();

    public bool HasSelection => Groups.Count > 0;
}

/// <summary>One category's worth of selected element ids, in both display forms (HANDOFF_Update panel spec).</summary>
public sealed class SelectionCategoryGroup
{
    /// <summary>Category name in Revit's current UI language (e.g. "牆", "Walls").</summary>
    public string CategoryDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The stable <c>BuiltInCategory</c> enum name (e.g. "OST_Walls") when the element's
    /// category resolves to one; falls back to <see cref="CategoryDisplayName"/> for
    /// non-built-in categories so the English/OST copy mode still has something to show.
    /// </summary>
    public string CategoryOstName { get; set; } = string.Empty;

    public List<long> ElementIds { get; set; } = new();
}
