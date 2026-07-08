namespace RevitParameterInspector.Core.Models;

/// <summary>
/// One localized dictionary entry. The Dictionary Engine itself is out of scope for this
/// step; this type only defines the shape of an entry. See HANDOFF Section 19.
/// </summary>
public sealed class DictionaryTermInfo
{
    public string? TermKey { get; set; }
    public string? ApiName { get; set; }
    public string? LocalizedName { get; set; }
    public string? Locale { get; set; }
    public string? Description { get; set; }
    public List<string> Keywords { get; set; } = new();
    public string? Category { get; set; }
    public int? Priority { get; set; }
    public DictionaryStatus Status { get; set; } = DictionaryStatus.Default;
    public string? Source { get; set; }
    public string? LastUpdated { get; set; }
    public string? Contributor { get; set; }
    public string? Notes { get; set; }
}
