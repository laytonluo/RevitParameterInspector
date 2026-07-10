using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.Core.Support;

namespace RevitParameterInspector.Export;

/// <summary>
/// Excel export: BIM-user-friendly table format with one sheet per section. See HANDOFF
/// Section 32.3. CSV export is intentionally not implemented (Section 32.4).
/// </summary>
public static class ExcelExporter
{
    /// <summary>Writes the snapshot to the exact <paramref name="filePath"/> given (see <see cref="ExportFileNaming"/> for a suggested name/path).</summary>
    public static string ExportToFile(ElementContextSnapshot snapshot, string filePath)
    {
        var timestamp = DateTimeOffset.UtcNow;

        snapshot.ExportMetadata = new ExportMetadata
        {
            ExportFormat = "Excel",
            FileName = Path.GetFileName(filePath),
            ExportedAt = timestamp,
            ToolName = "RevitParameterInspector",
            ToolVersion = snapshot.AddinVersion,
        };

        using var workbook = new XLWorkbook();

        AddSummarySheet(workbook, snapshot);
        AddParametersSheet(workbook, "Parameters_All", snapshot.Parameters);
        AddParametersSheet(workbook, "Parameters_Instance", snapshot.Parameters.Where(p => p.Scope == ParameterScope.Instance).ToList());
        AddParametersSheet(workbook, "Parameters_Type", snapshot.Parameters.Where(p => p.Scope == ParameterScope.Type).ToList());
        AddFieldValueSheet(workbook, "Geometry", ObjectInspector.ToFieldRows(snapshot.Geometry));
        AddFieldValueSheet(workbook, "Location", ObjectInspector.ToFieldRows(snapshot.Location));
        AddFieldValueSheet(workbook, "Relationships", ObjectInspector.ToFieldRows(snapshot.Relationships));
        AddViewSheetContextSheet(workbook, snapshot);
        AddDictionarySheet(workbook, snapshot);
        AddRawMetadataSheet(workbook, snapshot);

        workbook.SaveAs(filePath);
        return filePath;
    }

    private static void AddSummarySheet(XLWorkbook workbook, ElementContextSnapshot snapshot)
    {
        var identity = snapshot.Identity;
        var rows = new List<FieldRow>
        {
            new("Element Id", identity?.ElementIdString ?? string.Empty),
            new("Unique Id", identity?.UniqueId ?? string.Empty),
            new("Class Name", identity?.ClassName ?? string.Empty),
            new("Category", identity?.CategoryName ?? string.Empty),
            new("Built-in Category", identity?.BuiltInCategory ?? string.Empty),
            new("Name", identity?.Name ?? string.Empty),
            new("Family", identity?.FamilyName ?? string.Empty),
            new("Type", identity?.TypeName ?? string.Empty),
            new("Document", identity?.DocumentTitle ?? string.Empty),
            new("Is Linked Element", identity?.IsLinkedElement.ToString() ?? string.Empty),
        };

        AddFieldValueSheet(workbook, "Summary", rows);
    }

    private static void AddFieldValueSheet(XLWorkbook workbook, string sheetName, IReadOnlyList<FieldRow> rows)
    {
        var sheet = workbook.Worksheets.Add(sheetName);
        WriteHeader(sheet, "Field", "Value");

        var rowIndex = 2;
        foreach (var row in rows)
        {
            sheet.Cell(rowIndex, 1).Value = row.Field;
            sheet.Cell(rowIndex, 2).Value = row.Value;
            rowIndex++;
        }

        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
    }

    private static void AddParametersSheet(XLWorkbook workbook, string sheetName, IReadOnlyList<ParameterInfoRecord> parameters)
    {
        var sheet = workbook.Worksheets.Add(sheetName);
        WriteHeader(
            sheet,
            "Name", "Localized Name", "Scope", "Value Display", "Value Raw", "Storage Type",
            "Built-in Parameter", "Parameter Kind", "Is Read Only", "Has Value", "API Path", "Description");

        var rowIndex = 2;
        foreach (var parameter in parameters)
        {
            sheet.Cell(rowIndex, 1).Value = parameter.Name ?? string.Empty;
            sheet.Cell(rowIndex, 2).Value = parameter.LocalizedName ?? string.Empty;
            sheet.Cell(rowIndex, 3).Value = parameter.Scope.ToString();
            sheet.Cell(rowIndex, 4).Value = parameter.ValueDisplay ?? string.Empty;
            sheet.Cell(rowIndex, 5).Value = parameter.ValueRaw ?? string.Empty;
            sheet.Cell(rowIndex, 6).Value = parameter.StorageType ?? string.Empty;
            sheet.Cell(rowIndex, 7).Value = parameter.BuiltInParameter ?? string.Empty;
            sheet.Cell(rowIndex, 8).Value = parameter.ParameterKind.ToString();
            sheet.Cell(rowIndex, 9).Value = parameter.IsReadOnly;
            sheet.Cell(rowIndex, 10).Value = parameter.HasValue;
            sheet.Cell(rowIndex, 11).Value = parameter.ApiPath ?? string.Empty;
            sheet.Cell(rowIndex, 12).Value = parameter.Description ?? string.Empty;
            rowIndex++;
        }

        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
    }

    private static void AddViewSheetContextSheet(XLWorkbook workbook, ElementContextSnapshot snapshot)
    {
        var sheet = workbook.Worksheets.Add("ViewSheet_Context");
        WriteHeader(sheet, "ContextType", "Name", "ElementId", "UniqueId", "AdditionalInfo");

        var rowIndex = 2;
        foreach (var context in snapshot.ViewSheetContexts)
        {
            sheet.Cell(rowIndex, 1).Value = context.ContextType ?? string.Empty;
            sheet.Cell(rowIndex, 2).Value = context.Name ?? string.Empty;
            sheet.Cell(rowIndex, 3).Value = context.ElementId ?? string.Empty;
            sheet.Cell(rowIndex, 4).Value = context.UniqueId ?? string.Empty;
            sheet.Cell(rowIndex, 5).Value = context.AdditionalInfo ?? string.Empty;
            rowIndex++;
        }

        if (snapshot.ViewSheetContexts.Count == 0)
        {
            sheet.Cell(2, 1).Value = "No View / Sheet context found.";
        }

        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
    }

    private static void AddDictionarySheet(XLWorkbook workbook, ElementContextSnapshot snapshot)
    {
        var sheet = workbook.Worksheets.Add("Dictionary");
        WriteHeader(sheet, "Term Key", "API Name", "Localized Name", "Locale", "Description", "Status", "Source", "Notes");

        var rowIndex = 2;
        foreach (var term in snapshot.Dictionary)
        {
            sheet.Cell(rowIndex, 1).Value = term.TermKey ?? string.Empty;
            sheet.Cell(rowIndex, 2).Value = term.ApiName ?? string.Empty;
            sheet.Cell(rowIndex, 3).Value = term.LocalizedName ?? string.Empty;
            sheet.Cell(rowIndex, 4).Value = term.Locale ?? string.Empty;
            sheet.Cell(rowIndex, 5).Value = term.Description ?? string.Empty;
            sheet.Cell(rowIndex, 6).Value = term.Status.ToString();
            sheet.Cell(rowIndex, 7).Value = term.Source ?? string.Empty;
            sheet.Cell(rowIndex, 8).Value = term.Notes ?? string.Empty;
            rowIndex++;
        }

        if (snapshot.Dictionary.Count == 0)
        {
            sheet.Cell(2, 1).Value = "No dictionary is loaded in this build; all names are raw Revit API names.";
        }

        if (snapshot.UnresolvedDictionaryTerms.Count > 0)
        {
            rowIndex++;
            sheet.Cell(rowIndex, 1).Value = "Unresolved Terms";
            sheet.Cell(rowIndex, 1).Style.Font.Bold = true;
            sheet.Cell(rowIndex, 2).Value = "Category";
            sheet.Cell(rowIndex, 2).Style.Font.Bold = true;
            rowIndex++;

            foreach (var term in snapshot.UnresolvedDictionaryTerms)
            {
                sheet.Cell(rowIndex, 1).Value = term.Term ?? string.Empty;
                sheet.Cell(rowIndex, 2).Value = term.Category.ToString();
                rowIndex++;
            }
        }

        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
    }

    private static void AddRawMetadataSheet(XLWorkbook workbook, ElementContextSnapshot snapshot)
    {
        var rows = new List<FieldRow>
        {
            new("Schema Version", snapshot.SchemaVersion),
            new("Generated At", snapshot.GeneratedAt.ToString("O")),
            new("Revit Version", snapshot.RevitVersion ?? string.Empty),
            new("Addin Version", snapshot.AddinVersion ?? string.Empty),
            new("Document Title", snapshot.Document?.Title ?? string.Empty),
            new("Document Path", snapshot.Document?.PathName ?? string.Empty),
            new("Is Workshared", snapshot.Document?.IsWorkshared.ToString() ?? string.Empty),
            new("Export Format", snapshot.ExportMetadata?.ExportFormat ?? string.Empty),
            new("Export File Name", snapshot.ExportMetadata?.FileName ?? string.Empty),
            new("Exported At", snapshot.ExportMetadata?.ExportedAt?.ToString("O") ?? string.Empty),
        };

        foreach (var entry in snapshot.Raw)
        {
            rows.Add(new FieldRow($"Raw.{entry.Key}", entry.Value));
        }

        AddFieldValueSheet(workbook, "Raw_Metadata", rows);
    }

    private static void WriteHeader(IXLWorksheet sheet, params string[] headers)
    {
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
        }
    }
}
