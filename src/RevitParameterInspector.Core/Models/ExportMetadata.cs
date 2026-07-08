namespace RevitParameterInspector.Core.Models;

/// <summary>
/// Metadata about a specific export action (JSON/Markdown/Excel/Clipboard), as opposed to
/// the snapshot-level generation info on <see cref="ElementContextSnapshot"/>. See HANDOFF
/// Sections 10 and 32.
/// </summary>
public sealed class ExportMetadata
{
    public string? ExportFormat { get; set; }
    public string? FileName { get; set; }
    public DateTimeOffset? ExportedAt { get; set; }
    public string? ExportedBy { get; set; }
    public string? ToolName { get; set; }
    public string? ToolVersion { get; set; }
    public string? Notes { get; set; }
}
