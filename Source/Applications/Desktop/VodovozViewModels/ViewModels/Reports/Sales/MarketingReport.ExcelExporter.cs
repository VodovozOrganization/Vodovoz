using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class MarketingReport
	{
		public class ExcelExporter
		{
			private readonly MarketingReport _report;

			private uint _defaultFormatId;
			private uint _boldFormatId;
			private uint _headerFormatId;

			public ExcelExporter(MarketingReport report)
			{
				_report = report ?? throw new ArgumentNullException(nameof(report));
			}

			public void Export(string path)
			{
				using(var spreadsheet = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
				{
					spreadsheet.AddWorkbookPart();
					spreadsheet.WorkbookPart.Workbook = new Workbook();

					var worksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
					worksheetPart.Worksheet = new Worksheet();

					var stylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>();
					stylesPart.Stylesheet = BuildStylesheet();
					stylesPart.Stylesheet.Save();

					worksheetPart.Worksheet.Append(BuildColumns());
					worksheetPart.Worksheet.Append(BuildSheetData());
					worksheetPart.Worksheet.Save();

					var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
					sheets.AppendChild(new Sheet
					{
						Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
						SheetId = 1,
						Name = "Маркетинговый отчет"
					});

					spreadsheet.WorkbookPart.Workbook.Save();
				}
			}

			private SheetData BuildSheetData()
			{
				var sheetData = new SheetData();
				var rowIndex = 1u;

				var headerRow = CreateRow(rowIndex);
				headerRow.Append(CreateStringCell("A", rowIndex, "Показатель", _headerFormatId));
				headerRow.Append(CreateStringCell("B", rowIndex, "Значение", _headerFormatId));
				headerRow.Append(CreateStringCell("C", rowIndex, "Дополнительно", _headerFormatId));
				sheetData.Append(headerRow);
				rowIndex++;

				foreach(var displayRow in _report.DisplayRows)
				{
					var row = CreateRow(rowIndex);
					var styleId = displayRow.IsSection ? _boldFormatId : _defaultFormatId;
					row.Append(CreateStringCell("A", rowIndex, displayRow.Title ?? string.Empty, styleId));
					row.Append(CreateStringCell("B", rowIndex, displayRow.Value ?? string.Empty, styleId));
					row.Append(CreateStringCell("C", rowIndex, displayRow.AdditionalValue ?? string.Empty, styleId));
					sheetData.Append(row);
					rowIndex++;
				}

				return sheetData;
			}

			private Columns BuildColumns()
			{
				return new Columns(
					CreateColumn(1, 48),
					CreateColumn(2, 20),
					CreateColumn(3, 20));
			}

			private static Column CreateColumn(uint min, double width)
			{
				return new Column
				{
					Min = min,
					Max = min,
					Width = width,
					CustomWidth = true
				};
			}

			private static Row CreateRow(uint rowIndex)
			{
				return new Row { RowIndex = rowIndex };
			}

			private Cell CreateStringCell(string column, uint rowIndex, string value, uint styleIndex = 0)
			{
				return new Cell
				{
					CellReference = column + rowIndex,
					StyleIndex = styleIndex,
					DataType = CellValues.String,
					CellValue = new CellValue(value ?? string.Empty)
				};
			}

			private Stylesheet BuildStylesheet()
			{
				_defaultFormatId = 1;
				_boldFormatId = 2;
				_headerFormatId = 3;

				return new Stylesheet(
					new Fonts(
						new Font(new FontSize { Val = 10 }, new FontName { Val = "Arial" }),
						new Font(new Bold(), new FontSize { Val = 10 }, new FontName { Val = "Arial" }))
					{ Count = 2 },
					new Fills(
						new Fill(new PatternFill { PatternType = PatternValues.None }),
						new Fill(new PatternFill { PatternType = PatternValues.Gray125 }))
					{ Count = 2 },
					new Borders(new Border())
					{ Count = 1 },
					new CellStyleFormats(new CellFormat())
					{ Count = 1 },
					new CellFormats(
						new CellFormat(),
						CreateCellFormat(0),
						CreateCellFormat(1),
						CreateCellFormat(1))
					{ Count = 4 });
			}

			private static CellFormat CreateCellFormat(uint fontId)
			{
				return new CellFormat
				{
					FontId = fontId,
					ApplyFont = true
				};
			}
		}
	}
}
