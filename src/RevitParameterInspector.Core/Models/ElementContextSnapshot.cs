namespace RevitParameterInspector.Core.Models;

/// <summary>
/// The central normalized object for a selected element. This is the single source of
/// truth for the UI, JSON export, Markdown export, Excel export, Copy AI Context, and any
/// future read-only MCP context. See HANDOFF Sections 5.1 and 10.
/// </summary>
public sealed class ElementContextSnapshot
{
    public string SchemaVersion { get; set; } = "1.0.0";
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? RevitVersion { get; set; }
    public string? AddinVersion { get; set; }

    public DocumentInfo? Document { get; set; }
    public IdentityInfo? Identity { get; set; }
    public ClassificationInfo? Classification { get; set; }
    public List<ParameterInfoRecord> Parameters { get; set; } = new();
    public GeometryInfo? Geometry { get; set; }
    public LocationInfo? Location { get; set; }
    public RelationshipInfo? Relationships { get; set; }
    public ViewContextInfo? ViewContext { get; set; }
    public SheetContextInfo? SheetContext { get; set; }

    /// <summary>
    /// Simplified UI-ready View / Sheet context rows for the inspected element
    /// (HANDOFF_Update_ViewSheetContext_V1). ViewContext/SheetContext above remain the
    /// detailed structures; this list drives the View / Sheet Context page and exports.
    /// </summary>
    public List<ViewSheetContextItem> ViewSheetContexts { get; set; } = new();

    public List<DictionaryTermInfo> Dictionary { get; set; } = new();

    /// <summary>Distinct API names looked up during this build that had no dictionary mapping, grouped by lookup category. See HANDOFF Section 29.</summary>
    public List<UnresolvedTermInfo> UnresolvedDictionaryTerms { get; set; } = new();

    /// <summary>Escape hatch for raw/unprocessed API data that doesn't fit elsewhere yet.</summary>
    public Dictionary<string, string> Raw { get; set; } = new();

    public ExportMetadata? ExportMetadata { get; set; }
}
