using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using Vodovoz.JournalNodes;

namespace Vodovoz.ViewModels.ViewModels.Reports.Orders
{
	[Appellative(Nominative = "Отчет по заказам")]
	public class OrdersReport
	{
		private const double _defaultColumnWidth = 12;
		private uint _defaultCellFormatId;
		private uint _tableHeadersCellFormatId;
		private uint _tableTitleCellFormatId;
		private uint _currencyCellFormatId;

		private readonly IEnumerable<OrderJournalNode> _orderJournalNodes;

		public OrdersReport(
			DateTime createDateFrom,
			DateTime createDateTo,
			IEnumerable<OrderJournalNode> orderJournalNodes)
		{
			_orderJournalNodes = orderJournalNodes ?? new List<OrderJournalNode>();

			CreateDateFrom = createDateFrom;
			CreateDateTo = createDateTo;
			ReportCreatedAt = DateTime.Now;
		}

		public string Title => 
			$"Список заказов " +
			$"за период с {CreateDateFrom:dd.MM.yyyy} по {CreateDateTo:dd.MM.yyyy}";

		public DateTime CreateDateFrom { get; }

		public DateTime CreateDateTo { get; }

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

				var sheet = new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Заказы" };
				var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
				sheets.AppendChild(sheet);

				spreadsheet.WorkbookPart.Workbook.Save();
			}
		}

		private Columns CreateColumns(double defaultColumnWidth)
		{
			var columns = new Columns();

			var orderIdColumn = CreateColumn(1, defaultColumnWidth);
			var dateColumn = CreateColumn(2, defaultColumnWidth);
			var authorColumn = CreateColumn(3, defaultColumnWidth * 2);
			var deliveryTimeColumn = CreateColumn(4, defaultColumnWidth);
			var orderStatusColumn = CreateColumn(5, defaultColumnWidth);
			var typeColumn = CreateColumn(6, defaultColumnWidth * 1.5);
			var bottleAmountCount = CreateColumn(7, defaultColumnWidth);
			var sanitisationAmount = CreateColumn(8, defaultColumnWidth);
			var counterpartyColumn = CreateColumn(9, defaultColumnWidth * 3);
			var innColumn = CreateColumn(10, defaultColumnWidth);
			var sumColumn = CreateColumn(11, defaultColumnWidth);
			var paymentStatusColumn = CreateColumn(12, defaultColumnWidth);
			var edoDocFlowStatusColumn = CreateColumn(13, defaultColumnWidth);
			var districtNameColumn = CreateColumn(14, defaultColumnWidth * 2);
			var addressColumn = CreateColumn(15, defaultColumnWidth * 5);
			var lastEditorColumn = CreateColumn(16, defaultColumnWidth * 2);
			var lastEditedTimeColumn = CreateColumn(17, defaultColumnWidth * 2);
			var driverCallIdColumn = CreateColumn(18, defaultColumnWidth);
			var onLineNumberColumn = CreateColumn(19, defaultColumnWidth);
			var eShopNumberColumn = CreateColumn(20, defaultColumnWidth);

			columns.Append(orderIdColumn);
			columns.Append(dateColumn);
			columns.Append(authorColumn);
			columns.Append(deliveryTimeColumn);
			columns.Append(orderStatusColumn);
			columns.Append(typeColumn);
			columns.Append(bottleAmountCount);
			columns.Append(sanitisationAmount);
			columns.Append(counterpartyColumn);
			columns.Append(innColumn);
			columns.Append(sumColumn);
			columns.Append(paymentStatusColumn);
			columns.Append(edoDocFlowStatusColumn);
			columns.Append(districtNameColumn);
			columns.Append(addressColumn);
			columns.Append(lastEditorColumn);
			columns.Append(lastEditedTimeColumn);
			columns.Append(driverCallIdColumn);
			columns.Append(onLineNumberColumn);
			columns.Append(eShopNumberColumn);

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

			row.AppendChild(GetTableHeaderStringCell("Номер"));
			row.AppendChild(GetTableHeaderStringCell("Дата"));
			row.AppendChild(GetTableHeaderStringCell("Автор"));
			row.AppendChild(GetTableHeaderStringCell("Время"));
			row.AppendChild(GetTableHeaderStringCell("Статус"));
			row.AppendChild(GetTableHeaderStringCell("Тип"));
			row.AppendChild(GetTableHeaderStringCell("Бутыли"));
			row.AppendChild(GetTableHeaderStringCell("Кол-во с/о"));
			row.AppendChild(GetTableHeaderStringCell("Клиент"));
			row.AppendChild(GetTableHeaderStringCell("ИНН"));
			row.AppendChild(GetTableHeaderStringCell("Сумма"));
			row.AppendChild(GetTableHeaderStringCell("Статус оплаты"));
			row.AppendChild(GetTableHeaderStringCell("Статус документооборота"));
			row.AppendChild(GetTableHeaderStringCell("Район доставки"));
			row.AppendChild(GetTableHeaderStringCell("Адрес"));
			row.AppendChild(GetTableHeaderStringCell("Изменил"));
			row.AppendChild(GetTableHeaderStringCell("Послед. изменения"));
			row.AppendChild(GetTableHeaderStringCell("Номер звонка"));
			row.AppendChild(GetTableHeaderStringCell("Online заказ №"));
			row.AppendChild(GetTableHeaderStringCell("Номер заказа интернет-магазина"));

			return row;
		}

		private Row GetTableDataRow(OrderJournalNode node)
		{
			var row = new Row();

			row.AppendChild(GetNumericCell(node.Id));
			row.AppendChild(GetStringCell(node.Date != null ? ((DateTime)node.Date).ToString("d") : string.Empty));
			row.AppendChild(GetStringCell(node.Author));
			row.AppendChild(GetStringCell(node.IsSelfDelivery ? "-" : node.DeliveryTime));
			row.AppendChild(GetStringCell(node.StatusEnum.GetEnumDisplayName()));
			row.AppendChild(GetStringCell(node.ViewType));
			row.AppendChild(GetNumericCell((int)node.BottleAmount));
			row.AppendChild(GetNumericCell((int)node.SanitisationAmount));
			row.AppendChild(GetStringCell(node.Counterparty));
			row.AppendChild(GetStringCell(node.Inn));
			row.AppendChild(GetCurrencyFormatCell(node.Sum));
			row.AppendChild(GetStringCell(((node.OrderPaymentStatus != OrderPaymentStatus.None) ? node.OrderPaymentStatus.GetEnumDisplayName() : "")));
			row.AppendChild(GetStringCell(node.EdoDocFlowStatus?.GetEnumDisplayName()));
			row.AppendChild(GetStringCell(node.IsSelfDelivery ? "-" : node.DistrictName));
			row.AppendChild(GetStringCell(node.Address));
			row.AppendChild(GetStringCell(node.LastEditor));
			row.AppendChild(GetStringCell(node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString(CultureInfo.CurrentCulture) : string.Empty));
			row.AppendChild(GetStringCell(node.DriverCallId.ToString()));
			row.AppendChild(GetStringCell(node.OnLineNumber));
			row.AppendChild(GetStringCell(node.EShopNumber));

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

			var currencyCellFormat = new CellFormat
			{
				FormatId = 0,
				FontId = 0,
				BorderId = 1,
				NumberFormatId = 44
			};
			currencyCellFormat.Alignment = new Alignment { WrapText = true };

			var tableHeadersCellFormat = new CellFormat { FormatId = 0, FontId = 1, BorderId = 1 };
			tableHeadersCellFormat.Alignment = new Alignment { WrapText = true };

			var tableTitleCellFormat = new CellFormat { FormatId = 0, FontId = 2 };

			stylesheet.CellStyleFormats = new CellStyleFormats();
			stylesheet.CellStyleFormats.AppendChild(new CellFormat());
			stylesheet.CellFormats = new CellFormats();
			stylesheet.CellFormats.AppendChild(new CellFormat());

			stylesheet.CellFormats.AppendChild(defaultCellFormat);
			_defaultCellFormatId = 1;
			
			stylesheet.CellFormats.AppendChild(currencyCellFormat);
			_currencyCellFormatId = 2;
			
			stylesheet.CellFormats.AppendChild(tableHeadersCellFormat);
			_tableHeadersCellFormatId = 3;

			stylesheet.CellFormats.AppendChild(tableTitleCellFormat);
			_tableTitleCellFormatId = 4;

			stylesheet.CellFormats.Count = 5;

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

		private Cell GetCurrencyFormatCell(decimal value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				StyleIndex = _currencyCellFormatId
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
