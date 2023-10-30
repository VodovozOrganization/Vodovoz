using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using MoreLinq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Vodovoz.Domain.Client.ClientClassification;
using System;

namespace Vodovoz.ViewModels.Counterparties.ClientClassification
{
	public partial class CounterpartyClassificationCalculationViewModel
	{
		public class ClassificationCalculationReport
		{
			private int _period;
			private string _lastReportDate;

			private const double _defaultColumnWidth = 12;
			private uint _defaultCellFormatId;
			private uint _tableHeadersCellFormatId;
			private uint _tableTitleCellFormatId;

			public ClassificationCalculationReport(
				IDictionary<int, CounterpartyClassification> newClassifications,
				IDictionary<int, CounterpartyClassification> oldClassifications,
				IDictionary<int, string> counterpartyNames,
				int periodInMonth)
			{
				_period = periodInMonth;

				var lastReportDate = oldClassifications.Select(c => c.Value.ClassificationCalculationDate).Max();
				_lastReportDate = 
					lastReportDate ==  DateTime.MinValue 
					? "не выполнялось" 
					: lastReportDate.ToString("dd.MM.yyyy");

				var rows = CreateRows(newClassifications, oldClassifications, counterpartyNames);

				Export(rows);
			}

			public string Title => 
				$"ОТЧЁТ ОБ ИЗМЕНЕНИИ КАТЕГОРИИ КЛИЕНТОВ ОТ {DateTime.Now.ToString("dd.MM.yyyy")} за Период в {_period} месяца";

			public static ClassificationCalculationReport GenerateReport(
				IDictionary<int, CounterpartyClassification> newClassifications,
				IDictionary<int, CounterpartyClassification> oldClassifications,
				IDictionary<int, string> counterpartyNames,
				int periodInMonth)
			{
				var report = new ClassificationCalculationReport(
					newClassifications,
					oldClassifications,
					counterpartyNames,
					periodInMonth);

				return report;
			}

			private IEnumerable<ClassificationCalculationReportRow> CreateRows(
				IDictionary<int, CounterpartyClassification> newClassifications,
				IDictionary<int, CounterpartyClassification> oldClassifications,
				IDictionary<int, string> counterpartyNames)
			{
				var rows = new List<ClassificationCalculationReportRow>();

				foreach(var classification in newClassifications.Values)
				{
					var hasOldClassification =
						oldClassifications.TryGetValue(classification.CounterpartyId, out CounterpartyClassification oldClassification);

					if(hasOldClassification
						&& classification.ClassificationByBottlesCount == oldClassification.ClassificationByBottlesCount
						&& classification.ClassificationByOrdersCount == oldClassification.ClassificationByOrdersCount)
					{
						continue;
					}

					var row = new ClassificationCalculationReportRow();

					row.CounterpartyId = classification.CounterpartyId;

					row.CounterpartyName =
						counterpartyNames.TryGetValue(classification.CounterpartyId, out string name)
						? name
						: $"Имя не указано. Id = {classification.CounterpartyId}";

					row.NewAverageBottlesCount = classification.BottlesPerMonthAverageCount;
					row.NewAverageOrdersCount = classification.OrdersPerMonthAverageCount;
					row.NewAverageMoneyTurnoverSum = classification.MoneyTurnoverPerMonthAverageSum;
					row.NewClassificationByBottles = classification.ClassificationByBottlesCount;
					row.NewClassificationByOrders = classification.ClassificationByOrdersCount;

					if(hasOldClassification)
					{
						row.OldAverageBottlesCount = oldClassification.BottlesPerMonthAverageCount;
						row.OldAverageOrdersCount = oldClassification.OrdersPerMonthAverageCount;
						row.OldAverageMoneyTurnoverSum = oldClassification.MoneyTurnoverPerMonthAverageSum;
						row.OldClassificationByBottles = oldClassification.ClassificationByBottlesCount;
						row.OldClassificationByOrders = oldClassification.ClassificationByOrdersCount;
					}

					rows.Add(row);
				}

				return rows;
			}

			private void Export(IEnumerable<ClassificationCalculationReportRow> rows)
			{

				var groupedByBottlesClassification = (from r in rows
													  group r by new { r.NewClassificationByBottles, r.OldClassificationByBottles })
													 .ToDictionary(g => g.Key, g => g.ToList());

				var groupedByOrdersClassification = (from r in rows
													 group r by new { r.NewClassificationByOrders, r.OldClassificationByOrders })
													 .ToDictionary(g => g.Key, g => g.ToList());


				using(var spreadsheet = SpreadsheetDocument.Create("D:\\new.xmls", SpreadsheetDocumentType.Workbook))
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
					sheetData.Append(GetLastReportInfoRow(_lastReportDate));
					sheetData.Append(GetBlankRow());
					sheetData.Append(GetTableHeadersRow());

					//foreach(var node in _orderJournalNodes)
					//{
					//	sheetData.Append(GetTableDataRow(node));
					//}

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

				var emptyColumn = CreateColumn(1, defaultColumnWidth);
				var categoryFromToColumn = CreateColumn(2, defaultColumnWidth * 0.5);
				var counterpartyColumn = CreateColumn(3, defaultColumnWidth * 1.5);
				var bottlesCountOldColumn = CreateColumn(4, defaultColumnWidth);
				var bottlesCountNewColumn = CreateColumn(5, defaultColumnWidth);
				var turnowerSumOldColumn = CreateColumn(6, defaultColumnWidth);
				var turnowerSumNewColumn = CreateColumn(7, defaultColumnWidth);
				var orderPerMonthOldColumn = CreateColumn(8, defaultColumnWidth);
				var orderPerMonthNewColumn = CreateColumn(9, defaultColumnWidth);
				var categoryOldColumn = CreateColumn(10, defaultColumnWidth);
				var categoryNewColumn = CreateColumn(11, defaultColumnWidth);

				columns.Append(emptyColumn);
				columns.Append(categoryFromToColumn);
				columns.Append(counterpartyColumn);
				columns.Append(bottlesCountOldColumn);
				columns.Append(bottlesCountNewColumn);
				columns.Append(turnowerSumOldColumn);
				columns.Append(turnowerSumNewColumn);
				columns.Append(orderPerMonthOldColumn);
				columns.Append(orderPerMonthNewColumn);
				columns.Append(categoryOldColumn);
				columns.Append(categoryNewColumn);

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

			private Row GetLastReportInfoRow(string lastReportDate)
			{
				var row = new Row();

				var value = $"Дата последнего пересчета {lastReportDate}";
				row.AppendChild(GetTableTitleStringCell(value));

				return row;
			}

			private Row GetTableHeadersRow()
			{
				var row = new Row();

				row.AppendChild(GetTableHeaderStringCell(""));
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

			//private Row GetTableDataRow(OrderJournalNode node)
			//{
			//	var row = new Row();

			//	row.AppendChild(GetNumericCell(node.Id));
			//	row.AppendChild(GetStringCell(node.Date != null ? ((DateTime)node.Date).ToString("d") : string.Empty));
			//	row.AppendChild(GetStringCell(node.Author));
			//	row.AppendChild(GetStringCell(node.IsSelfDelivery ? "-" : node.DeliveryTime));
			//	row.AppendChild(GetStringCell(node.StatusEnum.GetEnumDisplayName()));
			//	row.AppendChild(GetStringCell(node.ViewType));
			//	row.AppendChild(GetNumericCell((int)node.BottleAmount));
			//	row.AppendChild(GetNumericCell((int)node.SanitisationAmount));
			//	row.AppendChild(GetStringCell(node.Counterparty));
			//	row.AppendChild(GetStringCell(node.Inn));
			//	row.AppendChild(GetStringCurrencyFormatCell(node.Sum));
			//	row.AppendChild(GetStringCell(((node.OrderPaymentStatus != OrderPaymentStatus.None) ? node.OrderPaymentStatus.GetEnumDisplayName() : "")));
			//	row.AppendChild(GetStringCell(node.EdoDocFlowStatus.GetEnumDisplayName()));
			//	row.AppendChild(GetStringCell(node.IsSelfDelivery ? "-" : node.DistrictName));
			//	row.AppendChild(GetStringCell(node.Address));
			//	row.AppendChild(GetStringCell(node.LastEditor));
			//	row.AppendChild(GetStringCell(node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString(CultureInfo.CurrentCulture) : string.Empty));
			//	row.AppendChild(GetStringCell(node.DriverCallId.ToString()));
			//	row.AppendChild(GetStringCell(node.OnLineNumber));
			//	row.AppendChild(GetStringCell(node.EShopNumber));

			//	return row;
			//}

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
}
