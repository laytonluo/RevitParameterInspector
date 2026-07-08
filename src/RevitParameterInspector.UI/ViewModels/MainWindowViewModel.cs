using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.Core.Support;
using RevitParameterInspector.Export;

namespace RevitParameterInspector.UI.ViewModels;

/// <summary>
/// Backs <see cref="Views.MainWindow"/>. Presents a single, already-built
/// <see cref="ElementContextSnapshot"/> across the Summary/Parameters/Geometry/Location/
/// Relationships/View-Sheet Context/Dictionary/Raw JSON/AI Context tabs, plus the Export
/// JSON/Markdown/Excel and Copy AI Context actions.
/// </summary>
public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly List<ParameterInfoRecord> _allParameters;
    private string _parameterSearchText = string.Empty;
    private string _selectedParameterScope = AllScopesOption;

    private const string AllScopesOption = "All";

    public ElementContextSnapshot Snapshot { get; }

    public string WindowTitle { get; }

    public IReadOnlyList<FieldRow> SummaryRows { get; }
    public IReadOnlyList<FieldRow> GeometryRows { get; }
    public IReadOnlyList<FieldRow> LocationRows { get; }
    public IReadOnlyList<FieldRow> RelationshipRows { get; }
    public IReadOnlyList<FieldRow> ViewSheetContextRows { get; }
    public IReadOnlyList<DictionaryTermInfo> DictionaryTerms { get; }
    public string UnresolvedDictionaryTermsText { get; }
    public string RawJson { get; }
    public string AiContext { get; }

    public ObservableCollection<ParameterInfoRecord> FilteredParameters { get; } = new();

    public IReadOnlyList<string> ParameterScopeOptions { get; } = new[] { AllScopesOption, "Instance", "Type" };

    public string ParameterSearchText
    {
        get => _parameterSearchText;
        set
        {
            if (_parameterSearchText == value)
            {
                return;
            }

            _parameterSearchText = value;
            OnPropertyChanged();
            ApplyParameterFilter();
        }
    }

    public string SelectedParameterScope
    {
        get => _selectedParameterScope;
        set
        {
            if (_selectedParameterScope == value)
            {
                return;
            }

            _selectedParameterScope = value;
            OnPropertyChanged();
            ApplyParameterFilter();
        }
    }

    public MainWindowViewModel(ElementContextSnapshot snapshot)
    {
        Snapshot = snapshot;
        _allParameters = snapshot.Parameters;

        var identity = snapshot.Identity;
        WindowTitle = $"RevitParameterInspector - {identity?.ClassName ?? "Element"} ({identity?.ElementIdString ?? "?"})";

        SummaryRows = BuildSummaryRows(snapshot);
        GeometryRows = ObjectInspector.ToFieldRows(snapshot.Geometry);
        LocationRows = ObjectInspector.ToFieldRows(snapshot.Location);
        RelationshipRows = ObjectInspector.ToFieldRows(snapshot.Relationships);
        ViewSheetContextRows = ObjectInspector.ToFieldRows(snapshot.ViewContext, "View")
            .Concat(ObjectInspector.ToFieldRows(snapshot.SheetContext, "Sheet"))
            .ToList();
        DictionaryTerms = snapshot.Dictionary;
        UnresolvedDictionaryTermsText = snapshot.UnresolvedDictionaryTerms.Count == 0
            ? "(none)"
            : string.Join(", ", snapshot.UnresolvedDictionaryTerms);
        RawJson = JsonExporter.Serialize(snapshot);
        AiContext = AiContextComposer.Build(snapshot);

        ApplyParameterFilter();
    }

    private void ApplyParameterFilter()
    {
        IEnumerable<ParameterInfoRecord> query = _allParameters;

        if (SelectedParameterScope == "Instance")
        {
            query = query.Where(p => p.Scope == ParameterScope.Instance);
        }
        else if (SelectedParameterScope == "Type")
        {
            query = query.Where(p => p.Scope == ParameterScope.Type);
        }

        if (!string.IsNullOrWhiteSpace(ParameterSearchText))
        {
            var text = ParameterSearchText.Trim();
            query = query.Where(p =>
                Contains(p.Name, text) ||
                Contains(p.LocalizedName, text) ||
                Contains(p.BuiltInParameter, text) ||
                Contains(p.ValueDisplay, text) ||
                Contains(p.Description, text) ||
                Contains(p.ApiPath, text));
        }

        FilteredParameters.Clear();
        foreach (var record in query)
        {
            FilteredParameters.Add(record);
        }
    }

    private static bool Contains(string? source, string text) =>
        !string.IsNullOrEmpty(source) && source!.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;

    private static List<FieldRow> BuildSummaryRows(ElementContextSnapshot snapshot)
    {
        var identity = snapshot.Identity;
        return new List<FieldRow>
        {
            new("Element Id", identity?.ElementIdString ?? string.Empty),
            new("Unique Id", identity?.UniqueId ?? string.Empty),
            new("Class Name", identity?.ClassName ?? string.Empty),
            new("Category", identity?.CategoryName ?? string.Empty),
            new("Built-in Category", identity?.BuiltInCategory ?? string.Empty),
            new("Name", identity?.Name ?? string.Empty),
            new("Family", identity?.FamilyName ?? string.Empty),
            new("Type", identity?.TypeName ?? string.Empty),
            new("Document", identity?.DocumentTitle ?? string.Empty),
            new("Is Linked Element", identity?.IsLinkedElement.ToString() ?? string.Empty),
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
