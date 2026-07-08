using System;
using System.IO;
using System.Text.Json;
using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Export;

/// <summary>
/// JSON export: the primary machine-readable format, a full serialization of
/// <see cref="ElementContextSnapshot"/> including schema/Revit/add-in versions and raw API
/// names/paths. See HANDOFF Section 32.1.
/// </summary>
public static class JsonExporter
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string Serialize(ElementContextSnapshot snapshot) => JsonSerializer.Serialize(snapshot, Options);

    /// <summary>Writes the snapshot to the exact <paramref name="filePath"/> given (see <see cref="ExportFileNaming"/> for a suggested name/path).</summary>
    public static string ExportToFile(ElementContextSnapshot snapshot, string filePath)
    {
        var timestamp = DateTimeOffset.UtcNow;

        snapshot.ExportMetadata = new ExportMetadata
        {
            ExportFormat = "Json",
            FileName = Path.GetFileName(filePath),
            ExportedAt = timestamp,
            ToolName = "RevitParameterInspector",
            ToolVersion = snapshot.AddinVersion,
        };

        File.WriteAllText(filePath, Serialize(snapshot));
        return filePath;
    }
}
