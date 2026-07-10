using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.UI.Reselect;

/// <summary>Outcome of a <see cref="IReselectRequestHandler.RequestReselect"/> request.</summary>
public sealed class ReselectResult
{
    /// <summary>The freshly built snapshot; null when <see cref="SourceType"/> is None.</summary>
    public ElementContextSnapshot? Snapshot { get; set; }

    public ReselectSourceType SourceType { get; set; } = ReselectSourceType.None;

    /// <summary>Friendly error text when the reselect failed; null on success.</summary>
    public string? ErrorMessage { get; set; }
}
