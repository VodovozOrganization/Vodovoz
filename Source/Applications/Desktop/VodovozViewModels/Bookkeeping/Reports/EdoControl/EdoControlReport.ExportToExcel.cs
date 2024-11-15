using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl
{
	public partial class EdoControlReport
	{
		private uint _defaultCellFormatId;
		private uint _defaultBoldFontCellFormatId;
		private uint _tableHeadersCellFormatId;
		private uint _sheetTitleCellFormatId;
		public string ReportTitle =>
			$"Контроль за ЭДО за период с {StartDate.ToString(_dateFormatString)} по {EndDate.ToString(_dateFormatString)}";

		public void ExportToExcel(string path)
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

				worksheetPart.Worksheet.Save();

				var sheet = new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Контроль ЭДО" };
				var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
				sheets.AppendChild(sheet);

				spreadsheet.WorkbookPart.Workbook.Save();
			}
		}

		#region Stylesheet

		private Stylesheet GetStyleSheet()
		{
			var stylesheet = new Stylesheet();

			stylesheet.Fonts = new Fonts();
			stylesheet.Fonts.AppendChild(DefaultFont);
			uint defaultFontId = 0;

			stylesheet.Fonts.AppendChild(DefaultBoldFont);
			uint defaultBoldFontId = 1;

			stylesheet.Fonts.AppendChild(WorksheetTitleFont);
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

			var sheetTitleCellFormat = CreateCellFormat(sheetTitleFontId);

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

			stylesheet.CellFormats.AppendChild(sheetTitleCellFormat);
			_sheetTitleCellFormatId = 4;

			stylesheet.CellFormats.Count = 5;

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

		private Font DefaultFont => GetFont();

		private Font DefaultBoldFont => GetFont(isBold: true);

		private Font WorksheetTitleFont => GetFont(14, true);

		private Font GetFont(
			double size = 12,
			bool isBold = false)
		{
			var fontSize = new FontSize
			{
				Val = size
			};

			var font = new Font
			{
				FontSize = fontSize
			};

			if(isBold)
			{
				var bold = new Bold();
				font.Bold = bold;
			}

			return font;
		}

		#endregion Stylesheet

		#region Columns

		private Columns GetColumns()
		{
			var startColumnIndex = 1;

			var columns = new Columns();

			var column1 = CreateColumn(startColumnIndex++, 10);
			var column2 = CreateColumn(startColumnIndex++, 60);
			var column3 = CreateColumn(startColumnIndex++, 15);
			var column4 = CreateColumn(startColumnIndex++, 15);
			var column5 = CreateColumn(startColumnIndex++, 20);
			var column6 = CreateColumn(startColumnIndex++, 35);
			var column7 = CreateColumn(startColumnIndex++, 20);
			var column8 = CreateColumn(startColumnIndex++, 30);

			columns.Append(column1);
			columns.Append(column2);
			columns.Append(column3);
			columns.Append(column4);
			columns.Append(column5);
			columns.Append(column6);
			columns.Append(column7);
			columns.Append(column8);

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

		#endregion Columns

		#region Sheet Data

		private SheetData GetSheetData()
		{
			var sheetData = new SheetData();

			sheetData.Append(GetReportTitleRow());
			sheetData.Append(GetBlankRow());
			sheetData.Append(GetTableHeadersRow());

			foreach(var row in Rows)
			{
				sheetData.Append(GetTableDataRow(row));
			}

			return sheetData;
		}

		private Row GetBlankRow()
		{
			var row = new Row();

			return row;
		}

		private Row GetReportTitleRow()
		{
			var row = new Row();

			row.AppendChild(GetSheetTitleStringCell(ReportTitle));

			return row;
		}

		private Row GetTableHeadersRow()
		{
			var row = new Row();

			row.AppendChild(GetTableHeaderStringCell("Номер в ЭДО"));
			row.AppendChild(GetTableHeaderStringCell("Клиент"));
			row.AppendChild(GetTableHeaderStringCell("Номер заказа"));
			row.AppendChild(GetTableHeaderStringCell("Номер МЛ"));
			row.AppendChild(GetTableHeaderStringCell("Дата"));
			row.AppendChild(GetTableHeaderStringCell("Статус документооборота"));
			row.AppendChild(GetTableHeaderStringCell("Тип доставки"));
			row.AppendChild(GetTableHeaderStringCell("Тип переноса"));

			return row;
		}

		private Row GetTableDataRow(EdoControlReportRow node)
		{
			var row = new Row();

			row.AppendChild(GetStringCell(node.EdoContainerId));
			row.AppendChild(GetStringCell(node.IsRootRow ? node.GroupTitle : node.ClientName, node.IsRootRow));
			row.AppendChild(GetStringCell(node.OrderId));
			row.AppendChild(GetStringCell(node.RouteListId));
			row.AppendChild(GetStringCell(node.DeliveryDate));
			row.AppendChild(GetStringCell(node.EdoStatus));
			row.AppendChild(GetStringCell(node.OrderDeliveryType));
			row.AppendChild(GetStringCell(node.AddressTransferType));

			return row;
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

		#endregion SheetDaata
	}
}
