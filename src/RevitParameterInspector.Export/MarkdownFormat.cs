namespace RevitParameterInspector.Export;

/// <summary>Small formatting helpers shared by <see cref="MarkdownExporter"/> and <see cref="AiContextComposer"/>.</summary>
internal static class MarkdownFormat
{
    public static string Bullet(string label, string? value) =>
        $"- **{label}**: {(string.IsNullOrEmpty(value) ? "_(none)_" : EscapeInline(value!))}";

    public static string EscapeCell(string? value) =>
        string.IsNullOrEmpty(value) ? string.Empty : value!.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");

    private static string EscapeInline(string value) => value.Replace("\r", " ").Replace("\n", " ");
}
