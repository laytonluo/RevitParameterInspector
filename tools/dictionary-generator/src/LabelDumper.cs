using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace RpiLabelGen
{
    /// <summary>
    /// Dumps every BuiltInCategory / BuiltInParameter enum name together with the label Revit's
    /// own UI shows for it (LabelUtils), in whatever language the running Revit is localized to.
    /// Output is a small raw JSON file; turning that into RevitParameterInspector's dictionary
    /// schema happens outside Revit.
    ///
    /// Deliberately dependency-free (manual JSON escaping, no Newtonsoft/System.Text.Json) so it
    /// can run either from a one-shot IExternalCommand or pasted into an MCP code-execution body.
    /// </summary>
    public static class LabelDumper
    {
        /// <summary>Writes the dump and returns a one-line summary.</summary>
        public static string Dump(Application app, string outputPath)
        {
            var categories = DumpEnum<BuiltInCategory>(GetCategoryLabel);
            var parameters = DumpEnum<BuiltInParameter>(GetParameterLabel);

            var json = new StringBuilder();
            json.Append('{');
            AppendString(json, "revitVersion", app.VersionNumber);
            json.Append(',');
            AppendString(json, "revitBuild", app.VersionBuild);
            json.Append(',');
            AppendString(json, "language", app.Language.ToString());
            json.Append(',');
            AppendString(json, "generatedAt", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            json.Append(',');
            AppendEntries(json, "categories", categories);
            json.Append(',');
            AppendEntries(json, "parameters", parameters);
            json.Append('}');

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // UTF-8 without BOM: the post-processing step and git both prefer it.
            File.WriteAllText(outputPath, json.ToString(), new UTF8Encoding(false));

            return string.Format(
                "Revit {0} ({1}), language {2}: {3} categories, {4} parameters -> {5}",
                app.VersionNumber,
                app.VersionBuild,
                app.Language,
                categories.Count,
                parameters.Count,
                outputPath);
        }

        /// <summary>
        /// Iterates Enum.GetNames rather than Enum.GetValues: BuiltInParameter has aliases
        /// (several names sharing one numeric value) and GetValues collapses those, silently
        /// dropping names the dictionary is supposed to key on.
        /// </summary>
        private static List<KeyValuePair<string, string>> DumpEnum<TEnum>(Func<string, string> getLabel)
            where TEnum : struct
        {
            var results = new List<KeyValuePair<string, string>>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var name in Enum.GetNames(typeof(TEnum)))
            {
                if (name == "INVALID" || !seen.Add(name))
                {
                    continue;
                }

                string label;
                try
                {
                    label = getLabel(name);
                }
                catch
                {
                    // Plenty of enum members have no user-facing label at all; that is expected,
                    // not an error - they simply do not belong in a terminology dictionary.
                    continue;
                }

                if (string.IsNullOrWhiteSpace(label))
                {
                    continue;
                }

                results.Add(new KeyValuePair<string, string>(name, label));
            }

            return results;
        }

        private static string GetCategoryLabel(string name)
        {
            var value = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), name);
            return LabelUtils.GetLabelFor(value);
        }

        private static string GetParameterLabel(string name)
        {
            var value = (BuiltInParameter)Enum.Parse(typeof(BuiltInParameter), name);
            return LabelUtils.GetLabelFor(value);
        }

        private static void AppendEntries(StringBuilder json, string propertyName, List<KeyValuePair<string, string>> entries)
        {
            json.Append('"').Append(propertyName).Append("\":[");
            for (var i = 0; i < entries.Count; i++)
            {
                if (i > 0)
                {
                    json.Append(',');
                }

                json.Append('{');
                AppendString(json, "n", entries[i].Key);
                json.Append(',');
                AppendString(json, "l", entries[i].Value);
                json.Append('}');
            }

            json.Append(']');
        }

        private static void AppendString(StringBuilder json, string propertyName, string value)
        {
            json.Append('"').Append(propertyName).Append("\":\"");
            Escape(json, value ?? string.Empty);
            json.Append('"');
        }

        private static void Escape(StringBuilder json, string value)
        {
            foreach (var c in value)
            {
                switch (c)
                {
                    case '"': json.Append("\\\""); break;
                    case '\\': json.Append("\\\\"); break;
                    case '\b': json.Append("\\b"); break;
                    case '\f': json.Append("\\f"); break;
                    case '\n': json.Append("\\n"); break;
                    case '\r': json.Append("\\r"); break;
                    case '\t': json.Append("\\t"); break;
                    default:
                        if (c < ' ')
                        {
                            json.Append("\\u").Append(((int)c).ToString("x4"));
                        }
                        else
                        {
                            // CJK stays as literal UTF-8; the file is written as UTF-8.
                            json.Append(c);
                        }

                        break;
                }
            }
        }
    }
}
