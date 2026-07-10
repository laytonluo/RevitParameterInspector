namespace RevitParameterInspector.Core.Models;

/// <summary>
/// Which kind of API identifier a dictionary lookup was for. Used to group unresolved
/// terms on the Dictionary page so they can serve as a build list for future dictionary
/// content. IfcParameter/AnalyticalParameter have no readers yet and stay empty for now.
/// </summary>
public enum DictionaryTermCategory
{
    ApiTerm,
    BuiltInCategory,
    BuiltInParameter,
    IfcParameter,
    AnalyticalParameter,
}
