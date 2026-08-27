using System;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RpiLabelGen
{
    /// <summary>
    /// One-shot dev tool: Add-Ins &gt; External Tools &gt; "Dump Revit Labels". Writes
    /// %LOCALAPPDATA%\RpiLabelGen\labels-&lt;version&gt;.json for the running Revit's UI language.
    /// Read-only - it never touches the model, and works with no document open.
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public sealed class DumpLabelsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var app = commandData.Application.Application;
                var outputPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "RpiLabelGen",
                    "labels-" + app.VersionNumber + ".json");

                var summary = LabelDumper.Dump(app, outputPath);
                TaskDialog.Show("RpiLabelGen", summary);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                return Result.Failed;
            }
        }
    }
}
