using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitParameterInspector.Revit.Compatibility;
using RevitParameterInspector.Revit.Selection;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>Builds <see cref="CoreModels.SelectionIdsSnapshot"/> for the "Selection Ids List Ex" panel.</summary>
public static class SelectionIdsSnapshotBuilder
{
    public static CoreModels.SelectionIdsSnapshot Build(UIDocument uiDocument)
    {
        var snapshot = new CoreModels.SelectionIdsSnapshot
        {
            ActiveViewId = uiDocument.ActiveView is { } activeView
                ? RevitCompatibility.GetIdValue(activeView.Id)
                : -1,
        };

        var elements = new SelectionReader().GetCurrentSelection(uiDocument);
        var groupsByKey = new Dictionary<string, CoreModels.SelectionCategoryGroup>();

        foreach (var element in elements)
        {
            var category = element.Category;
            var displayName = category?.Name ?? element.GetType().Name;
            var ostName = TryGetOstCategoryName(category) ?? displayName;

            // Group by the stable OST/type name, not the localized display name, so two
            // categories that happen to share a UI-language label never merge.
            if (!groupsByKey.TryGetValue(ostName, out var group))
            {
                group = new CoreModels.SelectionCategoryGroup
                {
                    CategoryDisplayName = displayName,
                    CategoryOstName = ostName,
                };
                groupsByKey[ostName] = group;
                snapshot.Groups.Add(group);
            }

            group.ElementIds.Add(RevitCompatibility.GetIdValue(element.Id));
        }

        return snapshot;
    }

    private static string? TryGetOstCategoryName(Category? category)
    {
        if (category is null)
        {
            return null;
        }

        var idValue = RevitCompatibility.GetIdValue(category.Id);
        return idValue >= 0 ? null : ((BuiltInCategory)(int)idValue).ToString();
    }
}
