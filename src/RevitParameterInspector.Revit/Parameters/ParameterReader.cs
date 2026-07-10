using System.Collections.Generic;
using System.Globalization;
using Autodesk.Revit.DB;
using RevitParameterInspector.Dictionary;
using RevitParameterInspector.Revit.Compatibility;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Parameters;

/// <summary>
/// Reads instance and type parameters into <see cref="CoreModels.ParameterInfoRecord"/> rows.
/// A single malformed parameter must not fail the whole read (HANDOFF Section 35).
/// See HANDOFF Section 13.
/// </summary>
public static class ParameterReader
{
    public static List<CoreModels.ParameterInfoRecord> ReadInstanceParameters(Element element, DictionaryResolver? resolver = null)
        => ReadParameters(element, CoreModels.ParameterScope.Instance, "Element.GetOrderedParameters()", resolver);

    /// <summary>Returns null if the element has no type (e.g. it is itself an ElementType).</summary>
    public static List<CoreModels.ParameterInfoRecord>? ReadTypeParameters(Element element, DictionaryResolver? resolver = null)
    {
        var typeId = element.GetTypeId();
        if (typeId == ElementId.InvalidElementId)
        {
            return null;
        }

        var typeElement = element.Document.GetElement(typeId);
        if (typeElement is null)
        {
            return null;
        }

        return ReadParameters(
            typeElement,
            CoreModels.ParameterScope.Type,
            "ElementType.GetOrderedParameters()",
            resolver,
            RevitCompatibility.GetIdValue(typeId));
    }

    private static List<CoreModels.ParameterInfoRecord> ReadParameters(
        Element source,
        CoreModels.ParameterScope scope,
        string apiPath,
        DictionaryResolver? resolver,
        long? sourceElementIdOverride = null)
    {
        var records = new List<CoreModels.ParameterInfoRecord>();
        var isFamilyDocument = source.Document.IsFamilyDocument;
        var sourceElementId = sourceElementIdOverride ?? RevitCompatibility.GetIdValue(source.Id);

        foreach (var parameter in source.GetOrderedParameters())
        {
            try
            {
                records.Add(BuildRecord(parameter, scope, apiPath, sourceElementId, isFamilyDocument, resolver));
            }
            catch
            {
                // Skip parameters that fail to read rather than aborting the whole element.
            }
        }

        return records;
    }

    private static CoreModels.ParameterInfoRecord BuildRecord(
        Parameter parameter,
        CoreModels.ParameterScope scope,
        string apiPath,
        long sourceElementId,
        bool isFamilyDocument,
        DictionaryResolver? resolver)
    {
        var definition = parameter.Definition;
        var builtInParameter = (definition as InternalDefinition)?.BuiltInParameter ?? BuiltInParameter.INVALID;
        var isBuiltIn = builtInParameter != BuiltInParameter.INVALID;

        var kind = isBuiltIn
            ? CoreModels.ParameterKind.BuiltInParameter
            : parameter.IsShared
                ? CoreModels.ParameterKind.SharedParameter
                : isFamilyDocument
                    ? CoreModels.ParameterKind.FamilyParameter
                    : CoreModels.ParameterKind.ProjectParameter;

        var record = new CoreModels.ParameterInfoRecord
        {
            // Name is the stable raw API identifier (BuiltInParameter enum string); user-created
            // Shared/Project/Family parameters have no such identifier, so Name stays null for
            // them. LocalizedName is what Revit's own UI shows (Definition.Name).
            Name = isBuiltIn ? builtInParameter.ToString() : null,
            LocalizedName = definition?.Name,
            ApiPath = apiPath,
            Scope = scope,
            ParameterKind = kind,
            StorageType = parameter.StorageType.ToString(),
            ValueRaw = SafeGetValueRaw(parameter),
            ValueDisplay = SafeGetValueDisplay(parameter),
            GroupName = SafeGetGroupName(definition),
            GroupTypeId = SafeGetGroupTypeId(definition),
            DataType = SafeGetDataType(definition),
            UnitType = SafeGetUnitType(parameter),
            BuiltInParameter = isBuiltIn ? builtInParameter.ToString() : null,
            BuiltInParameterId = isBuiltIn ? (long)builtInParameter : null,
            IsShared = parameter.IsShared,
            IsProjectParameter = kind == CoreModels.ParameterKind.ProjectParameter,
            IsFamilyParameter = kind == CoreModels.ParameterKind.FamilyParameter,
            IsBuiltIn = isBuiltIn,
            IsReadOnly = parameter.IsReadOnly,
            IsWritable = !parameter.IsReadOnly,
            HasValue = parameter.HasValue,
            SourceElementId = sourceElementId,
            DictionaryStatus = CoreModels.DictionaryStatus.NotFound,
        };

        // Only BuiltInParameter is a stable, locale-independent API identifier worth looking up;
        // Shared/Project/Family parameters are user-created and must never be translated (HANDOFF
        // Section 5.4). The dictionary never overwrites LocalizedName - that stays Definition.Name.
        if (resolver is not null && isBuiltIn)
        {
            var term = resolver.Resolve(record.BuiltInParameter!, CoreModels.DictionaryTermCategory.BuiltInParameter);
            record.Description = term.Description;
            record.Keywords = term.Keywords;
            record.DictionaryStatus = term.Status;
        }

        return record;
    }

    private static string? SafeGetValueRaw(Parameter parameter)
    {
        try
        {
            return parameter.StorageType switch
            {
                StorageType.Integer => parameter.AsInteger().ToString(CultureInfo.InvariantCulture),
                StorageType.Double => parameter.AsDouble().ToString(CultureInfo.InvariantCulture),
                StorageType.String => parameter.AsString(),
                StorageType.ElementId => parameter.AsElementId()?.Value.ToString(CultureInfo.InvariantCulture),
                _ => null,
            };
        }
        catch
        {
            return null;
        }
    }

    private static string? SafeGetValueDisplay(Parameter parameter)
    {
        try
        {
            return parameter.AsValueString() ?? SafeGetValueRaw(parameter);
        }
        catch
        {
            return null;
        }
    }

    private static string? SafeGetGroupName(Definition? definition)
    {
        try
        {
            var groupTypeId = definition?.GetGroupTypeId();
            return groupTypeId is null ? null : LabelUtils.GetLabelForGroup(groupTypeId);
        }
        catch
        {
            return null;
        }
    }

    private static string? SafeGetGroupTypeId(Definition? definition)
    {
        try
        {
            return definition?.GetGroupTypeId()?.TypeId;
        }
        catch
        {
            return null;
        }
    }

    private static string? SafeGetDataType(Definition? definition)
    {
        try
        {
            return definition?.GetDataType()?.TypeId;
        }
        catch
        {
            return null;
        }
    }

    private static string? SafeGetUnitType(Parameter parameter)
    {
        try
        {
            return parameter.StorageType == StorageType.Double ? parameter.GetUnitTypeId()?.TypeId : null;
        }
        catch
        {
            return null;
        }
    }
}
