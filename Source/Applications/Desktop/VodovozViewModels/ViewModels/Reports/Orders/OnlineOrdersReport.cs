using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Data.Bindings;
using Vodovoz.Extensions;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;

namespace Vodovoz.ViewModels.ViewModels.Reports.Orders
{
	[Appellative(Nominative = "Отчет по онлайн заказам")]
	public class OnlineOrdersReport
	{
		private const double _defaultColumnWidth = 12;
		private uint _defaultCellFormatId;
		private uint _tableHeadersCellFormatId;
		private uint _tableTitleCellFormatId;
		private uint _currencyCellFormatId;

		private readonly IEnumerable<OnlineOrdersJournalNode> _onlineOrdersJournalNodes;

		public OnlineOrdersReport(
			DateTime createDateFrom,
			DateTime createDateTo,
			IEnumerable<OnlineOrdersJournalNode> onlineOrdersJournalNodes)
		{
			_onlineOrdersJournalNodes = onlineOrdersJournalNodes ?? new List<OnlineOrdersJournalNode>();

			CreateDateFrom = createDateFrom;
			CreateDateTo = createDateTo;
			ReportCreatedAt = DateTime.Now;
		}

		public string Title =>
			$"Список онлайн заказов за период с {CreateDateFrom:dd.MM.yyyy} по {CreateDateTo:dd.MM.yyyy}";

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

				foreach(var node in _onlineOrdersJournalNodes)
				{
					sheetData.Append(GetTableDataRow(node));
				}

				worksheetPart.Worksheet.Append(sheetData);
				worksheetPart.Worksheet.Save();

				var sheet = new Sheet
				{
					Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
					SheetId = 1,
					Name = "Онлайн заказы"
				};

				var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
				sheets.AppendChild(sheet);

				spreadsheet.WorkbookPart.Workbook.Save();
			}
		}

		private Columns CreateColumns(double defaultColumnWidth)
		{
			var columns = new Columns();

			columns.Append(CreateColumn(1, defaultColumnWidth));        // Id
			columns.Append(CreateColumn(2, defaultColumnWidth * 1.5));  // Тип сущности
			columns.Append(CreateColumn(3, defaultColumnWidth * 3));    // Клиент
			columns.Append(CreateColumn(4, defaultColumnWidth * 5));    // Адрес
			columns.Append(CreateColumn(5, defaultColumnWidth * 1.5));  // Дата доставки
			columns.Append(CreateColumn(6, defaultColumnWidth * 1.5));  // Дата создания
			columns.Append(CreateColumn(7, defaultColumnWidth));        // Самовывоз
			columns.Append(CreateColumn(8, defaultColumnWidth));        // Быстрая доставка
			columns.Append(CreateColumn(9, defaultColumnWidth * 1.5));  // Время доставки
			columns.Append(CreateColumn(10, defaultColumnWidth * 1.5)); // Статус
			columns.Append(CreateColumn(11, defaultColumnWidth * 2));   // Менеджер
			columns.Append(CreateColumn(12, defaultColumnWidth * 1.5)); // Источник
			columns.Append(CreateColumn(13, defaultColumnWidth * 1.5)); // Сумма
			columns.Append(CreateColumn(14, defaultColumnWidth * 1.5)); // Статус оплаты
			columns.Append(CreateColumn(15, defaultColumnWidth));       // Онлайн оплата
			columns.Append(CreateColumn(16, defaultColumnWidth * 1.5)); // Тип оплаты
			columns.Append(CreateColumn(17, defaultColumnWidth));       // Нужен звонок
			columns.Append(CreateColumn(18, defaultColumnWidth * 2));   // Причина отмены
			columns.Append(CreateColumn(19, defaultColumnWidth * 2));   // Id заказов

			return columns;
		}

		private Column CreateColumn(int columnId, double columnWidth)
		{
			return new Column
			{
				Min = (uint)columnId,
				Max = (uint)columnId,
				CustomWidth = true,
				Width = columnWidth
			};
		}

		private Row GetTableTitleRow()
		{
			var row = new Row();
			row.AppendChild(GetTableTitleStringCell(Title));
			return row;
		}

		private Row GetBlankRow()
		{
			return new Row();
		}

		private Row GetTableHeadersRow()
		{
			var row = new Row();

			row.AppendChild(GetTableHeaderStringCell("Номер"));
			row.AppendChild(GetTableHeaderStringCell("Тип сущности"));
			row.AppendChild(GetTableHeaderStringCell("Клиент"));
			row.AppendChild(GetTableHeaderStringCell("Адрес"));
			row.AppendChild(GetTableHeaderStringCell("Дата доставки"));
			row.AppendChild(GetTableHeaderStringCell("Дата создания"));
			row.AppendChild(GetTableHeaderStringCell("Самовывоз"));
			row.AppendChild(GetTableHeaderStringCell("Быстрая доставка"));
			row.AppendChild(GetTableHeaderStringCell("Время доставки"));
			row.AppendChild(GetTableHeaderStringCell("Статус"));
			row.AppendChild(GetTableHeaderStringCell("Менеджер"));
			row.AppendChild(GetTableHeaderStringCell("Источник"));
			row.AppendChild(GetTableHeaderStringCell("Сумма"));
			row.AppendChild(GetTableHeaderStringCell("Статус оплаты"));
			row.AppendChild(GetTableHeaderStringCell("Онлайн оплата"));
			row.AppendChild(GetTableHeaderStringCell("Тип оплаты"));
			row.AppendChild(GetTableHeaderStringCell("Нужен звонок"));
			row.AppendChild(GetTableHeaderStringCell("Причина отмены"));
			row.AppendChild(GetTableHeaderStringCell("Номера заказов"));

			return row;
		}

		private Row GetTableDataRow(OnlineOrdersJournalNode node)
		{
			var row = new Row();

			row.AppendChild(GetNumericCell(node.Id));
			row.AppendChild(GetStringCell(node.EntityTypeString));
			row.AppendChild(GetStringCell(node.CounterpartyName));
			row.AppendChild(GetStringCell(node.CompiledAddress));
			row.AppendChild(GetStringCell(node.DeliveryDate?.ToString("d") ?? string.Empty));
			row.AppendChild(GetStringCell(node.CreationDate.ToString("g")));
			row.AppendChild(GetStringCell(node.IsSelfDelivery ? "Да" : "Нет"));
			row.AppendChild(GetStringCell(node.IsFastDelivery ? "Да" : "Нет"));
			row.AppendChild(GetStringCell(node.DeliveryTime));
			row.AppendChild(GetStringCell(node.Status));
			row.AppendChild(GetStringCell(node.ManagerWorkWith));
			row.AppendChild(GetStringCell(node.Source.GetEnumTitle()));
			row.AppendChild(GetCurrencyFormatCell(node.OnlineOrderSum ?? 0m));
			row.AppendChild(GetStringCell(node.OnlineOrderPaymentStatus?.GetEnumDisplayName() ?? string.Empty));
			row.AppendChild(GetStringCell(node.OnlinePayment.HasValue ? node.OnlinePayment.Value.ToString() : string.Empty));
			row.AppendChild(GetStringCell(node.OnlineOrderPaymentType.GetEnumDisplayName()));
			row.AppendChild(GetStringCell(node.IsNeedConfirmationByCall ? "Да" : "Нет"));
			row.AppendChild(GetStringCell(node.CancelReason));
			row.AppendChild(GetStringCell(node.OrdersIds));

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

			var leftBorder = new LeftBorder { Style = BorderStyleValues.Thin };
			leftBorder.Append(new Color { Indexed = (UInt32Value)64U });

			var rightBorder = new RightBorder { Style = BorderStyleValues.Thin };
			rightBorder.Append(new Color { Indexed = (UInt32Value)64U });

			var topBorder = new TopBorder { Style = BorderStyleValues.Thin };
			topBorder.Append(new Color { Indexed = (UInt32Value)64U });

			var bottomBorder = new BottomBorder { Style = BorderStyleValues.Thin };
			bottomBorder.Append(new Color { Indexed = (UInt32Value)64U });

			border.Append(leftBorder);
			border.Append(rightBorder);
			border.Append(topBorder);
			border.Append(bottomBorder);
			border.Append(new DiagonalBorder());

			return border;
		}

		private Cell GetTableTitleStringCell(string value)
		{
			return new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.String,
				StyleIndex = _tableTitleCellFormatId
			};
		}

		private Cell GetTableHeaderStringCell(string value)
		{
			return new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.String,
				StyleIndex = _tableHeadersCellFormatId
			};
		}

		private Cell GetStringCell(string value)
		{
			return new Cell
			{
				CellValue = new CellValue(value ?? string.Empty),
				DataType = CellValues.String,
				StyleIndex = _defaultCellFormatId
			};
		}

		private Cell GetNumericCell(int value)
		{
			return new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.Number,
				StyleIndex = _defaultCellFormatId
			};
		}

		private Cell GetCurrencyFormatCell(decimal value)
		{
			return new Cell
			{
				CellValue = new CellValue(value),
				StyleIndex = _currencyCellFormatId
			};
		}

		private Font GetDefaultFont()
		{
			return new Font
			{
				FontSize = new FontSize { Val = 10 }
			};
		}

		private Font GetTableHeadersFont()
		{
			return new Font
			{
				Bold = new Bold(),
				FontSize = new FontSize { Val = 10 }
			};
		}

		private Font GetWorksheetTitleFont()
		{
			return new Font
			{
				Bold = new Bold(),
				FontSize = new FontSize { Val = 14 }
			};
		}
	}
}
