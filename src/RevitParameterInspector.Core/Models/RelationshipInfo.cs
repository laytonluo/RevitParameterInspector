namespace RevitParameterInspector.Core.Models;

/// <summary>
/// Related Revit objects (type, family, host, level, room/space, view/sheet, etc.).
/// V1 displays ids/names only; recursive inspection of related elements is deferred.
/// See HANDOFF Section 16.
/// </summary>
public sealed class RelationshipInfo
{
    public long? TypeElementId { get; set; }
    public string? TypeName { get; set; }

    public long? FamilyId { get; set; }
    public string? FamilyName { get; set; }

    public long? HostElementId { get; set; }
    public string? HostName { get; set; }

    public long? LevelId { get; set; }
    public string? LevelName { get; set; }

    public long? RoomId { get; set; }
    public string? RoomName { get; set; }

    public long? SpaceId { get; set; }
    public string? SpaceName { get; set; }

    public List<long> MaterialIds { get; set; } = new();

    public long? ViewOwnerId { get; set; }
    public string? ViewOwnerName { get; set; }

    public long? SheetId { get; set; }
    public string? SheetNumber { get; set; }
    public string? SheetName { get; set; }

    public long? GroupId { get; set; }
    public long? AssemblyId { get; set; }
    public long? DesignOptionId { get; set; }
    public long? PhaseCreatedId { get; set; }
    public long? PhaseDemolishedId { get; set; }

    public string? LinkedDocumentTitle { get; set; }
    public long? LinkedElementId { get; set; }
}
