using Autodesk.Revit.DB;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>Builds <see cref="CoreModels.ClassificationInfo"/> from a Revit element. See HANDOFF Section 12.</summary>
public static class ClassificationInfoBuilder
{
    public static CoreModels.ClassificationInfo Build(Element element)
    {
        var category = element.Category;
        var builtInCategory = TryGetBuiltInCategory(category);

        var isSheet = element is ViewSheet;
        var isViewport = element is Viewport;
        var isTitleBlock = builtInCategory == BuiltInCategory.OST_TitleBlocks;
        var isView = element is View && !isSheet;
        var isElementType = element is ElementType;
        var isFamilyInstance = element is FamilyInstance;
        var isDatumElement = element is Level or Grid or ReferencePlane;
        var isAnnotation = category?.CategoryType == CategoryType.Annotation;
        var isModelElement = !isElementType && !isView && !isSheet && category?.CategoryType == CategoryType.Model;

        var familySymbol = (element as FamilyInstance)?.Symbol;
        // System families (walls, floors, roofs, ...) are not backed by a loadable Family
        // object; loadable families (doors, furniture, ...) are. This is a heuristic.
        var isLoadableFamily = isFamilyInstance && familySymbol?.Family is { IsInPlace: false };
        var isSystemFamily = !isFamilyInstance && !isElementType && !isView && !isDatumElement
                              && category?.CategoryType == CategoryType.Model;

        var classification = new CoreModels.ClassificationInfo
        {
            ElementKind = DetermineElementKind(element, isSheet, isViewport, isTitleBlock, isView, isElementType, isDatumElement, category),
            CategoryType = MapCategoryType(category?.CategoryType),
            IsElementType = isElementType,
            IsFamilyInstance = isFamilyInstance,
            IsView = isView,
            IsSheet = isSheet,
            IsViewport = isViewport,
            IsTitleBlock = isTitleBlock,
            IsAnnotation = isAnnotation,
            IsModelElement = isModelElement,
            IsDatumElement = isDatumElement,
            IsSystemFamily = isSystemFamily,
            IsLoadableFamily = isLoadableFamily,
            Discipline = element is View view ? MapDiscipline(view.Discipline) : CoreModels.Discipline.Unknown,
        };

        classification.SupportedInspectionGroups.Add("Identity");
        classification.SupportedInspectionGroups.Add("Parameters");

        if (!isElementType)
        {
            classification.SupportedInspectionGroups.Add("Geometry");
            classification.SupportedInspectionGroups.Add("Location");
        }

        classification.SupportedInspectionGroups.Add("Relationships");

        if (isView || isSheet || isViewport)
        {
            classification.SupportedInspectionGroups.Add("ViewContext");
        }

        if (isSheet || isViewport || isTitleBlock)
        {
            classification.SupportedInspectionGroups.Add("SheetContext");
        }

        return classification;
    }

    private static CoreModels.ElementKind DetermineElementKind(
        Element element,
        bool isSheet,
        bool isViewport,
        bool isTitleBlock,
        bool isView,
        bool isElementType,
        bool isDatumElement,
        Category? category)
    {
        if (isSheet) return CoreModels.ElementKind.Sheet;
        if (isViewport) return CoreModels.ElementKind.Viewport;
        if (isTitleBlock) return CoreModels.ElementKind.TitleBlock;
        if (isView) return CoreModels.ElementKind.View;
        if (isElementType) return CoreModels.ElementKind.ElementType;
        if (isDatumElement) return CoreModels.ElementKind.Datum;
        if (element is Material) return CoreModels.ElementKind.Material;
        if (category?.CategoryType == CategoryType.Annotation) return CoreModels.ElementKind.AnnotationElement;
        if (category?.CategoryType == CategoryType.Model) return CoreModels.ElementKind.ModelElement;
        return CoreModels.ElementKind.Unknown;
    }

    private static CoreModels.CategoryType MapCategoryType(CategoryType? categoryType) => categoryType switch
    {
        CategoryType.Model => CoreModels.CategoryType.Model,
        CategoryType.Annotation => CoreModels.CategoryType.Annotation,
        CategoryType.Internal => CoreModels.CategoryType.Internal,
        CategoryType.AnalyticalModel => CoreModels.CategoryType.AnalyticalModel,
        CategoryType.Invalid => CoreModels.CategoryType.Invalid,
        _ => CoreModels.CategoryType.Unknown,
    };

    private static CoreModels.Discipline MapDiscipline(ViewDiscipline discipline) => discipline switch
    {
        ViewDiscipline.Architectural => CoreModels.Discipline.Architectural,
        ViewDiscipline.Structural => CoreModels.Discipline.Structural,
        ViewDiscipline.Mechanical => CoreModels.Discipline.Mechanical,
        ViewDiscipline.Electrical => CoreModels.Discipline.Electrical,
        ViewDiscipline.Coordination => CoreModels.Discipline.Coordination,
        _ => CoreModels.Discipline.Unknown,
    };

    private static BuiltInCategory? TryGetBuiltInCategory(Category? category)
    {
        if (category is null)
        {
            return null;
        }

        var idValue = category.Id.Value;
        return idValue >= 0 ? null : (BuiltInCategory)(int)idValue;
    }
}
