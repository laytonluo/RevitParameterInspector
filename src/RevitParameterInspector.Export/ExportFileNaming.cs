using System;
using System.IO;
using System.Linq;
using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Export;

/// <summary>
/// Builds export file names per HANDOFF Section 32:
/// element-context_{documentName}_{elementId}_{timestamp}.{ext}
/// </summary>
public static class ExportFileNaming
{
    public static string BuildFileName(ElementContextSnapshot snapshot, string extension, DateTimeOffset timestamp)
    {
        var documentName = Sanitize(snapshot.Document?.Title, "document");
        var elementId = Sanitize(snapshot.Identity?.ElementIdString, "unknown");
        var stamp = timestamp.ToString("yyyyMMdd-HHmmss");

        return $"element-context_{documentName}_{elementId}_{stamp}.{extension.TrimStart('.')}";
    }

    private static string Sanitize(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Select(c => invalidChars.Contains(c) || c == ' ' ? '_' : c).ToArray());
        cleaned = cleaned.Trim('_');
        return cleaned.Length == 0 ? fallback : cleaned;
    }
}
