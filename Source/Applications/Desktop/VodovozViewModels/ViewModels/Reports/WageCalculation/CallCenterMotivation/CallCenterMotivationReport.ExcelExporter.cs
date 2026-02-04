using System;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Vodovoz.ViewModels.ViewModels.Reports.WageCalculation.CallCenterMotivation
{
	public partial class CallCenterMotivationReport
	{
		public class ExcelExporter
		{
			private readonly CallCenterMotivationReport _report;

			#region Excel render properties

			private uint _defaultCellFormatId;
			private uint _defaultBoldFontCellFormatId;
			private uint _tableHeadersCellFormatId;
			private uint _tableHeadersWithRotationCellFormatId;
			private uint _parametersHeadersCellFormatId;
			private uint _parametersValuesCellFormatId;
			private uint _sheetTitleCellFormatId;
			private uint _moneyFormatId;
			private string _totalHeaderMergeRange;
			private const int _tableHeaderRowNum = 7;

			#endregion Excel render properties

			public ExcelExporter(CallCenterMotivationReport report)
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
					stylesPart.Stylesheet = GetStyleSheet();
					stylesPart.Stylesheet.Save();

					var columns = GetColumns();
					worksheetPart.Worksheet.Append(columns);

					var sheetData = GetSheetData();
					worksheetPart.Worksheet.Append(sheetData);

					var mergeCells = GetMergeCells();
					worksheetPart.Worksheet.Append(mergeCells);

					worksheetPart.Worksheet.Save();

					var sheet = new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };
					var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
					sheets.AppendChild(sheet);

					spreadsheet.WorkbookPart.Workbook.Save();
				}
			}

			private SheetData GetSheetData()
			{
				var sheetData = new SheetData();

				sheetData.Append(GetTableTitleRow());
				sheetData.Append(GetBlankRow());
				sheetData.Append(GetPropertiesHeadersRow());
				sheetData.Append(GetPropertiesValuesRow());
				sheetData.Append(GetGroupingValueRow());
				sheetData.Append(GetBlankRow());
				sheetData.Append(GetTableHeadersRow());
				sheetData.Append(GetTableSubHeadersRow());
				sheetData.Append(GetBlankRow());

				foreach(var node in _report.Rows)
				{
					sheetData.Append(GetTableDataRow(node));
				}

				return sheetData;
			}

			private MergeCells GetMergeCells()
			{
				var mergeCells = new MergeCells();

				mergeCells.Append(new MergeCell() { Reference = new StringValue("A1:D1") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("E1:R1") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("A3:B3") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("A4:B4") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("A5:F5") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("G3:R3") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("G4:R4") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("A7:B7") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue(_totalHeaderMergeRange) });

				var startColIndex = 2;
				var groupSize = _report.ShowDynamics ? 4 : 2;

				for(var col = startColIndex; col < startColIndex + _report.Slices.Count * groupSize; col += groupSize)
				{
					var col1 = GetExcelColumnName(col);
					var col2 = GetExcelColumnName(col + groupSize - 1);

					var range = $"{col1}{_tableHeaderRowNum}:{col2}{_tableHeaderRowNum}";
					mergeCells.Append(new MergeCell { Reference = range });
				}

				return mergeCells;
			}

			private static string GetExcelColumnName(int index)
			{
				var dividend = index + 1;
				var columnName = string.Empty;

				while(dividend > 0)
				{
					int modulo = (dividend - 1) % 26;
					columnName = (char)('A' + modulo) + columnName;
					dividend = (dividend - modulo) / 26;
				}

				return columnName;
			}


			private Columns GetColumns()
			{
				var dataColumnsStartIndex = 3;

				var dataColumnsLastIndex = dataColumnsStartIndex + (_report.Slices.Count * (_report.ShowDynamics ? 4 : 2)) + 1;

				var columns = new Columns();

				var rowIdColumn = CreateColumn(1, 6);
				var rowTitle = CreateColumn(2, 45);
				var rowData = CreateColumn(dataColumnsStartIndex, dataColumnsLastIndex, 12);
				var rowTotal = CreateColumn(dataColumnsLastIndex + 1, 12);

				columns.Append(rowIdColumn);
				columns.Append(rowTitle);
				columns.Append(rowData);
				columns.Append(rowTotal);

				return columns;
			}

			private Column CreateColumn(int columnId, double columnWidth)
			{
				var column = new Column
				{
					Min = (uint)columnId,
					Max = (uint)columnId,
					CustomWidth = true,
					Width = columnWidth
				};

				return column;
			}

			private Column CreateColumn(int columnMinId, int columnMaxId, double columnWidth)
			{
				var column = new Column
				{
					Min = (uint)columnMinId,
					Max = (uint)columnMaxId,
					CustomWidth = true,
					Width = columnWidth
				};

				return column;
			}

			private Row GetTableTitleRow()
			{
				var row = new Row();

				row.AppendChild(GetSheetTitleStringCell(_report.ReportTitle));
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(GetParametersValuesStringCell($"Дата и время формирования:  {_report.CreatedAt:dd.MM.yyyy HH:mm}"));

				return row;
			}

			private Row GetPropertiesHeadersRow()
			{
				var row = new Row();

				row.AppendChild(GetParametersHeaderStringCell("Настройки отчета:"));
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(GetParametersHeaderStringCell("Выбранные фильтры:"));

				return row;
			}

			private Row GetPropertiesValuesRow()
			{
				var row = new Row();

				row.CustomHeight = true;
				row.Height = 50;

				row.AppendChild(GetParametersValuesStringCell($"Разрез: {_report.SliceTypeString}"));
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(GetParametersValuesStringCell(_report.Filters));

				return row;
			}

			private Row GetGroupingValueRow()
			{
				var row = new Row();

				row.AppendChild(GetParametersValuesStringCell($"Группировка: {_report.GroupingTitle}"));

				return row;
			}

			private Row GetTableHeadersRow()
			{
				var row = new Row();

				row.AppendChild(GetTableHeaderStringCell(_report.GroupingTitle));
				row.AppendChild(GetTableHeaderStringCell(""));
				row.CustomHeight = true;
				row.Height = 80;

				var sliceSubColumnsCount = _report.ShowDynamics ? 3 : 1;
				foreach(var slice in _report.Slices)
				{
					var cell = GetTableHeaderWithRotationStringCell(slice.StartDate.ToString("dd.MM.yyyy"));

					row.AppendChild(cell);

					for(var i = 0; i < sliceSubColumnsCount; i++)
					{
						row.AppendChild(new Cell());
					}
				}

				var lastCell = row.AppendChild(GetTableHeaderStringCell("Всего за период"));

				var lastColIndex = row.Elements<Cell>().ToList().IndexOf(lastCell) + 1;
				var lastCellAddress = $"{GetColumnLetter(lastColIndex)}{_tableHeaderRowNum}";
				var nextCellAddress = $"{GetColumnLetter(lastColIndex + 1)}{_tableHeaderRowNum}";

				_totalHeaderMergeRange = $"{lastCellAddress}:{nextCellAddress}";

				return row;
			}

			private static string GetColumnLetter(int colNumber)
			{
				var letters = "";
				colNumber--;

				while(colNumber >= 0)
				{
					letters = (char)('A' + colNumber % 26) + letters;
					colNumber = colNumber / 26 - 1;
				}

				return letters;
			}

			private Row GetTableSubHeadersRow()
			{
				var row = new Row();

				row.AppendChild(new Cell());
				row.AppendChild(new Cell());

				for(var i = 0; i < _report.Slices.Count; i++)
				{
					row.AppendChild(GetStringCell("Продано"));
					row.AppendChild(GetStringCell("Премия"));

					if(_report.ShowDynamics)
					{
						row.AppendChild(GetStringCell("Δ продано"));
						row.AppendChild(GetStringCell("Δ премия"));
					}
				}

				row.AppendChild(GetStringCell("Продано"));
				row.AppendChild(GetStringCell("Премия"));

				return row;
			}

			private Row GetTableDataRow(CallCenterMotivationReportRow node)
			{
				var row = new Row();

				row.AppendChild(GetStringCell(node.Index, node.IsTotalsRow));
				row.AppendChild(GetStringCell(node.Title, node.IsTotalsRow));

				for(var i = 0; i < _report.Slices.Count; i++)
				{
					var index = i;

					var sliceColumn = node.SliceColumnValues[index];
					
					var soldSliceCell = node.HideSold ? GetStringCell(string.Empty) : GetFloatingPointCell(sliceColumn.Sold, node.IsMoneyFormat);

					row.AppendChild(soldSliceCell);
					row.AppendChild(GetFloatingPointCell(sliceColumn.Premium, true));

					if(_report.ShowDynamics)
					{
						if(index == 0)
						{
							row.AppendChild(GetStringCell(string.Empty));
							row.AppendChild(GetStringCell(string.Empty));
						}
						else
						{
							var dynamicColumn = node.DynamicColumns[index - 1];
							var soldDynamicCell = node.HideSold ? GetStringCell("") : GetFloatingPointCell(dynamicColumn.Sold, node.IsMoneyFormat);
							row.AppendChild(soldDynamicCell);
							row.AppendChild(GetFloatingPointCell(dynamicColumn.Premium, true));
						}
					}
				}

				var totalSliceCell = node.HideSold
					? GetStringCell(string.Empty)
					: GetFloatingPointCell(node.SliceColumnValues.Sum(x => x.Sold), node.IsMoneyFormat);
				
				row.AppendChild(totalSliceCell);
				row.AppendChild(GetFloatingPointCell(node.SliceColumnValues.Sum(x => x.Premium), true));

				return row;
			}

			private Row GetBlankRow()
			{
				var row = new Row();

				return row;
			}

			private Stylesheet GetStyleSheet()
			{
				var stylesheet = new Stylesheet();

				stylesheet.NumberingFormats = new NumberingFormats();
				uint moneyFormatId = 164; // 0–163 зарезервированы под встроенные форматы Excel 
				stylesheet.NumberingFormats.AppendChild(new NumberingFormat()
				{
					NumberFormatId = moneyFormatId,
					FormatCode = "#,##0.00 [$₽-419]"
				});
				stylesheet.NumberingFormats.Count = 1;

				stylesheet.Fonts = new Fonts();
				stylesheet.Fonts.AppendChild(GetDefaultFont());
				uint defaultFontId = 0;

				stylesheet.Fonts.AppendChild(GetDefaultBoldFont());
				uint defaultBoldFontId = 1;

				stylesheet.Fonts.AppendChild(GetWorksheetTitleFont());
				uint sheetTitleFontId = 2;

				stylesheet.Fonts.Count = 3;

				stylesheet.Fills = new Fills();
				stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } });
				stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } });

				stylesheet.Borders = new Borders();
				stylesheet.Borders.AppendChild(new Border());
				stylesheet.Borders.Count = 1;

				var defaultCellFormat = CreateCellFormat(defaultFontId);

				var boldTextCellFormat = CreateCellFormat(defaultBoldFontId);

				var tableHeadersCellFormat = CreateCellFormat(defaultBoldFontId, isWrapText: true);
				tableHeadersCellFormat.Alignment.Vertical = VerticalAlignmentValues.Center;
				tableHeadersCellFormat.Alignment.Horizontal = HorizontalAlignmentValues.Center;

				var tableHeadersWithRotationCellFormat = CreateCellFormat(defaultBoldFontId, isRotateText: true);
				tableHeadersWithRotationCellFormat.Alignment.Horizontal = HorizontalAlignmentValues.Center;
				tableHeadersWithRotationCellFormat.Alignment.Vertical = VerticalAlignmentValues.Center;

				var parametersHeadersCellFormat = CreateCellFormat(defaultBoldFontId);

				var parametersValuesCellFormat = CreateCellFormat(defaultFontId, isWrapText: true);
				parametersValuesCellFormat.Alignment.Vertical = VerticalAlignmentValues.Top;

				var sheetTitleCellFormat = CreateCellFormat(sheetTitleFontId);

				var moneyCellFormat = CreateCellFormat(defaultFontId);
				moneyCellFormat.NumberFormatId = moneyFormatId;
				moneyCellFormat.ApplyNumberFormat = true;

				stylesheet.CellStyleFormats = new CellStyleFormats();
				stylesheet.CellStyleFormats.AppendChild(new CellFormat());
				stylesheet.CellFormats = new CellFormats();
				stylesheet.CellFormats.AppendChild(new CellFormat());

				stylesheet.CellFormats.AppendChild(defaultCellFormat);
				_defaultCellFormatId = 1;

				stylesheet.CellFormats.AppendChild(boldTextCellFormat);
				_defaultBoldFontCellFormatId = 2;

				stylesheet.CellFormats.AppendChild(tableHeadersCellFormat);
				_tableHeadersCellFormatId = 3;

				stylesheet.CellFormats.AppendChild(tableHeadersWithRotationCellFormat);
				_tableHeadersWithRotationCellFormatId = 4;

				stylesheet.CellFormats.AppendChild(parametersHeadersCellFormat);
				_parametersHeadersCellFormatId = 5;

				stylesheet.CellFormats.AppendChild(parametersValuesCellFormat);
				_parametersValuesCellFormatId = 6;

				stylesheet.CellFormats.AppendChild(sheetTitleCellFormat);
				_sheetTitleCellFormatId = 7;

				stylesheet.CellFormats.AppendChild(moneyCellFormat);
				_moneyFormatId = 8;

				stylesheet.CellFormats.Count = 9;

				return stylesheet;
			}

			private CellFormat CreateCellFormat(
				uint fontId,
				bool isWrapText = false,
				bool isRotateText = false)
			{
				var aligment = new Alignment();
				aligment.WrapText = isWrapText;

				if(isRotateText)
				{
					aligment.TextRotation = 90;
				}

				var cellFormat = new CellFormat
				{
					FormatId = 0,
					FontId = fontId,
					Alignment = aligment
				};

				return cellFormat;
			}

			private Cell GetSheetTitleStringCell(string value)
			{
				var cell = new Cell
				{
					CellValue = new CellValue(value),
					DataType = CellValues.String,
					StyleIndex = _sheetTitleCellFormatId
				};

				return cell;
			}

			private Cell GetParametersHeaderStringCell(string value)
			{
				var cell = new Cell
				{
					CellValue = new CellValue(value),
					DataType = CellValues.String,
					StyleIndex = _parametersHeadersCellFormatId
				};

				return cell;
			}

			private Cell GetParametersValuesStringCell(string value)
			{
				var cell = new Cell
				{
					CellValue = new CellValue(value),
					DataType = CellValues.String,
					StyleIndex = _parametersValuesCellFormatId
				};

				return cell;
			}

			private Cell GetTableHeaderStringCell(string value)
			{
				var cell = new Cell
				{
					CellValue = new CellValue(value),
					DataType = CellValues.String,
					StyleIndex = _tableHeadersCellFormatId
				};

				return cell;
			}

			private Cell GetTableHeaderWithRotationStringCell(string value)
			{
				var cell = new Cell
				{
					CellValue = new CellValue(value),
					DataType = CellValues.String,
					StyleIndex = _tableHeadersWithRotationCellFormatId
				};

				return cell;
			}

			private Cell GetStringCell(string value, bool isBold = false)
			{
				var cell = new Cell
				{
					CellValue = new CellValue(value),
					DataType = CellValues.String,
					StyleIndex = isBold ? _defaultBoldFontCellFormatId : _defaultCellFormatId
				};

				return cell;
			}

			private Cell GetNumericCell(int value, bool isBold = false)
			{
				var cell = new Cell
				{
					CellValue = new CellValue(value),
					DataType = CellValues.Number,
					StyleIndex = isBold ? _defaultBoldFontCellFormatId : _defaultCellFormatId
				};

				return cell;
			}

			private Cell GetFloatingPointCell(decimal value, bool isMoneyFormat)
			{
				var cell = new Cell
				{
					CellValue = new CellValue(value),
					DataType = CellValues.Number,
					StyleIndex = isMoneyFormat ? _moneyFormatId : _defaultCellFormatId
				};

				return cell;
			}

			private Font GetDefaultFont()
			{
				var fontSize = new FontSize
				{
					Val = 12
				};

				var font = new Font
				{
					FontSize = fontSize
				};

				return font;
			}

			private Font GetDefaultBoldFont()
			{
				var bold = new Bold();

				var fontSize = new FontSize
				{
					Val = 12
				};

				var font = new Font
				{
					Bold = bold,
					FontSize = fontSize
				};

				return font;
			}

			private Font GetWorksheetTitleFont()
			{
				var bold = new Bold();

				var fontSize = new FontSize
				{
					Val = 14
				};

				var font = new Font
				{
					Bold = bold,
					FontSize = fontSize
				};

				return font;
			}
		}
	}
}
