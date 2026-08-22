using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.UI.ViewModels;

/// <summary>
/// Backs <see cref="Views.SelectionIdsListWindow"/>: renders a <see cref="SelectionIdsSnapshot"/>
/// as display text, and builds the three clipboard variants the panel's buttons copy
/// (ids-only, full displayed text, and an English/OST_ category version).
/// </summary>
public sealed class SelectionIdsListViewModel
{
    private readonly SelectionIdsSnapshot _snapshot;

    public SelectionIdsListViewModel(SelectionIdsSnapshot snapshot)
    {
        _snapshot = snapshot;
    }

    public bool HasSelection => _snapshot.HasSelection;

    /// <summary>The text shown in the panel's content box, in Revit's current UI language.</summary>
    public string DisplayText
    {
        get
        {
            var text = new StringBuilder();
            text.Append("當前視圖ID:").Append(_snapshot.ActiveViewId);

            if (_snapshot.HasSelection)
            {
                text.Append('\n').Append("選取物件ID:")
                    .Append(JoinGroups(_snapshot.Groups, g => g.CategoryDisplayName, "；"))
                    .Append('。');
            }

            return text.ToString();
        }
    }

    /// <summary>Copy button C: view id + every selected element id, comma-separated, no labels.</summary>
    public string BuildIdsOnlyText()
    {
        var ids = new List<long> { _snapshot.ActiveViewId };
        ids.AddRange(_snapshot.Groups.SelectMany(g => g.ElementIds));
        return string.Join(",", ids);
    }

    /// <summary>Copy button D: exactly what the panel currently displays.</summary>
    public string BuildFullDisplayedText() => DisplayText;

    /// <summary>Copy button E: same content, but with OST_ category names and English field labels.</summary>
    public string BuildOstCategoryText()
    {
        var text = new StringBuilder();
        text.Append("Active View ID:").Append(_snapshot.ActiveViewId);

        if (_snapshot.HasSelection)
        {
            text.Append('\n').Append("Selection Elements ID:")
                .Append(JoinGroups(_snapshot.Groups, g => g.CategoryOstName, ";"))
                .Append('.');
        }

        return text.ToString();
    }

    private static string JoinGroups(
        IEnumerable<SelectionCategoryGroup> groups,
        Func<SelectionCategoryGroup, string> categoryName,
        string groupSeparator) =>
        string.Join(groupSeparator, groups.Select(g => $"{categoryName(g)} {string.Join(",", g.ElementIds)}"));
}
