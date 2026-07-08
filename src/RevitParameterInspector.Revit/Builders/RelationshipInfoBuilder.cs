using System.Linq;
using Autodesk.Revit.DB;
using RevitParameterInspector.Revit.Compatibility;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>
/// Builds related-object ids/names (type, family, host, level, room/space, view/sheet,
/// group, assembly, design option, phases). Recursive inspection of related elements is
/// deferred to a later step. See HANDOFF Section 16.
/// </summary>
public static class RelationshipInfoBuilder
{
    public static CoreModels.RelationshipInfo Build(Element element)
    {
        var document = element.Document;
        var info = new CoreModels.RelationshipInfo();

        ReadType(element, document, info);
        ReadFamilyAndHost(element, info);
        ReadLevel(element, document, info);
        ReadRoomAndSpace(element, info);
        ReadMaterials(element, info);
        ReadViewOwner(element, document, info);
        ReadSheet(element, document, info);
        ReadGroupAndAssembly(element, info);
        ReadDesignOption(element, info);
        ReadPhases(element, info);

        return info;
    }

    private static void ReadType(Element element, Document document, CoreModels.RelationshipInfo info)
    {
        try
        {
            var typeId = element.GetTypeId();
            if (typeId == ElementId.InvalidElementId)
            {
                return;
            }

            info.TypeElementId = RevitCompatibility.GetIdValue(typeId);
            info.TypeName = SafeGetName(document.GetElement(typeId));
        }
        catch
        {
            // Not every element supports GetTypeId meaningfully.
        }
    }

    private static void ReadFamilyAndHost(Element element, CoreModels.RelationshipInfo info)
    {
        if (element is not FamilyInstance familyInstance)
        {
            return;
        }

        try
        {
            var family = familyInstance.Symbol?.Family;
            if (family is not null)
            {
                info.FamilyId = RevitCompatibility.GetIdValue(family.Id);
                info.FamilyName = family.Name;
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            var host = familyInstance.Host;
            if (host is not null)
            {
                info.HostElementId = RevitCompatibility.GetIdValue(host.Id);
                info.HostName = SafeGetName(host);
            }
        }
        catch
        {
            // ignore
        }
    }

    private static void ReadLevel(Element element, Document document, CoreModels.RelationshipInfo info)
    {
        try
        {
            var levelId = element.LevelId;
            if (levelId is null || levelId == ElementId.InvalidElementId)
            {
                return;
            }

            info.LevelId = RevitCompatibility.GetIdValue(levelId);
            info.LevelName = (document.GetElement(levelId) as Level)?.Name;
        }
        catch
        {
            // LevelId is not meaningful for every element type.
        }
    }

    private static void ReadRoomAndSpace(Element element, CoreModels.RelationshipInfo info)
    {
        if (element is not FamilyInstance familyInstance)
        {
            return;
        }

        try
        {
            var room = familyInstance.Room;
            if (room is not null)
            {
                info.RoomId = RevitCompatibility.GetIdValue(room.Id);
                info.RoomName = SafeGetName(room);
            }
        }
        catch
        {
            // Room lookup can throw for instances that don't support it.
        }

        try
        {
            var space = familyInstance.Space;
            if (space is not null)
            {
                info.SpaceId = RevitCompatibility.GetIdValue(space.Id);
                info.SpaceName = SafeGetName(space);
            }
        }
        catch
        {
            // Space lookup can throw for instances that don't support it.
        }
    }

    private static void ReadMaterials(Element element, CoreModels.RelationshipInfo info)
    {
        try
        {
            var materialIds = element.GetMaterialIds(false);
            if (materialIds is null)
            {
                return;
            }

            info.MaterialIds.AddRange(materialIds.Select(RevitCompatibility.GetIdValue));
        }
        catch
        {
            // ignore
        }
    }

    private static void ReadViewOwner(Element element, Document document, CoreModels.RelationshipInfo info)
    {
        try
        {
            var ownerViewId = element.OwnerViewId;
            if (ownerViewId is null || ownerViewId == ElementId.InvalidElementId)
            {
                return;
            }

            info.ViewOwnerId = RevitCompatibility.GetIdValue(ownerViewId);
            info.ViewOwnerName = SafeGetName(document.GetElement(ownerViewId));
        }
        catch
        {
            // ignore
        }
    }

    private static void ReadSheet(Element element, Document document, CoreModels.RelationshipInfo info)
    {
        if (element is not Viewport viewport)
        {
            return;
        }

        try
        {
            var sheetId = viewport.SheetId;
            if (sheetId == ElementId.InvalidElementId || document.GetElement(sheetId) is not ViewSheet sheet)
            {
                return;
            }

            info.SheetId = RevitCompatibility.GetIdValue(sheetId);
            info.SheetNumber = sheet.SheetNumber;
            info.SheetName = sheet.Name;
        }
        catch
        {
            // ignore
        }
    }

    private static void ReadGroupAndAssembly(Element element, CoreModels.RelationshipInfo info)
    {
        try
        {
            var groupId = element.GroupId;
            if (groupId is not null && groupId != ElementId.InvalidElementId)
            {
                info.GroupId = RevitCompatibility.GetIdValue(groupId);
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            var assemblyId = element.AssemblyInstanceId;
            if (assemblyId is not null && assemblyId != ElementId.InvalidElementId)
            {
                info.AssemblyId = RevitCompatibility.GetIdValue(assemblyId);
            }
        }
        catch
        {
            // ignore
        }
    }

    private static void ReadDesignOption(Element element, CoreModels.RelationshipInfo info)
    {
        try
        {
            var designOption = element.DesignOption;
            if (designOption is not null)
            {
                info.DesignOptionId = RevitCompatibility.GetIdValue(designOption.Id);
            }
        }
        catch
        {
            // ignore
        }
    }

    private static void ReadPhases(Element element, CoreModels.RelationshipInfo info)
    {
        try
        {
            var createdId = element.CreatedPhaseId;
            if (createdId is not null && createdId != ElementId.InvalidElementId)
            {
                info.PhaseCreatedId = RevitCompatibility.GetIdValue(createdId);
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            var demolishedId = element.DemolishedPhaseId;
            if (demolishedId is not null && demolishedId != ElementId.InvalidElementId)
            {
                info.PhaseDemolishedId = RevitCompatibility.GetIdValue(demolishedId);
            }
        }
        catch
        {
            // ignore
        }
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
            return null;
        }
    }
}
