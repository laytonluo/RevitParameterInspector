using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.Core.Support;
using RevitParameterInspector.Export;

namespace RevitParameterInspector.UI.ViewModels;

/// <summary>
/// Backs <see cref="Views.MainWindow"/>. Presents an <see cref="ElementContextSnapshot"/>
/// across the Summary/Parameters/Geometry/Location/Relationships/View-Sheet Context/
/// Dictionary/AI Context tabs. <see cref="LoadSnapshot"/> replaces the whole snapshot in
/// place (the Reselect workflow), refreshing every tab with no stale data.
/// </summary>
public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private List<SelectableParameterRow> _allParameters = new();
    private string _parameterSearchText = string.Empty;
    private string _selectedParameterScope = AllScopesOption;
    private string _statusMessage = string.Empty;

    private const string AllScopesOption = "All";

    public ElementContextSnapshot Snapshot { get; private set; } = null!;

    public string WindowTitle { get; private set; } = string.Empty;

    public IReadOnlyList<SelectableFieldRow> SummaryRows { get; private set; } = Array.Empty<SelectableFieldRow>();
    public IReadOnlyList<SelectableFieldRow> GeometryRows { get; private set; } = Array.Empty<SelectableFieldRow>();
    public IReadOnlyList<SelectableFieldRow> LocationRows { get; private set; } = Array.Empty<SelectableFieldRow>();
    public IReadOnlyList<SelectableFieldRow> RelationshipRows { get; private set; } = Array.Empty<SelectableFieldRow>();
    public IReadOnlyList<SelectableViewSheetContextRow> ViewSheetContextRows { get; private set; } = Array.Empty<SelectableViewSheetContextRow>();
    public IReadOnlyList<DictionaryTermInfo> DictionaryTerms { get; private set; } = Array.Empty<DictionaryTermInfo>();
    public string DictionaryCountsText { get; private set; } = string.Empty;
    public string UnresolvedDictionaryTermsText { get; private set; } = string.Empty;
    public string RawJson { get; private set; } = string.Empty;
    public string AiContext { get; private set; } = string.Empty;

    public ObservableCollection<SelectableParameterRow> FilteredParameters { get; } = new();

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

    /// <summary>Short feedback line next to the Reselect button (e.g. "Reloaded from active view: ...").</summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage == value)
            {
                return;
            }

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public MainWindowViewModel(ElementContextSnapshot snapshot)
    {
        LoadSnapshot(snapshot);
    }

    /// <summary>
    /// Replaces the current snapshot and rebuilds every tab's data. The previous snapshot's
    /// rows (and their Export checkbox state) are discarded entirely - no stale data survives
    /// a Reselect (HANDOFF_Update_Reload_CurrentContext_V1 Section 15).
    /// </summary>
    public void LoadSnapshot(ElementContextSnapshot snapshot)
    {
        Snapshot = snapshot;

        var identity = snapshot.Identity;
        WindowTitle = $"RevitParameterInspector - {identity?.ClassName ?? "Element"} ({identity?.ElementIdString ?? "?"})";

        SummaryRows = BuildSummaryRows(snapshot).Select(row => new SelectableFieldRow(row)).ToList();
        GeometryRows = Wrap(ObjectInspector.ToFieldRows(snapshot.Geometry));
        LocationRows = Wrap(ObjectInspector.ToFieldRows(snapshot.Location));
        RelationshipRows = Wrap(ObjectInspector.ToFieldRows(snapshot.Relationships));
        ViewSheetContextRows = snapshot.ViewSheetContexts.Select(item => new SelectableViewSheetContextRow(item)).ToList();
        DictionaryTerms = snapshot.Dictionary;
        DictionaryCountsText = $"Resolved Terms: {snapshot.Dictionary.Count}    Unresolved Terms: {snapshot.UnresolvedDictionaryTerms.Count}";
        UnresolvedDictionaryTermsText = BuildUnresolvedTermsText(snapshot.UnresolvedDictionaryTerms);
        RawJson = JsonExporter.Serialize(snapshot);
        AiContext = AiContextComposer.Build(snapshot);

        _allParameters = snapshot.Parameters.Select(record => new SelectableParameterRow(record)).ToList();
        ApplyParameterFilter();

        // Refresh every binding at once; the tab set is fixed, only the data changed.
        OnPropertyChanged(string.Empty);
    }

    /// <summary>
    /// Clipboard payload for Copy JSON: the full snapshot JSON, or - when any Export checkbox
    /// is ticked - only basic identity plus the checked rows.
    /// </summary>
    public string BuildCopyJson()
    {
        var request = BuildSelectiveRequest();
        return request is null ? RawJson : SelectiveCopyBuilder.BuildJson(request);
    }

    /// <summary>Clipboard payload for Copy AI Context; same selective rule as <see cref="BuildCopyJson"/>.</summary>
    public string BuildCopyAiContext()
    {
        var request = BuildSelectiveRequest();
        return request is null ? AiContextComposer.Build(Snapshot) : SelectiveCopyBuilder.BuildAiContext(request);
    }

    /// <summary>Returns null when nothing is checked anywhere (callers fall back to full output).</summary>
    private SelectiveCopyRequest? BuildSelectiveRequest()
    {
        var summary = SummaryRows.Where(row => row.IsSelected).ToList();
        var parameters = _allParameters.Where(row => row.IsSelected).ToList();
        var geometry = GeometryRows.Where(row => row.IsSelected).ToList();
        var location = LocationRows.Where(row => row.IsSelected).ToList();
        var relationships = RelationshipRows.Where(row => row.IsSelected).ToList();
        var viewSheet = ViewSheetContextRows.Where(row => row.IsSelected).ToList();

        if (summary.Count + parameters.Count + geometry.Count + location.Count + relationships.Count + viewSheet.Count == 0)
        {
            return null;
        }

        var request = new SelectiveCopyRequest
        {
            Name = Snapshot.Identity?.Name,
            ElementIdString = Snapshot.Identity?.ElementIdString,
        };

        request.SummaryRows.AddRange(summary.Select(row => row.Row));
        request.Parameters.AddRange(parameters.Select(row => row.Record));
        request.GeometryRows.AddRange(geometry.Select(row => row.Row));
        request.LocationRows.AddRange(location.Select(row => row.Row));
        request.RelationshipRows.AddRange(relationships.Select(row => row.Row));
        request.ViewSheetContexts.AddRange(viewSheet.Select(row => row.Item));
        return request;
    }

    private static IReadOnlyList<SelectableFieldRow> Wrap(IReadOnlyList<FieldRow> rows) =>
        rows.Select(row => new SelectableFieldRow(row)).ToList();

    private void ApplyParameterFilter()
    {
        IEnumerable<SelectableParameterRow> query = _allParameters;

        if (SelectedParameterScope == "Instance")
        {
            query = query.Where(row => row.Record.Scope == ParameterScope.Instance);
        }
        else if (SelectedParameterScope == "Type")
        {
            query = query.Where(row => row.Record.Scope == ParameterScope.Type);
        }

        if (!string.IsNullOrWhiteSpace(ParameterSearchText))
        {
            var text = ParameterSearchText.Trim();
            query = query.Where(row =>
                Contains(row.Record.Name, text) ||
                Contains(row.Record.LocalizedName, text) ||
                Contains(row.Record.BuiltInParameter, text) ||
                Contains(row.Record.ValueDisplay, text) ||
                Contains(row.Record.Description, text) ||
                Contains(row.Record.ApiPath, text));
        }

        FilteredParameters.Clear();
        foreach (var row in query)
        {
            FilteredParameters.Add(row);
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

    /// <summary>
    /// Groups unresolved terms by lookup category so the list reads as a dictionary build
    /// list. All categories are always shown, including the not-yet-read ones (IFC /
    /// Analytical), so contributors can see the full intended taxonomy.
    /// </summary>
    private static string BuildUnresolvedTermsText(IReadOnlyList<UnresolvedTermInfo> unresolvedTerms)
    {
        var categories = new (DictionaryTermCategory Category, string DisplayName)[]
        {
            (DictionaryTermCategory.ApiTerm, "API Terms"),
            (DictionaryTermCategory.BuiltInCategory, "BuiltInCategory"),
            (DictionaryTermCategory.BuiltInParameter, "BuiltInParameter"),
            (DictionaryTermCategory.IfcParameter, "IFC Parameters"),
            (DictionaryTermCategory.AnalyticalParameter, "Analytical Parameters"),
        };

        var sb = new StringBuilder();
        foreach (var (category, displayName) in categories)
        {
            var terms = unresolvedTerms
                .Where(term => term.Category == category && term.Term is not null)
                .Select(term => term.Term!)
                .ToList();

            sb.AppendLine($"{displayName} ({terms.Count}): {(terms.Count == 0 ? "(none)" : string.Join(", ", terms))}");
        }

        return sb.ToString().TrimEnd();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
