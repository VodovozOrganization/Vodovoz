using System;
using System.Collections.Generic;
using Core.Infrastructure;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Extensions;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;

namespace Vodovoz.ViewModels.ViewModels.Reports.Payments
{
	[Appellative(Nominative = "Выгрузка журнала движений средств по расчетным счетам")]
	public class BankAccountsMovementsJournalReport
	{
		private const double _defaultColumnWidth = 12;
		private uint _defaultCellFormatId;
		private uint _tableHeadersCellFormatId;
		private uint _tableTitleCellFormatId;

		private readonly IEnumerable<BankAccountsMovementsJournalNode> _accountsMovementsJournalNodes;

		public BankAccountsMovementsJournalReport(
			DateTime startDate,
			DateTime? endDate,
			IEnumerable<BankAccountsMovementsJournalNode> accountsMovementsJournalNodes)
		{
			_accountsMovementsJournalNodes = accountsMovementsJournalNodes ?? new List<BankAccountsMovementsJournalNode>();

			StartDate = startDate;
			EndDate = endDate;
			ReportCreatedAt = DateTime.Now;
		}

		public string Title =>
			string.Join(
				" ", 
				"Список движений по р/сч за период",
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

				foreach(var node in _accountsMovementsJournalNodes)
				{
					sheetData.Append(GetTableDataRow(node));
				}

				worksheetPart.Worksheet.Append(sheetData);

				worksheetPart.Worksheet.Save();

				var sheet = new Sheet
				{
					Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
					SheetId = 1,
					Name = "Движения по р/сч"
				};
				var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
				sheets.AppendChild(sheet);

				spreadsheet.WorkbookPart.Workbook.Save();
			}
		}

		private Columns CreateColumns(double defaultColumnWidth)
		{
			var columns = new Columns();

			var idColumn = CreateColumn(1, defaultColumnWidth);
			var startDateColumn = CreateColumn(2, defaultColumnWidth);
			var endDateColumn = CreateColumn(3, defaultColumnWidth);
			var accountColumn = CreateColumn(4, defaultColumnWidth * 2);
			var bankColumn = CreateColumn(5, defaultColumnWidth * 3);
			var dataTypeColumn = CreateColumn(6, defaultColumnWidth * 1.5);
			var amountFromDocumentColumn = CreateColumn(7, defaultColumnWidth * 1.5);
			var amountFromProgramColumn = CreateColumn(8, defaultColumnWidth * 1.5);
			var discrepancyColumn = CreateColumn(9, defaultColumnWidth * 1.5);
			var discrepancyDescriptionColumn = CreateColumn(10, defaultColumnWidth * 5);

			columns.Append(idColumn);
			columns.Append(startDateColumn);
			columns.Append(endDateColumn);
			columns.Append(accountColumn);
			columns.Append(bankColumn);
			columns.Append(dataTypeColumn);
			columns.Append(amountFromDocumentColumn);
			columns.Append(amountFromProgramColumn);
			columns.Append(discrepancyColumn);
			columns.Append(discrepancyDescriptionColumn);

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

			row.AppendChild(GetTableHeaderStringCell(BankAccountsMovementsJournalColumns.Id));
			row.AppendChild(GetTableHeaderStringCell(BankAccountsMovementsJournalColumns.StartDate));
			row.AppendChild(GetTableHeaderStringCell(BankAccountsMovementsJournalColumns.EndDate));
			row.AppendChild(GetTableHeaderStringCell(BankAccountsMovementsJournalColumns.Account));
			row.AppendChild(GetTableHeaderStringCell(BankAccountsMovementsJournalColumns.Bank));
			row.AppendChild(GetTableHeaderStringCell(BankAccountsMovementsJournalColumns.Empty));
			row.AppendChild(GetTableHeaderStringCell(BankAccountsMovementsJournalColumns.AmountFromDocument));
			row.AppendChild(GetTableHeaderStringCell(BankAccountsMovementsJournalColumns.AmountFromProgram));
			row.AppendChild(GetTableHeaderStringCell(BankAccountsMovementsJournalColumns.Discrepancy));
			row.AppendChild(GetTableHeaderStringCell(BankAccountsMovementsJournalColumns.DiscrepancyDescription));

			return row;
		}

		private Row GetTableDataRow(BankAccountsMovementsJournalNode node)
		{
			var row = new Row();

			row.AppendChild(node.Id.HasValue ? GetNumericCell(node.Id.Value) : GetStringCell(null));
			row.AppendChild(GetStringCell(node.StartDate.ToShortDateString()));
			row.AppendChild(GetStringCell(node.EndDate.ToShortDateString()));
			row.AppendChild(GetStringCell(node.Account));
			row.AppendChild(GetStringCell(node.Bank));
			row.AppendChild(GetStringCell(node.AccountMovementDataType.GetEnumDisplayName()));
			row.AppendChild(node.Amount.HasValue ? GetFloatingPointCell(node.Amount.Value) : GetStringCell(StringConstants.NotSet));
			row.AppendChild(node.AmountFromProgram.HasValue ? GetFloatingPointCell(node.AmountFromProgram.Value) : GetStringCell(null));
			row.AppendChild(node.HasDiscrepancy ? GetFloatingPointCell(node.Difference.Value) : GetStringCell(null));
			row.AppendChild(GetStringCell(node.GetDiscrepancyDescription()));

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
