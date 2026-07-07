using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DateTimeHelpers;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Gamma.Utilities;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class MarketingReport
	{
		public class ExcelExporter
		{
			private readonly MarketingReport _report;

			private uint _defaultFormatId;
			private uint _boldFormatId;
			private uint _descriptionFormatId;
			private uint _percent2FormatId;
			private uint _percent0FormatId;
			private uint _dateFormatId;
			private uint _monthFormatId;
			private uint _titleFormatId;
			private uint _settingsValueFormatId;

			private const uint CustomDateFormatId = 164;
			private const uint CustomMonthFormatId = 165;
			private const int MaxVisibleSliceColumns = 6;

			public ExcelExporter(MarketingReport report)
			{
				_report = report ?? throw new ArgumentNullException(nameof(report));
			}

			private string _filtersDescriptionMergeRange;

			public void Export(string path)
			{
				_filtersDescriptionMergeRange = null;

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
					worksheetPart.Worksheet.Append(BuildMergeCells());
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

				rowIndex = AppendTitleRow(sheetData, rowIndex);
				rowIndex = AppendSettingsBlock(sheetData, rowIndex);
				rowIndex++;

				foreach(var group in _report.Groups)
				{
					if(_report.Groups.Count > 1)
					{
						rowIndex = AppendGroupTitleRow(sheetData, rowIndex, group.Title);
						rowIndex++;
					}

					rowIndex = AppendMetricsBlock(sheetData, rowIndex, group.Metrics);
					rowIndex++;
				}

				return sheetData;
			}

			private uint AppendTitleRow(SheetData sheetData, uint rowIndex)
			{
				var row = CreateRow(rowIndex);
				row.Append(CreateStringCell("A", rowIndex, "Маркетинговый отчет", _titleFormatId));
				sheetData.Append(row);
				return rowIndex + 1;
			}

			private uint AppendSettingsBlock(SheetData sheetData, uint rowIndex)
			{
				rowIndex = AppendSettingsRow(sheetData, rowIndex, "Настройки", string.Empty, _boldFormatId);
				rowIndex = AppendSettingsRow(sheetData, rowIndex, "Период", $"с {_report.StartDate:dd.MM.yyyy} по {_report.EndDate:dd.MM.yyyy}");
				rowIndex = AppendSettingsRow(sheetData, rowIndex, "Разрез", _report.GroupingType.GetEnumTitle());
				rowIndex = AppendSettingsRow(sheetData, rowIndex, "Статусы заказа", "Выбранные фильтры:");
				rowIndex = AppendFiltersDescriptionRow(sheetData, rowIndex, _report.FiltersDescription);
				rowIndex = AppendSettingsRow(sheetData, rowIndex, "Дата заказа", _report.DateType.GetEnumTitle());
				return rowIndex;
			}

			private uint AppendFiltersDescriptionRow(SheetData sheetData, uint rowIndex, string filtersDescription)
			{
				var row = CreateRow(rowIndex, 60);
				row.Append(CreateStringCell("A", rowIndex, string.Empty));
				row.Append(CreateStringCell("B", rowIndex, string.Empty));
				row.Append(CreateStringCell("C", rowIndex, filtersDescription ?? string.Empty, _settingsValueFormatId));
				sheetData.Append(row);
				_filtersDescriptionMergeRange = $"C{rowIndex}:F{rowIndex}";
				return rowIndex + 1;
			}

			private uint AppendGroupTitleRow(SheetData sheetData, uint rowIndex, string title)
			{
				var row = CreateRow(rowIndex);
				row.Append(CreateStringCell("A", rowIndex, title, _boldFormatId));
				sheetData.Append(row);
				return rowIndex + 1;
			}

			private uint AppendMetricsBlock(SheetData sheetData, uint rowIndex, MarketingReportMetrics metrics)
			{
				rowIndex = AppendSimpleMetricRow(sheetData, rowIndex, "Всего контрагентов", metrics.TotalCounterparties, "N0");
				rowIndex = AppendActiveBaseRow(sheetData, rowIndex, metrics);
				rowIndex++;

				rowIndex = AppendTimeSeriesMetric(
					sheetData,
					rowIndex,
					"1. DAU (Daily Active Users) - ежедневно активные пользователи\r\nКоличество уникальных пользователей, совершивших покупку за один день.",
					"Average DAU",
					metrics.AverageDau,
					metrics.DailyActiveClients,
					DateTimeSliceType.Day);
				rowIndex++;

				rowIndex = AppendTimeSeriesMetric(
					sheetData,
					rowIndex,
					"2. WAU (Weekly Active Users) - еженедельно активные пользователи\r\nКоличество уникальных пользователей, совершивших покупку за неделю.",
					"Average WAU",
					metrics.AverageWau,
					metrics.WeeklyActiveClients,
					DateTimeSliceType.Week);
				rowIndex++;

				rowIndex = AppendTimeSeriesMetric(
					sheetData,
					rowIndex,
					"3. MAU (Monthly Active Users) - ежемесячно активные пользователи\r\nКоличество уникальных пользователей, совершивших покупку за месяц.",
					"Average MAU",
					metrics.AverageMau,
					metrics.MonthlyActiveClients,
					DateTimeSliceType.Month);
				rowIndex++;

				rowIndex = AppendDescriptionWithValueRow(
					sheetData,
					rowIndex,
					"4. Коэффициент «Липкости» (Sticky Factor)\r\nПоказывает, насколько продукт «удерживает» пользователей.\r\nФормула: Sticky Factor = AvgDAU/AvgMAU*100%",
					metrics.StickyFactor,
					isPercent: true);
				rowIndex++;

				rowIndex = AppendDescriptionRow(
					sheetData,
					rowIndex,
					"5. Частота заказов на клиента - среднее количество заказов, которое делает один клиент за определённый период");
				rowIndex = AppendSubMetricRow(sheetData, rowIndex, "день", metrics.OrdersFrequencyPerDay, "N2");
				rowIndex = AppendSubMetricRow(sheetData, rowIndex, "неделя", metrics.OrdersFrequencyPerWeek, "N2");
				rowIndex = AppendSubMetricRow(sheetData, rowIndex, "месяц", metrics.OrdersFrequencyPerMonth, "N2");
				rowIndex++;

				rowIndex = AppendDescriptionWithValueRow(
					sheetData,
					rowIndex,
					"6. Средний объём заказа - среднее количество бутылей 19 л, которое клиент заказывает за один раз.",
					metrics.AverageOrderVolume19L,
					"N1");
				rowIndex++;

				rowIndex = AppendDescriptionWithValueRow(
					sheetData,
					rowIndex,
					"7. Средний чек",
					metrics.AverageCheck,
					"N0");
				rowIndex++;

				rowIndex = AppendDescriptionWithValueRow(
					sheetData,
					rowIndex,
					"8. Средний интервал между заказами - время, которое проходит между двумя последовательными заказами одного клиента",
					$"{metrics.AverageIntervalBetweenOrdersDays.ToString("N1", CultureInfo.CurrentCulture)} дней",
					isText: true);
				rowIndex++;

				rowIndex = AppendDescriptionWithValueRow(
					sheetData,
					rowIndex,
					"9. Конверсия из пробного заказа в регулярный - процент клиентов, которые после первичного заказа сделали второй",
					metrics.TrialToRegularConversion,
					isPercent: true);
				rowIndex++;

				rowIndex = AppendDescriptionWithValueRow(
					sheetData,
					rowIndex,
					"10. Доля клиентов, использующих доп.услуги - например, аренда кулеров, сан.обработка, покупка сопутки (стаканчики, чай)",
					metrics.AdditionalServicesClientsShare,
					isPercent: true);
				rowIndex++;

				rowIndex = AppendDescriptionRow(
					sheetData,
					rowIndex,
					"11. Срок жизни клиента (Customer Lifetime) - период, в течение которого клиент продолжает пользоваться продуктом\r\nРазница между датой первой и последней покупки клиентской базы. Считаем в днях и переводим в месяцы по календарному среднему арифметическому.\r\nФормула: Customer Lifetime = Σ по всем клиентам(время между первой и последней покупкой) / количество клиентов");
				rowIndex = AppendCustomerLifetimeRow(sheetData, rowIndex, metrics);
				rowIndex++;

				rowIndex = AppendDescriptionRow(
					sheetData,
					rowIndex,
					"12. Оценка и уровень удовлетворённости - оценка клиентов через систему оценки заказов\r\nЕсли нет оценки, считаем 5 баллов");
				rowIndex = AppendSubMetricRow(sheetData, rowIndex, string.Empty, metrics.AverageSatisfaction, "N2");
				rowIndex++;

				rowIndex = AppendDescriptionRow(
					sheetData,
					rowIndex,
					"13. Отток клиентов (Churn Rate) - процент клиентов, которые прекратили пользоваться услугой за определённый период.\r\nПример: на 1 января было 1 000 активных клиентов, считаем отток за последние 3 мес.\r\nК 1 апреля из январской уникальной базы 50 клиентов перестали заказывать на следующие 3 мес.\r\nChurn Rate = 50 / 1 000 = 0,05 (или 5%)");
				rowIndex = AppendSubMetricRow(sheetData, rowIndex, string.Empty, metrics.ChurnRate, isPercent: true);
				rowIndex++;

				rowIndex = AppendDescriptionRow(
					sheetData,
					rowIndex,
					"14. Коэффициент удержания клиентов (Retention rate) - показывает процент пользователей, совершивших повторную покупку за определённый период.\r\nФормула расчёта: RR = (E-N)/S*100%\r\nE = количество клиентов на конец периода.\r\nN = количество новых клиентов, привлечённых за период.\r\nS = количество клиентов на начало периода");
				rowIndex = AppendSubMetricRow(sheetData, rowIndex, string.Empty, metrics.RetentionRate, isPercent: true);
				rowIndex++;

				rowIndex = AppendDescriptionRow(
					sheetData,
					rowIndex,
					"15. LTV (Lifetime Value) - прогнозируемая ценность клиента, показывающая прибыль от одного пользователя за всё время взаимодействия с компанией.\r\nОсновная формула: LTV = Средний чек × Частота покупок(например, за месяц) × Срок жизни клиента (в месяцах)");
				rowIndex = AppendLtvRow(sheetData, rowIndex, metrics);

				return rowIndex;
			}

			private uint AppendActiveBaseRow(SheetData sheetData, uint rowIndex, MarketingReportMetrics metrics)
			{
				var row = CreateRow(rowIndex, 45);
				row.Append(CreateStringCell(
					"A",
					rowIndex,
					"Процент активной базы\r\nКоличество уникальных клиентов, совершивших покупку за последние 3 месяца из всей базы",
					_descriptionFormatId));
				row.Append(CreateStringCell("B", rowIndex, string.Empty));
				row.Append(CreateNumberCell("C", rowIndex, metrics.ActiveClientsCount, _defaultFormatId));
				row.Append(CreatePercentCell("D", rowIndex, (double)metrics.ActiveBasePercent, _percent2FormatId));
				sheetData.Append(row);
				return rowIndex + 1;
			}

			private uint AppendTimeSeriesMetric(
				SheetData sheetData,
				uint rowIndex,
				string description,
				string averageTitle,
				double averageValue,
				Dictionary<string, int> valuesBySlice,
				DateTimeSliceType sliceType)
			{
				rowIndex = AppendDescriptionRow(sheetData, rowIndex, description);
				rowIndex++;

				var slices = DateTimeSliceFactory.CreateSlices(sliceType, _report.StartDate, _report.EndDate)
					.Cast<DateTimeSlice>()
					.ToList();
				var sampledSlices = SampleSlices(slices);

				var headerRow = CreateRow(rowIndex);
				headerRow.Append(CreateStringCell("A", rowIndex, string.Empty));
				headerRow.Append(CreateStringCell("B", rowIndex, averageTitle, _descriptionFormatId));

				var columnIndex = 2;
				var ellipsisInserted = false;
				for(var i = 0; i < sampledSlices.Count; i++)
				{
					var slice = sampledSlices[i];
					if(slice == null)
					{
						headerRow.Append(CreateStringCell(GetColumnName(columnIndex++), rowIndex, "...", _descriptionFormatId));
						ellipsisInserted = true;
						continue;
					}

					headerRow.Append(CreateSliceHeaderCell(GetColumnName(columnIndex++), rowIndex, slice, sliceType));
				}

				if(!ellipsisInserted && slices.Count > sampledSlices.Count)
				{
					headerRow.Append(CreateStringCell(GetColumnName(columnIndex++), rowIndex, "...", _descriptionFormatId));
				}

				sheetData.Append(headerRow);

				var valueRow = CreateRow(rowIndex + 1);
				valueRow.Append(CreateStringCell("A", rowIndex + 1, string.Empty));
				valueRow.Append(CreateNumberCell("B", rowIndex + 1, averageValue, _defaultFormatId, "N0"));

				columnIndex = 2;
				ellipsisInserted = false;
				foreach(var slice in sampledSlices)
				{
					if(slice == null)
					{
						valueRow.Append(CreateStringCell(GetColumnName(columnIndex++), rowIndex + 1, string.Empty));
						ellipsisInserted = true;
						continue;
					}

					var value = valuesBySlice.TryGetValue(slice.ToString(), out var count) ? count : 0;
					valueRow.Append(CreateNumberCell(GetColumnName(columnIndex++), rowIndex + 1, value, _defaultFormatId, "N0"));
				}

				if(!ellipsisInserted && slices.Count > sampledSlices.Count)
				{
					valueRow.Append(CreateStringCell(GetColumnName(columnIndex++), rowIndex + 1, string.Empty));
				}

				sheetData.Append(valueRow);
				return rowIndex + 2;
			}

			private static IList<DateTimeSlice> SampleSlices(IList<DateTimeSlice> slices)
			{
				if(slices.Count <= MaxVisibleSliceColumns)
				{
					return slices;
				}

				var result = new List<DateTimeSlice>();
				result.AddRange(slices.Take(4));
				result.Add(null);
				result.Add(slices.Last());
				return result;
			}

			private Cell CreateSliceHeaderCell(string column, uint rowIndex, DateTimeSlice slice, DateTimeSliceType sliceType)
			{
				switch(sliceType)
				{
					case DateTimeSliceType.Day:
						return CreateDateCell(column, rowIndex, slice.StartDate, _dateFormatId);
					case DateTimeSliceType.Month:
						return CreateDateCell(column, rowIndex, slice.StartDate, _monthFormatId);
					default:
						return CreateStringCell(
							column,
							rowIndex,
							$"{slice.WeekNumber}.{slice.StartDate:yy}",
							_descriptionFormatId);
				}
			}

			private uint AppendDescriptionRow(SheetData sheetData, uint rowIndex, string description)
			{
				var row = CreateRow(rowIndex, 45);
				row.Append(CreateStringCell("A", rowIndex, description, _descriptionFormatId));
				sheetData.Append(row);
				return rowIndex + 1;
			}

			private uint AppendSimpleMetricRow(SheetData sheetData, uint rowIndex, string title, int value, string numberFormat)
			{
				var row = CreateRow(rowIndex);
				row.Append(CreateStringCell("A", rowIndex, title, _descriptionFormatId));
				row.Append(CreateStringCell("B", rowIndex, string.Empty));
				row.Append(CreateNumberCell("C", rowIndex, value, _defaultFormatId, numberFormat));
				sheetData.Append(row);
				return rowIndex + 1;
			}

			private uint AppendDescriptionWithValueRow(
				SheetData sheetData,
				uint rowIndex,
				string description,
				object value,
				string numberFormat = null,
				bool isPercent = false,
				bool isText = false)
			{
				rowIndex = AppendDescriptionRow(sheetData, rowIndex, description);
				rowIndex++;

				var row = CreateRow(rowIndex);
				row.Append(CreateStringCell("A", rowIndex, string.Empty));
				if(isText)
				{
					row.Append(CreateStringCell("B", rowIndex, value?.ToString() ?? string.Empty, _descriptionFormatId));
				}
				else if(isPercent)
				{
					row.Append(CreatePercentCell("B", rowIndex, Convert.ToDouble(value), _percent0FormatId));
				}
				else if(value is decimal decimalValue)
				{
					row.Append(CreateNumberCell("B", rowIndex, decimalValue, _defaultFormatId, numberFormat));
				}
				else if(value is double doubleValue)
				{
					row.Append(CreateNumberCell("B", rowIndex, doubleValue, _defaultFormatId, numberFormat));
				}
				else if(value is int intValue)
				{
					row.Append(CreateNumberCell("B", rowIndex, intValue, _defaultFormatId, numberFormat));
				}

				sheetData.Append(row);
				return rowIndex + 1;
			}

			private uint AppendSubMetricRow(
				SheetData sheetData,
				uint rowIndex,
				string label,
				object value,
				string numberFormat = null,
				bool isPercent = false)
			{
				var row = CreateRow(rowIndex);
				row.Append(CreateStringCell("A", rowIndex, string.Empty));
				row.Append(CreateStringCell("B", rowIndex, label, _descriptionFormatId));

				if(isPercent)
				{
					row.Append(CreatePercentCell("C", rowIndex, Convert.ToDouble(value), _percent0FormatId));
				}
				else if(value is decimal decimalValue)
				{
					row.Append(CreateNumberCell("C", rowIndex, decimalValue, _defaultFormatId, numberFormat));
				}
				else if(value is double doubleValue)
				{
					row.Append(CreateNumberCell("C", rowIndex, doubleValue, _defaultFormatId, numberFormat));
				}

				sheetData.Append(row);
				return rowIndex + 1;
			}

			private uint AppendCustomerLifetimeRow(SheetData sheetData, uint rowIndex, MarketingReportMetrics metrics)
			{
				var row = CreateRow(rowIndex);
				row.Append(CreateStringCell("A", rowIndex, string.Empty));
				row.Append(CreateStringCell(
					"B",
					rowIndex,
					$"{metrics.CustomerLifetimeDays.ToString("N0", CultureInfo.CurrentCulture)} дней",
					_descriptionFormatId));
				row.Append(CreateNumberCell("C", rowIndex, metrics.CustomerLifetimeMonths, _defaultFormatId, "N1"));
				row.Append(CreateStringCell("D", rowIndex, "месяц", _descriptionFormatId));
				sheetData.Append(row);
				return rowIndex + 1;
			}

			private uint AppendLtvRow(SheetData sheetData, uint rowIndex, MarketingReportMetrics metrics)
			{
				var row = CreateRow(rowIndex);
				row.Append(CreateStringCell("A", rowIndex, string.Empty));

				var formulaExample =
					$"{metrics.AverageCheck.ToString("N0", CultureInfo.CurrentCulture)}р * " +
					$"{metrics.OrdersFrequencyPerMonth.ToString("N1", CultureInfo.CurrentCulture)}раз * " +
					$"{metrics.CustomerLifetimeMonths.ToString("N1", CultureInfo.CurrentCulture)}мес. = " +
					$"{metrics.LifetimeValue.ToString("N0", CultureInfo.CurrentCulture)}р";

				row.Append(CreateStringCell("B", rowIndex, formulaExample, _descriptionFormatId));
				sheetData.Append(row);
				return rowIndex + 1;
			}

			private uint AppendSettingsRow(
				SheetData sheetData,
				uint rowIndex,
				string label,
				string value,
				uint labelStyleId = 0,
				uint valueStyleId = 0)
			{
				var row = CreateRow(rowIndex, string.IsNullOrWhiteSpace(value) ? 15 : 50);
				row.Append(CreateStringCell("A", rowIndex, string.Empty));

				if(!string.IsNullOrWhiteSpace(label))
				{
					row.Append(CreateStringCell("B", rowIndex, label, labelStyleId == 0 ? _boldFormatId : labelStyleId));
				}
				else
				{
					row.Append(CreateStringCell("B", rowIndex, string.Empty));
				}

				row.Append(CreateStringCell(
					"C",
					rowIndex,
					value ?? string.Empty,
					valueStyleId == 0 ? _settingsValueFormatId : valueStyleId));
				sheetData.Append(row);
				return rowIndex + 1;
			}

			private MergeCells BuildMergeCells()
			{
				var mergeCells = new MergeCells();

				if(!string.IsNullOrWhiteSpace(_filtersDescriptionMergeRange))
				{
					mergeCells.Append(new MergeCell { Reference = _filtersDescriptionMergeRange });
				}

				return mergeCells;
			}

			private Columns BuildColumns()
			{
				return new Columns(
					CreateColumn(1, 48),
					CreateColumn(2, 14),
					CreateColumn(3, 14),
					CreateColumn(4, 14),
					CreateColumn(5, 12, 20));
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

			private static Column CreateColumn(uint min, uint max, double width)
			{
				return new Column
				{
					Min = min,
					Max = max,
					Width = width,
					CustomWidth = true
				};
			}

			private static Row CreateRow(uint rowIndex, double? height = null)
			{
				var row = new Row { RowIndex = rowIndex };

				if(height.HasValue)
				{
					row.Height = height.Value;
					row.CustomHeight = true;
				}

				return row;
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

			private Cell CreateNumberCell(string column, uint rowIndex, double value, uint styleIndex, string format = null)
			{
				return new Cell
				{
					CellReference = column + rowIndex,
					StyleIndex = styleIndex,
					DataType = CellValues.Number,
					CellValue = new CellValue(value.ToString(CultureInfo.InvariantCulture))
				};
			}

			private Cell CreateNumberCell(string column, uint rowIndex, decimal value, uint styleIndex, string format = null)
			{
				return CreateNumberCell(column, rowIndex, (double)value, styleIndex, format);
			}

			private Cell CreateNumberCell(string column, uint rowIndex, int value, uint styleIndex, string format = null)
			{
				return CreateNumberCell(column, rowIndex, (double)value, styleIndex, format);
			}

			private Cell CreatePercentCell(string column, uint rowIndex, double value, uint styleIndex)
			{
				return new Cell
				{
					CellReference = column + rowIndex,
					StyleIndex = styleIndex,
					DataType = CellValues.Number,
					CellValue = new CellValue(value.ToString(CultureInfo.InvariantCulture))
				};
			}

			private Cell CreateDateCell(string column, uint rowIndex, DateTime date, uint styleIndex)
			{
				return new Cell
				{
					CellReference = column + rowIndex,
					StyleIndex = styleIndex,
					DataType = CellValues.Number,
					CellValue = new CellValue(date.ToOADate().ToString(CultureInfo.InvariantCulture))
				};
			}

			private static string GetColumnName(int index)
			{
				var dividend = index + 1;
				var columnName = string.Empty;

				while(dividend > 0)
				{
					var modulo = (dividend - 1) % 26;
					columnName = Convert.ToChar(65 + modulo) + columnName;
					dividend = (dividend - modulo) / 26;
				}

				return columnName;
			}

			private Stylesheet BuildStylesheet()
			{
				_defaultFormatId = 1;
				_boldFormatId = 2;
				_descriptionFormatId = 3;
				_percent2FormatId = 4;
				_dateFormatId = 5;
				_monthFormatId = 6;
				_percent0FormatId = 7;
				_titleFormatId = 8;
				_settingsValueFormatId = 9;

				return new Stylesheet(
					new NumberingFormats(
						new NumberingFormat { NumberFormatId = CustomDateFormatId, FormatCode = "dd.MM.yyyy" },
						new NumberingFormat { NumberFormatId = CustomMonthFormatId, FormatCode = "mmm.yy" })
					{ Count = 2 },
					new Fonts(
						new Font(new FontSize { Val = 10 }, new FontName { Val = "Arial" }),
						new Font(new Bold(), new FontSize { Val = 10 }, new FontName { Val = "Arial" }),
						new Font(new FontName { Val = "Arial" }),
						new Font(new Bold(), new FontSize { Val = 14 }, new FontName { Val = "Arial" }))
					{ Count = 4 },
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
						CreateCellFormat(2, wrapText: true, vertical: VerticalAlignmentValues.Top),
						CreateCellFormat(2, numberFormatId: 10),
						CreateCellFormat(2, numberFormatId: CustomDateFormatId),
						CreateCellFormat(2, numberFormatId: CustomMonthFormatId),
						CreateCellFormat(2, numberFormatId: 9),
						CreateCellFormat(3),
						CreateCellFormat(2, wrapText: true, vertical: VerticalAlignmentValues.Top))
					{ Count = 10 });
			}

			private static CellFormat CreateCellFormat(
				uint fontId,
				uint numberFormatId = 0,
				bool wrapText = false,
				VerticalAlignmentValues vertical = VerticalAlignmentValues.Bottom)
			{
				var format = new CellFormat
				{
					FontId = fontId,
					ApplyFont = true,
					ApplyAlignment = true,
					Alignment = new Alignment
					{
						WrapText = wrapText,
						Vertical = vertical
					}
				};

				if(numberFormatId > 0)
				{
					format.NumberFormatId = numberFormatId;
					format.ApplyNumberFormat = true;
				}

				return format;
			}
		}
	}
}
