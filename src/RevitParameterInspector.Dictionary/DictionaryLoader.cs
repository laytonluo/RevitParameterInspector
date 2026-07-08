using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Dictionary;

/// <summary>
/// Reads dictionary term files from disk. Tolerant by design: a missing directory, a missing
/// file, or invalid JSON in one file must never stop the Inspector from working (HANDOFF
/// Sections 5.2 and 20.4) - problems are reported as warnings, not thrown as exceptions.
/// </summary>
public static class DictionaryLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Loads every *.json file directly under {directoryPath}/{locale}/.</summary>
    public static DictionaryLoadResult LoadFromDirectory(string directoryPath, string locale)
    {
        var terms = new List<DictionaryTermInfo>();
        var warnings = new List<string>();

        var localeDirectory = Path.Combine(directoryPath, locale);
        if (!Directory.Exists(localeDirectory))
        {
            warnings.Add($"Dictionary directory not found: {localeDirectory}");
            return new DictionaryLoadResult(terms, warnings);
        }

        string[] files;
        try
        {
            files = Directory.GetFiles(localeDirectory, "*.json", SearchOption.TopDirectoryOnly);
        }
        catch (Exception ex)
        {
            warnings.Add($"Failed to list dictionary files in {localeDirectory}: {ex.Message}");
            return new DictionaryLoadResult(terms, warnings);
        }

        foreach (var file in files)
        {
            LoadFile(file, locale, terms, warnings);
        }

        return new DictionaryLoadResult(terms, warnings);
    }

    private static void LoadFile(string filePath, string locale, List<DictionaryTermInfo> terms, List<string> warnings)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var entries = JsonSerializer.Deserialize<List<DictionaryTermInfo>>(json, JsonOptions);
            if (entries is null)
            {
                return;
            }

            foreach (var entry in entries)
            {
                entry.Locale ??= locale;
                terms.Add(entry);
            }
        }
        catch (Exception ex)
        {
            // A single bad file (invalid JSON, unreadable, etc.) must not break the whole load.
            warnings.Add($"Skipped invalid dictionary file '{Path.GetFileName(filePath)}': {ex.Message}");
        }
    }
}
