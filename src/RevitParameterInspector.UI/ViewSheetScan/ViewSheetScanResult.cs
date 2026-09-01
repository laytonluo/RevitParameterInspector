using System.Collections.Generic;
using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.UI.ViewSheetScan;

/// <summary>Outcome of a <see cref="IViewSheetScanRequestHandler.RequestScan"/> request.</summary>
public sealed class ViewSheetScanResult
{
    /// <summary>The scanned rows; null when the scan failed (see <see cref="ErrorMessage"/>).</summary>
    public List<ViewSheetContextItem>? Items { get; set; }

    /// <summary>Friendly error text when the scan failed; null on success.</summary>
    public string? ErrorMessage { get; set; }
}
