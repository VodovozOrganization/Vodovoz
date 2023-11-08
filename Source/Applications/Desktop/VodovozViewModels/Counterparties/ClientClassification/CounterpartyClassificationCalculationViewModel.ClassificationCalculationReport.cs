using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MoreLinq;
using MoreLinq.Extensions;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.ViewModels.Counterparties.ClientClassification
{
	public partial class CounterpartyClassificationCalculationViewModel
	{
		public class ClassificationCalculationReport
		{
			private readonly IEnumerable<ClassificationCalculationReportRow> _reportRows;
			private readonly int _periodInMonth;
			private readonly DateTime _lastCalculationDate;

			private const double _defaultColumnWidth = 16;
			private uint _defaultCellFormatId;
			private uint _tableHeadersCellFormatId;
			private uint _tableTitleCellFormatId;
			private uint _integerFormatId;
			private uint _doublePrecisionDecimalFormatId;

			public ClassificationCalculationReport(
				IEnumerable<ClassificationCalculationReportRow> reportRows,
				int periodInMonth,
				DateTime lastCalculationDate)
			{
				_reportRows = reportRows ?? new List<ClassificationCalculationReportRow>();
				_periodInMonth = periodInMonth;
				_lastCalculationDate = lastCalculationDate;
			}

			public string Title =>
				$"ОТЧЁТ ОБ ИЗМЕНЕНИИ КАТЕГОРИИ КЛИЕНТОВ ОТ {DateTime.Now.ToString("dd.MM.yyyy")} за Период в {_periodInMonth} месяца";

			public byte[] Export()
			{
				var data = Export(_reportRows);

				return data;
			}

			public static async Task<ClassificationCalculationReport> CreateReport(
				IUnitOfWork uow,
				ICounterpartyRepository counterpartyRepository,
				IEnumerable<CounterpartyClassification> newClassifications,
				int periodInMonth,
				CancellationToken cancellationToken)
			{
				var lastCalculationSettingsId = (await counterpartyRepository
					.GetLastClassificationCalculationSettingsId(uow)
					.ToListAsync(cancellationToken))
					.FirstOrDefault();

				var lastCalculationDate = GetCalculationSettingsDateById(
					uow, 
					lastCalculationSettingsId);

				var oldClassifications = GetLastExistingClassifications(
					uow,
					counterpartyRepository,
					lastCalculationSettingsId);

				var rows = await CreateRows(
					uow,
					newClassifications,
					oldClassifications,
					cancellationToken);

				var report = new ClassificationCalculationReport(
					rows,
					periodInMonth,
					lastCalculationDate);

				return report;
			}

			private static async Task<IEnumerable<ClassificationCalculationReportRow>> CreateRows(
				IUnitOfWork uow,
				IEnumerable<CounterpartyClassification> newClassifications,
				IEnumerable<CounterpartyClassification> oldClassifications,
				CancellationToken cancellationToken)
			{
				var counterpartiesNames = await uow.GetAll<Counterparty>()
					.Select(c => new { Id = c.Id, Name = c.Name })
					.ToListAsync(cancellationToken);

				var rows = from newClassification in newClassifications
						   join counterpartyName in counterpartiesNames on newClassification.CounterpartyId equals counterpartyName.Id
						   join oc in oldClassifications on newClassification.CounterpartyId equals oc.CounterpartyId into merged
						   from oldClassification in merged.DefaultIfEmpty()
						   where newClassification.ClassificationByBottlesCount != oldClassification?.ClassificationByBottlesCount
							  || newClassification.ClassificationByOrdersCount != oldClassification?.ClassificationByOrdersCount
						   select new ClassificationCalculationReportRow
						   {
							   CounterpartyId = newClassification.CounterpartyId,
							   CounterpartyName = counterpartyName.Name,
							   NewAverageBottlesCount = newClassification.BottlesPerMonthAverageCount,
							   NewAverageOrdersCount = newClassification.OrdersPerMonthAverageCount,
							   NewAverageMoneyTurnoverSum = newClassification.MoneyTurnoverPerMonthAverageSum,
							   NewClassificationByBottles = newClassification.ClassificationByBottlesCount,
							   NewClassificationByOrders = newClassification.ClassificationByOrdersCount,
							   OldAverageBottlesCount = oldClassification?.BottlesPerMonthAverageCount,
							   OldAverageOrdersCount = oldClassification?.OrdersPerMonthAverageCount,
							   OldAverageMoneyTurnoverSum = oldClassification?.MoneyTurnoverPerMonthAverageSum,
							   OldClassificationByBottles = oldClassification?.ClassificationByBottlesCount,
							   OldClassificationByOrders = oldClassification?.ClassificationByOrdersCount
						   };

				return rows;
			}

			private static IEnumerable<CounterpartyClassification> GetLastExistingClassifications(
				IUnitOfWork uow,
				ICounterpartyRepository counterpartyRepository,
				int lastCalculationSettingsId)
			{
				if(lastCalculationSettingsId == default)
				{
					return new List<CounterpartyClassification>();
				}

				var lastExistingClassifications = counterpartyRepository
					.GetLastExistingClassificationsForCounterparties(uow, lastCalculationSettingsId)
					.ToList();

				return lastExistingClassifications;
			}

			private static DateTime GetCalculationSettingsDateById(IUnitOfWork uow, int settingsId)
			{
				var settings = uow.GetById<CounterpartyClassificationCalculationSettings>(settingsId);

				return settings?.SettingsCreationDate ?? default;
			}

			private byte[] Export(IEnumerable<ClassificationCalculationReportRow> rows)
			{
				Byte[] reportData = null;

				using(var stream = new MemoryStream())
				{
					using(var spreadsheet = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
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
						sheetData.Append(GetLastReportInfoRow(_lastCalculationDate));
						sheetData.Append(GetEmptyRow());
						sheetData.Append(GetGroupedByBottlesClassificationsRows(rows));
						sheetData.Append(GetEmptyRow());
						sheetData.Append(GetGroupedByOrdersClassificationsRows(rows));
						worksheetPart.Worksheet.Append(sheetData);

						worksheetPart.Worksheet.Save();

						var sheet = new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Заказы" };
						var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
						sheets.AppendChild(sheet);

						spreadsheet.WorkbookPart.Workbook.Save();
					}

					reportData = stream.ToArray();
				}

				return reportData ?? Array.Empty<byte>();
			}

			private IEnumerable<Row> GetGroupedByBottlesClassificationsRows(IEnumerable<ClassificationCalculationReportRow> reportRows)
			{
				var groupedByBottlesClassification = (from r in reportRows
													  group r by new { r.NewClassificationByBottles, r.OldClassificationByBottles })
													 .ToDictionary(g => g.Key, g => g.ToList());

				var rows = new List<Row>
				{
					GetTableHeadersRow("По среднему количеству бутылей 19л за месяц")
				};

				foreach(var item in groupedByBottlesClassification)
				{
					var oldCategoryStringValue =
						(item.Key.OldClassificationByBottles.HasValue
						? item.Key.OldClassificationByBottles.ToString()
						: "Новый");

					var newCategoryStringValue = item.Key.NewClassificationByBottles.ToString();

					if(oldCategoryStringValue == newCategoryStringValue)
					{
						continue;
					}

					var categoryChangedValue = $"{oldCategoryStringValue} -> {newCategoryStringValue}";

					rows.Add(GetTableSubheaderDataRow(categoryChangedValue));
					rows.AddRange(GetTableDataRows(item.Value));
					rows.Add(GetEmptyRow());
				}

				return rows;
			}

			private IEnumerable<Row> GetGroupedByOrdersClassificationsRows(IEnumerable<ClassificationCalculationReportRow> reportRows)
			{
				var groupedByOrdersClassification = (from r in reportRows
													 group r by new { r.NewClassificationByOrders, r.OldClassificationByOrders })
													 .ToDictionary(g => g.Key, g => g.ToList());

				var rows = new List<Row>
				{
					GetTableHeadersRow("По среднему количеству заказов")
				};

				foreach(var item in groupedByOrdersClassification)
				{
					var oldCategoryStringValue =
						(item.Key.OldClassificationByOrders.HasValue
						? item.Key.OldClassificationByOrders.ToString()
						: "Новый");

					var newCategoryStringValue = item.Key.NewClassificationByOrders.ToString();

					if(oldCategoryStringValue == newCategoryStringValue)
					{
						continue;
					}

					var categoryChangedValue = $"{oldCategoryStringValue} -> {newCategoryStringValue}";

					rows.Add(GetTableSubheaderDataRow(categoryChangedValue));
					rows.AddRange(GetTableDataRows(item.Value));
					rows.Add(GetEmptyRow());
				}

				return rows;
			}

			#region Rows AndColumns
			private Columns CreateColumns(double defaultColumnWidth)
			{
				var columns = new Columns();

				var emptyColumn = CreateColumn(1, defaultColumnWidth);
				var categoryFromToColumn = CreateColumn(2, defaultColumnWidth);
				var counterpartyColumn = CreateColumn(3, defaultColumnWidth * 2);
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

			private Row GetLastReportInfoRow(DateTime lastCalculationDate)
			{
				var row = new Row();

				var lastCalculationDateString =
					lastCalculationDate == default
					? "не выполнялся"
					: lastCalculationDate.ToString("dd.MM.yyyy");

				var value = $"Дата последнего пересчета: {lastCalculationDateString}";

				row.AppendChild(GetTableTitleStringCell(value));

				return row;
			}

			private Row GetTableHeadersRow(string value)
			{
				var row = new Row();

				row.AppendChild(GetTableHeaderStringCell(value));

				for(int i = 0; i < 10; i++)
				{
					row.AppendChild(GetTableHeaderEmptyCell());
				}

				return row;
			}

			private Row GetTableSubheaderDataRow(string categoryChangedValue)
			{
				var row = new Row();

				row.AppendChild(GetStringCell(""));
				row.AppendChild(GetStringCell(categoryChangedValue));
				row.AppendChild(GetStringCell("Контрагент"));
				row.AppendChild(GetStringCell("Кол бутылей"));
				row.AppendChild(GetStringCell("Кол бутылей нов"));
				row.AppendChild(GetStringCell("Оборот"));
				row.AppendChild(GetStringCell("Оборот нов"));
				row.AppendChild(GetStringCell("Частота"));
				row.AppendChild(GetStringCell("Частота нов"));
				row.AppendChild(GetStringCell("Категория"));
				row.AppendChild(GetStringCell("Категория нов"));

				return row;
			}

			private IEnumerable<Row> GetTableDataRows(IEnumerable<ClassificationCalculationReportRow> items)
			{
				var rows = new List<Row>();
				var counter = 0;

				foreach(var item in items)
				{
					counter++;
					var row = new Row();

					var oldCategory = item.OldClassificationByBottles == null || item.OldClassificationByOrders == null
						? "Новый"
						: item.OldClassificationByBottles.Value.ToString() + item.OldClassificationByOrders.Value.ToString();

					var newCategory =
						item.NewClassificationByBottles.ToString() + item.NewClassificationByOrders.ToString();

					row.AppendChild(GetStringCell(""));
					row.AppendChild(GetNumericCell(counter));
					row.AppendChild(GetStringCell(item.CounterpartyName));
					row.AppendChild(GetFloatingPointCell(item.OldAverageBottlesCount.HasValue ? item.OldAverageBottlesCount.Value : 0));
					row.AppendChild(GetFloatingPointCell(item.NewAverageBottlesCount));
					row.AppendChild(GetFloatingPointCell(item.OldAverageMoneyTurnoverSum.HasValue ? item.OldAverageMoneyTurnoverSum.Value : 0));
					row.AppendChild(GetFloatingPointCell(item.NewAverageMoneyTurnoverSum));
					row.AppendChild(GetFloatingPointCell(item.OldAverageOrdersCount.HasValue ? item.OldAverageOrdersCount.Value : 0));
					row.AppendChild(GetFloatingPointCell(item.NewAverageOrdersCount));
					row.AppendChild(GetStringCell(oldCategory));
					row.AppendChild(GetStringCell(newCategory));

					rows.Add(row);
				}

				return rows;
			}

			private Row GetEmptyRow()
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

				stylesheet.NumberingFormats = new NumberingFormats();
				stylesheet.NumberingFormats.Append(GetIntegegFormat());
				stylesheet.NumberingFormats.Append(GetDoublePrecisionDecimalFormat());
				stylesheet.NumberingFormats.Count = 2;

				var solidYellow = new PatternFill() { PatternType = PatternValues.Solid };
				solidYellow.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("FFFF00") };
				solidYellow.BackgroundColor = new BackgroundColor { Indexed = 64 };

				stylesheet.Fills = new Fills();
				stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } });
				stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } });
				stylesheet.Fills.AppendChild(new Fill { PatternFill = solidYellow });
				stylesheet.Fills.Count = 3;

				stylesheet.Borders = new Borders();
				stylesheet.Borders.AppendChild(new Border());
				stylesheet.Borders.AppendChild(GetCellBorder());
				stylesheet.Borders.Count = 2;

				var defaultCellFormat = new CellFormat { FormatId = 0, FontId = 0, BorderId = 1 };
				defaultCellFormat.Alignment = new Alignment { WrapText = true };

				var tableHeadersCellFormat = new CellFormat { FormatId = 0, FontId = 1, FillId = 2 };

				var tableTitleCellFormat = new CellFormat { FormatId = 0, FontId = 2 };

				var integerValuesCellFormat = new CellFormat { FormatId = 0, FontId = 0, BorderId = 1, NumberFormatId = 1 };
				integerValuesCellFormat.Alignment = new Alignment { WrapText = true };

				var decimalValuesCellFormat = new CellFormat { FormatId = 0, FontId = 0, BorderId = 1, NumberFormatId = 0 };
				decimalValuesCellFormat.Alignment = new Alignment { WrapText = true };

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

				stylesheet.CellFormats.AppendChild(integerValuesCellFormat);
				_integerFormatId = 4;

				stylesheet.CellFormats.AppendChild(decimalValuesCellFormat);
				_doublePrecisionDecimalFormatId = 5;

				stylesheet.CellFormats.Count = 6;

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

			private Cell GetTableHeaderEmptyCell()
			{
				var cell = new Cell
				{
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
					StyleIndex = _integerFormatId
				};

				return cell;
			}

			private Cell GetFloatingPointCell(decimal value)
			{
				var cell = new Cell
				{
					CellValue = new CellValue(value),
					DataType = CellValues.Number,
					StyleIndex = _doublePrecisionDecimalFormatId
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

			private NumberingFormat GetDoublePrecisionDecimalFormat()
			{
				var format = new NumberingFormat
				{
					NumberFormatId = UInt32Value.FromUInt32(0),
					FormatCode = StringValue.FromString("# ### ### ##0.00")
				};

				return format;
			}

			private NumberingFormat GetIntegegFormat()
			{
				var format = new NumberingFormat
				{
					NumberFormatId = UInt32Value.FromUInt32(1),
					FormatCode = StringValue.FromString("# ### ### ##0")
				};

				return format;
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

			private Font GetWorksheetTitleFont()
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
			#endregion Rows AndColumns
		}
	}
}
