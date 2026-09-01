using System;
using System.IO;

namespace RevitParameterInspector.Core.Logging;

/// <summary>
/// Best-effort file logger for diagnosing in-Revit behavior (e.g. view redraw / performance
/// issues) without needing a debugger attached. A logging failure must never affect the add-in's
/// actual behavior, so every write is swallowed on error.
/// </summary>
public static class FileLogger
{
    private static readonly object Gate = new();
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RevitParameterInspector",
        "logs");

    public static void Log(string category, string message)
    {
        try
        {
            var line = $"{DateTime.Now:HH:mm:ss.fff} [{Environment.CurrentManagedThreadId}] [{category}] {message}";
            lock (Gate)
            {
                Directory.CreateDirectory(LogDirectory);
                var path = Path.Combine(LogDirectory, $"rpi-{DateTime.Now:yyyyMMdd}.log");
                File.AppendAllText(path, line + Environment.NewLine);
            }
        }
        catch
        {
            // Logging must never crash Revit or the UI.
        }
    }

    public static void LogException(string category, string context, Exception ex) =>
        Log(category, $"{context} threw: {ex}");
}
