using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.UI.Support;

/// <summary>
/// Turns any Core model object into a flat list of Field/Value rows via reflection, so
/// Geometry/Location/Relationships can share one generic detail grid instead of a bespoke
/// form per tab (HANDOFF Section 22.1: "practical, not over-designed").
/// </summary>
public static class ObjectInspector
{
    public static IReadOnlyList<FieldRow> ToFieldRows(object? source, string prefix = "")
    {
        var rows = new List<FieldRow>();
        if (source is null)
        {
            return rows;
        }

        var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            var name = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
            object? value;
            try
            {
                value = property.GetValue(source);
            }
            catch
            {
                continue;
            }

            AppendValue(rows, name, value);
        }

        return rows;
    }

    private static void AppendValue(List<FieldRow> rows, string name, object? value)
    {
        switch (value)
        {
            case null:
                rows.Add(new FieldRow(name, string.Empty));
                break;

            case string s:
                rows.Add(new FieldRow(name, s));
                break;

            case Point3D point:
                rows.Add(new FieldRow(
                    name,
                    string.Format(CultureInfo.InvariantCulture, "{0:0.###}, {1:0.###}, {2:0.###}", point.X, point.Y, point.Z)));
                break;

            case IEnumerable enumerable:
                var items = enumerable.Cast<object?>().Select(FormatScalar).ToList();
                rows.Add(new FieldRow(name, items.Count == 0 ? string.Empty : string.Join(", ", items)));
                break;

            default:
                if (value.GetType().Namespace == typeof(ElementContextSnapshot).Namespace)
                {
                    rows.AddRange(ToFieldRows(value, name));
                }
                else
                {
                    rows.Add(new FieldRow(name, FormatScalar(value) ?? string.Empty));
                }

                break;
        }
    }

    private static string? FormatScalar(object? value) => value switch
    {
        null => null,
        double d => d.ToString("0.###", CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };
}
