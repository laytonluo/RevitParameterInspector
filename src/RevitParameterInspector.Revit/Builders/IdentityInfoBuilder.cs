using Autodesk.Revit.DB;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.Dictionary;
using RevitParameterInspector.Revit.Compatibility;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>Builds <see cref="IdentityInfo"/> from a Revit element. See HANDOFF Section 11.</summary>
public static class IdentityInfoBuilder
{
    /// <param name="resolver">
    /// Optional Dictionary Engine resolver (HANDOFF Section 20). When null, the Inspector still
    /// works and dictionary fields are simply left unset (HANDOFF Section 5.2).
    /// </param>
    public static IdentityInfo Build(Element element, DictionaryResolver? resolver = null)
    {
        var document = element.Document;
        var category = element.Category;

        var info = new IdentityInfo
        {
            ElementId = RevitCompatibility.GetIdValue(element.Id),
            ElementIdString = element.Id.ToString(),
            UniqueId = element.UniqueId,
            Name = SafeGetName(element),
            ClassName = element.GetType().Name,
            CategoryName = category?.Name,
            CategoryId = category is null ? null : RevitCompatibility.GetIdValue(category.Id),
            BuiltInCategory = TryGetBuiltInCategoryName(category),
            DocumentTitle = document.Title,
            DocumentPath = document.PathName,
            // Linked-element inspection is deferred; see HANDOFF Section 34.3.
            IsLinkedElement = false,
            ApiPath = element.GetType().FullName,
        };

        PopulateTypeInfo(element, document, info);
        ApplyDictionary(info, resolver);

        return info;
    }

    private static void ApplyDictionary(IdentityInfo info, DictionaryResolver? resolver)
    {
        if (resolver is null)
        {
            return;
        }

        var classTerm = resolver.Resolve(info.ClassName ?? string.Empty);
        info.ClassNameLocalized = classTerm.LocalizedName;
        info.ClassNameDescription = classTerm.Description;
        info.ClassNameDictionaryStatus = classTerm.Status;

        if (info.BuiltInCategory is not null)
        {
            var categoryTerm = resolver.Resolve(info.BuiltInCategory, DictionaryTermCategory.BuiltInCategory);
            info.BuiltInCategoryLocalized = categoryTerm.LocalizedName;
            info.BuiltInCategoryDescription = categoryTerm.Description;
            info.BuiltInCategoryDictionaryStatus = categoryTerm.Status;
        }
    }

    private static void PopulateTypeInfo(Element element, Document document, IdentityInfo info)
    {
        if (element is FamilyInstance familyInstance)
        {
            var symbol = familyInstance.Symbol;
            info.FamilyName = symbol?.Family?.Name;
            info.TypeName = symbol?.Name;
            info.TypeId = symbol is null ? null : RevitCompatibility.GetIdValue(symbol.Id);
            return;
        }

        var typeId = element.GetTypeId();
        if (typeId == ElementId.InvalidElementId)
        {
            return;
        }

        info.TypeId = RevitCompatibility.GetIdValue(typeId);
        info.TypeName = SafeGetName(document.GetElement(typeId));
    }

    private static string? SafeGetName(Element? element)
    {
        if (element is null)
        {
            return null;
        }

        try
        {
            return element.Name;
        }
        catch
        {
            // Some element types (e.g. certain system elements) throw on Name access.
            return null;
        }
    }

    private static string? TryGetBuiltInCategoryName(Category? category)
    {
        if (category is null)
        {
            return null;
        }

        var idValue = RevitCompatibility.GetIdValue(category.Id);
        // Built-in categories always have negative ids; positive ids are user/custom categories.
        if (idValue >= 0)
        {
            return null;
        }

        return ((BuiltInCategory)(int)idValue).ToString();
    }
}
