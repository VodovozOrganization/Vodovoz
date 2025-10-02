using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using VodovozInfrastructure.Extensions;

namespace Vodovoz.ViewModels.ViewModels.Reports.Payments
{
	[Appellative(Nominative = "Выгрузка журнала платежей")]
	public class PaymentsFromBankClientReport
	{
		private const double _defaultColumnWidth = 12;
		private uint _defaultCellFormatId;
		private uint _tableHeadersCellFormatId;
		private uint _tableTitleCellFormatId;

		private readonly IEnumerable<PaymentJournalNode> _orderJournalNodes;

		public PaymentsFromBankClientReport(
			DateTime startDate,
			DateTime? endDate,
			IEnumerable<PaymentJournalNode> orderJournalNodes)
		{
			_orderJournalNodes = orderJournalNodes ?? new List<PaymentJournalNode>();

			StartDate = startDate;
			EndDate = endDate;
			ReportCreatedAt = DateTime.Now;
		}

		public string Title =>
			string.Join(
				" ", 
				"Список платежей/списаний за период",
				!EndDate.HasValue
					? $"с {StartDate:dd.MM.yyyy}"
					: $"с {StartDate:dd.MM.yyyy} по {EndDate:dd.MM.yyyy}");

		public DateTime StartDate { get; }

		public DateTime? EndDate { get; }

		public DateTime ReportCreatedAt { get; }

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

				worksheetPart.Worksheet.Append(CreateColumns(_defaultColumnWidth));

				var sheetData = new SheetData();
				sheetData.Append(GetTableTitleRow());
				sheetData.Append(GetBlankRow());
				sheetData.Append(GetTableHeadersRow());

				foreach(var node in _orderJournalNodes)
				{
					sheetData.Append(GetTableDataRow(node));
				}

				worksheetPart.Worksheet.Append(sheetData);

				worksheetPart.Worksheet.Save();

				var sheet = new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Платежи" };
				var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
				sheets.AppendChild(sheet);

				spreadsheet.WorkbookPart.Workbook.Save();
			}
		}

		private Columns CreateColumns(double defaultColumnWidth)
		{
			var columns = new Columns();

			var idColumn = CreateColumn(1, defaultColumnWidth);
			var numberColumn = CreateColumn(2, defaultColumnWidth);
			var dateColumn = CreateColumn(3, defaultColumnWidth);
			var totalColumn = CreateColumn(4, defaultColumnWidth);
			var ordersColumn = CreateColumn(5, defaultColumnWidth);
			var payerColumn = CreateColumn(6, defaultColumnWidth * 3);
			var counterpartyColumn = CreateColumn(7, defaultColumnWidth * 3);
			var organizationColumn = CreateColumn(8, defaultColumnWidth * 2);
			var organizationBankColumn = CreateColumn(9, defaultColumnWidth * 2);
			var organizationAccountColumn = CreateColumn(10, defaultColumnWidth * 2);
			var purposeColumn = CreateColumn(11, defaultColumnWidth * 5);
			var profitCategoryColumn = CreateColumn(12, defaultColumnWidth);
			var isManuallyCreatedColumn = CreateColumn(13, defaultColumnWidth);
			var unAllocatedSumColumn = CreateColumn(14, defaultColumnWidth);
			var documentTypeColumn = CreateColumn(15, defaultColumnWidth);

			columns.Append(idColumn);
			columns.Append(numberColumn);
			columns.Append(dateColumn);
			columns.Append(totalColumn);
			columns.Append(ordersColumn);
			columns.Append(payerColumn);
			columns.Append(counterpartyColumn);
			columns.Append(organizationColumn);
			columns.Append(organizationBankColumn);
			columns.Append(organizationAccountColumn);
			columns.Append(purposeColumn);
			columns.Append(profitCategoryColumn);
			columns.Append(isManuallyCreatedColumn);
			columns.Append(unAllocatedSumColumn);
			columns.Append(documentTypeColumn);

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

		private Row GetTableTitleRow()
		{
			var row = new Row();

			row.AppendChild(GetTableTitleStringCell(Title));

			return row;
		}

		private Row GetTableHeadersRow()
		{
			var row = new Row();

			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.IdColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.NumberColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.DateColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.TotalColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.OrdersColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.PayerColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.CounterpartyColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.OrganizationColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.OrganizationBankColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.OrganizationAccountColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.PurposeColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.ProfitCategoryColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.IsManuallyCreatedColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.UnAllocatedSumColumn));
			row.AppendChild(GetTableHeaderStringCell(PaymentsJournalColumns.DocumentTypeColumn));

			return row;
		}

		private Row GetTableDataRow(PaymentJournalNode node)
		{
			var row = new Row();

			row.AppendChild(GetNumericCell(node.Id));
			row.AppendChild(GetNumericCell(node.PaymentNum));
			row.AppendChild(GetStringCell(node.Date.ToShortDateString()));
			row.AppendChild(GetStringCurrencyFormatCell(node.Total));
			row.AppendChild(GetStringCell(node.Orders));
			row.AppendChild(GetStringCell(node.PayerName));
			row.AppendChild(GetStringCell(node.CounterpartyName));
			row.AppendChild(GetStringCell(node.Organization));
			row.AppendChild(GetStringCell(node.OrganizationBank));
			row.AppendChild(GetStringCell(node.OrganizationAccountNumber));
			row.AppendChild(GetStringCell(node.PaymentPurpose));
			row.AppendChild(GetStringCell(node.ProfitCategory));
			row.AppendChild(GetStringCell(node.IsManualCreated.ConvertToYesOrNo()));
			row.AppendChild(GetStringCurrencyFormatCell(node.UnAllocatedSum));
			row.AppendChild(GetStringCell(node.EntityType.GetClassUserFriendlyName().Nominative.CapitalizeSentence()));

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

			stylesheet.Fonts = new Fonts();
			stylesheet.Fonts.AppendChild(GetDefaultFont());
			stylesheet.Fonts.AppendChild(GetTableHeadersFont());
			stylesheet.Fonts.AppendChild(GetWorksheetTitleFont());
			stylesheet.Fonts.Count = 3;

			stylesheet.Fills = new Fills();
			stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } });
			stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } });

			stylesheet.Borders = new Borders();
			stylesheet.Borders.AppendChild(new Border());
			stylesheet.Borders.AppendChild(GetCellBorder());
			stylesheet.Borders.Count = 2;

			var defaultCellFormat = new CellFormat { FormatId = 0, FontId = 0, BorderId = 1 };
			defaultCellFormat.Alignment = new Alignment { WrapText = true };

			var tableHeadersCellFormat = new CellFormat { FormatId = 0, FontId = 1, BorderId = 1 };
			tableHeadersCellFormat.Alignment = new Alignment { WrapText = true };

			var tableTitleCellFormat = new CellFormat { FormatId = 0, FontId = 2 };

			stylesheet.CellStyleFormats = new CellStyleFormats();
			stylesheet.CellStyleFormats.AppendChild(new CellFormat());
			stylesheet.CellFormats = new CellFormats();
			stylesheet.CellFormats.AppendChild(new CellFormat());

			stylesheet.CellFormats.AppendChild(defaultCellFormat);
			_defaultCellFormatId = 1;

			stylesheet.CellFormats.AppendChild(tableHeadersCellFormat);
			_tableHeadersCellFormatId = 2;

			stylesheet.CellFormats.AppendChild(tableTitleCellFormat);
			_tableTitleCellFormatId = 3;

			stylesheet.CellFormats.Count = 4;

			return stylesheet;
		}

		private Border GetCellBorder()
		{
			var border = new Border();

			var leftBorder = new LeftBorder() { Style = BorderStyleValues.Thin };
			var leftBorderColor = new Color() { Indexed = (UInt32Value)64U };
			leftBorder.Append(leftBorderColor);

			var rightBorder = new RightBorder() { Style = BorderStyleValues.Thin };
			var rightBorderColor = new Color() { Indexed = (UInt32Value)64U };
			rightBorder.Append(rightBorderColor);

			var topBorder = new TopBorder() { Style = BorderStyleValues.Thin };
			var topBorderColor = new Color() { Indexed = (UInt32Value)64U };
			topBorder.Append(topBorderColor);

			var bottomBorder = new BottomBorder() { Style = BorderStyleValues.Thin };
			var bottomBorderColor = new Color() { Indexed = (UInt32Value)64U };
			bottomBorder.Append(bottomBorderColor);

			var diagonalBorder = new DiagonalBorder();

			border.Append(leftBorder);
			border.Append(rightBorder);
			border.Append(topBorder);
			border.Append(bottomBorder);
			border.Append(diagonalBorder);

			return border;
		}

		private Cell GetTableTitleStringCell(string value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.String,
				StyleIndex = _tableTitleCellFormatId
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

		private Cell GetStringCell(string value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.String,
				StyleIndex = _defaultCellFormatId
			};

			return cell;
		}

		private Cell GetNumericCell(int value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.Number,
				StyleIndex = _defaultCellFormatId
			};

			return cell;
		}

		private Cell GetFloatingPointCell(decimal value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.Number,
				StyleIndex = _defaultCellFormatId
			};

			return cell;
		}

		private Cell GetStringCurrencyFormatCell(decimal value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value.ToString("### ### ##0.00 ₽")),
				DataType = CellValues.String,
				StyleIndex = _defaultCellFormatId
			};

			return cell;
		}

		private Font GetDefaultFont()
		{
			var fontSize = new FontSize
			{
				Val = 10
			};

			var font = new Font
			{
				FontSize = fontSize
			};

			return font;
		}

		private Font GetTableHeadersFont()
		{
			var bold = new Bold();

			var fontSize = new FontSize
			{
				Val = 10
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
