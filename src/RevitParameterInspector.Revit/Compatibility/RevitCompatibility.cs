using Autodesk.Revit.DB;

namespace RevitParameterInspector.Revit.Compatibility;

/// <summary>
/// Single seam for Revit-version differences (HANDOFF Section 7). V1's minimum supported
/// version is Revit 2024, where ElementId storage is already 64-bit, so there is no
/// IntegerValue/Value branching to do today. Future version-specific quirks (2025/2026)
/// should be added here rather than scattered across readers/builders.
/// </summary>
public static class RevitCompatibility
{
    public static long GetIdValue(ElementId id) => id.Value;
}
