using System.Collections.Generic;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.Core.Support;

namespace RevitParameterInspector.Export;

/// <summary>
/// The user-checked subset of the inspector's rows for the "選定匯出" Copy JSON / Copy AI
/// Context actions. Name and ElementIdString are always included as basic identity; each
/// section list holds only the rows the user checked. File exports (JSON/Markdown/Excel)
/// never use this - they always export the full snapshot.
/// </summary>
public sealed class SelectiveCopyRequest
{
    public string? Name { get; set; }
    public string? ElementIdString { get; set; }

    public List<FieldRow> SummaryRows { get; } = new();
    public List<ParameterInfoRecord> Parameters { get; } = new();
    public List<FieldRow> GeometryRows { get; } = new();
    public List<FieldRow> LocationRows { get; } = new();
    public List<FieldRow> RelationshipRows { get; } = new();
    public List<ViewSheetContextItem> ViewSheetContexts { get; } = new();
}
