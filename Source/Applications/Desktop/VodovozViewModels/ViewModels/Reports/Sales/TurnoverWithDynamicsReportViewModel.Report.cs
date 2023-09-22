using DateTimeHelpers;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Gamma.Utilities;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Reports.Editing.Modifiers;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
	{
		public partial class TurnoverWithDynamicsReport
		{
			private readonly Func<int, decimal> _warehouseNomenclatureBalanceCallback;

			private TurnoverWithDynamicsReport(
				DateTime startDate,
				DateTime endDate,
				string filters,
				IEnumerable<GroupingType> groupingBy,
				DateTimeSliceType slicingType,
				MeasurementUnitEnum measurementUnit,
				bool showDynamics,
				DynamicsInEnum dynamicsIn,
				bool showLastSale,
				bool showResidueForNomenclaturesWithoutSales,
				bool showContacts,
				Func<int, decimal> warehouseNomenclatureBalanceCallback,
				Func<TurnoverWithDynamicsReport, IList<OrderItemNode>> dataFetchCallback)
			{
				StartDate = startDate;
				EndDate = endDate;
				Filters = filters;
				GroupingBy = groupingBy;
				SliceType = slicingType;
				MeasurementUnit = measurementUnit;
				ShowDynamics = showDynamics;
				DynamicsIn = dynamicsIn;
				ShowLastSale = showLastSale;
				ShowResidueForNomenclaturesWithoutSales = showResidueForNomenclaturesWithoutSales;
				ShowContacts = showContacts;
				_warehouseNomenclatureBalanceCallback = warehouseNomenclatureBalanceCallback;
				Slices = DateTimeSliceFactory.CreateSlices(slicingType, startDate, endDate).ToList();
				CreatedAt = DateTime.Now;
				Rows = ProcessData(dataFetchCallback(this));
				DisplayRows = ProcessTreeViewDisplay();
			}

			private IList<TurnoverWithDynamicsReportRow> ProcessTreeViewDisplay()
			{
				return new List<TurnoverWithDynamicsReportRow>
				{
					ReportTotal,
					new TurnoverWithDynamicsReportRow
					{
						Title = GroupingTitle,
						RowType = TurnoverWithDynamicsReportRow.RowTypes.Subheader,
						SliceColumnValues = CreateInitializedBy(Slices.Count, 0m),
						DynamicColumns = CreateInitializedBy(ShowDynamics ? Slices.Count * 2 : Slices.Count, ""),
						LastSaleDetails = new TurnoverWithDynamicsReportLastSaleDetails()
					}
				}.Union(Rows).ToList();
			}

			#region Parameters
			public DateTime StartDate { get; }

			public DateTime EndDate { get; }

			public string Filters { get; }

			public IEnumerable<GroupingType> GroupingBy { get; }

			public string GroupingTitle => string.Join(" | ", GroupingBy.Select(x => x.GetEnumTitle()));

			public DateTimeSliceType SliceType { get; }

			public MeasurementUnitEnum MeasurementUnit { get; }

			public bool ShowDynamics { get; }

			public DynamicsInEnum DynamicsIn { get; }

			public bool ShowLastSale { get; }

			public bool ShowResidueForNomenclaturesWithoutSales { get; }

			public bool ShowContacts { get; }

			public DateTime CreatedAt { get; }
			#endregion

			public string ReportTitle => $"Отчет по оборачиваемости с {StartDate:dd.MM.yyyy} по {EndDate:dd.MM.yyyy}";

			/// <summary>
			/// Временные срезы отчета
			/// </summary>
			public IList<IDateTimeSlice> Slices { get; }

			public IList<TurnoverWithDynamicsReportRow> Rows { get; }

			public IList<TurnoverWithDynamicsReportRow> DisplayRows { get; }

			public TurnoverWithDynamicsReportRow ReportTotal { get; private set; }

			public string SliceTypeString => SliceType.GetEnumTitle();

			public string MeasurementUnitString => MeasurementUnit.GetEnumTitle();

			public string DynamicsInStringShort => DynamicsIn == DynamicsInEnum.Percents ? "%" :
				MeasurementUnit == MeasurementUnitEnum.Price ? "Руб." : "Шт.";

			/// <summary>
			/// Зависит от текущего значения <see cref="MeasurementUnit"/>
			/// </summary>
			public string MeasurementUnitFormat => MeasurementUnit == MeasurementUnitEnum.Amount ? "# ### ### ##0" : "# ### ### ##0.00";

			private IList<TurnoverWithDynamicsReportRow> ProcessData(IList<OrderItemNode> ordersItemslist)
			{
				var groupingCount = GroupingBy.Count();

				switch(groupingCount)
				{
					case 3:
						var result3 = Process3rdLevelGroups(ordersItemslist);

						var group3Total = AddGroupTotals("Сводные данные по отчету", result3.Totals);

						ReportTotal = group3Total;

						ProcessIndexes(result3.Rows);

						return result3.Rows;
					case 2:
						var result2nd = Process2ndLevelGroups(ordersItemslist);

						var group2Total = AddGroupTotals("Сводные данные по отчету", result2nd.Totals);

						ReportTotal = group2Total;

						ProcessIndexes(result2nd.Rows);

						return result2nd.Rows;
					default:
						var result = Process1stLevelGroups(ordersItemslist);

						result.TotalRow.Title = "Сводные данные по отчету";

						ReportTotal = result.TotalRow;

						ProcessIndexes(result.Rows);

						return result.Rows;
				}
			}

			private void ProcessIndexes(IList<TurnoverWithDynamicsReportRow> Rows)
			{
				int index = 1;

				foreach(var item in Rows)
				{
					if(item.RowType == TurnoverWithDynamicsReportRow.RowTypes.Values)
					{
						item.Index = index.ToString();
						index++;
					}
				}
			}

			private (IList<TurnoverWithDynamicsReportRow> Rows, TurnoverWithDynamicsReportRow TotalRow) Process1stLevelGroups(
				IEnumerable<OrderItemNode> firstLevelGroup)
			{
				var result = new List<TurnoverWithDynamicsReportRow>();

				var firstSelector = GetSelector(GroupingBy.Last());

				var firstLevelKeyValues = firstLevelGroup.Select(firstSelector).Distinct();

				foreach(var key1 in firstLevelKeyValues)
				{
					var t = key1;

					var filtered = firstLevelGroup.Where(x => firstSelector.Invoke(x)?.Equals(key1) ?? firstSelector.Invoke(x) == key1);

					if(!filtered.Any())
					{
						continue;
					}

					var groupTitle = GetGroupTitle(GroupingBy.Last()).Invoke(filtered.First());

					string phones = string.Empty;
					string emails = string.Empty;

					if(ShowContacts)
					{
						phones = ProcessCounterpartyPhones(filtered.First());
						emails = filtered.First().CounterpartyEmails;
					}

					var row = new TurnoverWithDynamicsReportRow
					{
						RowType = TurnoverWithDynamicsReportRow.RowTypes.Values,
						Title = groupTitle,
						Phones = phones,
						Emails = emails
					};

					row.SliceColumnValues = CalculateValuesRow(filtered);

					ProcessDynamics(row);
					ProcessLastSale(filtered, row);

					result.Add(row);
				}

				var groupTotal = AddGroupTotals("", result);

				return (result, groupTotal);
			}

			private (IList<TurnoverWithDynamicsReportRow> Rows, IList<TurnoverWithDynamicsReportRow> Totals) Process2ndLevelGroups(
				IEnumerable<OrderItemNode> secondLevelGroup)
			{
				var result = new List<TurnoverWithDynamicsReportRow>();

				IList<TurnoverWithDynamicsReportRow> totalsRows = new List<TurnoverWithDynamicsReportRow>();

				var preLast = GroupingBy.Count() - 2;

				var firstSelector = GetSelector(GroupingBy.ElementAt(preLast));

				var firstLevelKeyValues = secondLevelGroup.Select(firstSelector).Distinct();

				foreach(var key1 in firstLevelKeyValues)
				{
					var filtered = secondLevelGroup.Where(x => firstSelector.Invoke(x)?.Equals(key1) ?? firstSelector.Invoke(x) == key1);

					if(!filtered.Any())
					{
						continue;
					}

					var groupTitle = GetGroupTitle(GroupingBy.ElementAt(preLast)).Invoke(filtered.First());

					var groupRows = Process1stLevelGroups(filtered);

					groupRows.TotalRow.Title = groupTitle;

					totalsRows.Add(groupRows.TotalRow);
					groupRows.Rows.Insert(0, groupRows.TotalRow);
					result = result.Union(groupRows.Rows).ToList();
				}

				return (result, totalsRows);
			}

			private (IList<TurnoverWithDynamicsReportRow> Rows, IList<TurnoverWithDynamicsReportRow> Totals) Process3rdLevelGroups(
				IEnumerable<OrderItemNode> thirdLevelGroup)
			{
				var result = new List<TurnoverWithDynamicsReportRow>();

				IList<TurnoverWithDynamicsReportRow> totalsRows = new List<TurnoverWithDynamicsReportRow>();

				var prePreLast = GroupingBy.Count() - 3;

				var firstSelector = GetSelector(GroupingBy.ElementAt(prePreLast));

				var firstLevelKeyValues = thirdLevelGroup.Select(firstSelector).Distinct();

				foreach(var key1 in firstLevelKeyValues)
				{
					var filtered = thirdLevelGroup.Where(x => firstSelector.Invoke(x)?.Equals(key1) ?? firstSelector.Invoke(x) == key1);

					if(!filtered.Any())
					{
						continue;
					}

					var groupTitle = GetGroupTitle(GroupingBy.ElementAt(prePreLast)).Invoke(filtered.First());

					var groupRows = Process2ndLevelGroups(filtered);

					var groupTotal = AddGroupTotals(groupTitle, groupRows.Totals);

					totalsRows.Add(groupTotal);
					groupRows.Rows.Insert(0, groupTotal);
					result = result.Union(groupRows.Rows).ToList();
				}

				return (result, totalsRows);
			}

			private Func<OrderItemNode, object> GetSelector(GroupingType groupingType)
			{
				switch(groupingType)
				{
					case GroupingType.Order:
						return x => x.OrderId;
					case GroupingType.Counterparty:
						return x => x.CounterpartyId;
					case GroupingType.Subdivision:
						return x => x.SubdivisionId;
					case GroupingType.DeliveryDate:
						return x => x.OrderDeliveryDate;
					case GroupingType.RouteList:
						return x => x.RouteListId;
					case GroupingType.Nomenclature:
						return x => x.NomenclatureId;
					case GroupingType.NomenclatureType:
						return x => x.NomenclatureCategory;
					case GroupingType.NomenclatureGroup:
						return x => x.ProductGroupId;
					case GroupingType.CounterpartyType:
						return x => x.CounterpartyType;
					case GroupingType.PaymentType:
						return x => x.PaymentType;
					case GroupingType.Organization:
						return x => x.OrganizationId;
					default:
						return x => x.Id;
				}
			}

			public Func<OrderItemNode, string> GetGroupTitle(GroupingType groupingType)
			{
				switch(groupingType)
				{
					case GroupingType.Order:
						return x => x.OrderId.ToString();
					case GroupingType.Counterparty:
						return x => x.CounterpartyFullName;
					case GroupingType.Subdivision:
						return x => x.SubdivisionName;
					case GroupingType.DeliveryDate:
						return x => x.OrderDeliveryDate?.ToString("yyyy-MM-dd") ?? "Без даты доставки";
					case GroupingType.RouteList:
						return x => x.RouteListId?.ToString() ?? "Без маршрутного листа";
					case GroupingType.Nomenclature:
						return x => x.NomenclatureOfficialName;
					case GroupingType.NomenclatureType:
						return x => x.NomenclatureCategory.GetEnumTitle();
					case GroupingType.NomenclatureGroup:
						return x => x.ProductGroupName;
					case GroupingType.CounterpartyType:
						return x => x.CounterpartyType.GetEnumTitle();
					case GroupingType.PaymentType:
						return x => x.PaymentType.GetEnumTitle();
					case GroupingType.Organization:
						return x => x.OrganizationName;
					default:
						return x => x.Id.ToString();
				}
			}

			private IList<T> CreateInitializedBy<T>(int length, T initializer)
			{
				var result = new List<T>(length);

				for(var i = 0; i < length; i++)
				{
					result.Add(initializer);
				}

				return result;
			}

			private string ProcessCounterpartyPhones(OrderItemNode counterpartyGroup)
			{
				var result = string.Empty;

				var counterpartyPhones = counterpartyGroup.CounterpartyPhones;

				if(string.IsNullOrWhiteSpace(counterpartyPhones))
				{
					counterpartyPhones = string.Empty;
				}

				var ordersContactPhones = counterpartyGroup
					.OrderContactPhone
					.Where(ocp => !counterpartyPhones.Contains(ocp))
					.Distinct();

				string resultedOrderContactPhones = string.Empty;

				if(ordersContactPhones.Any())
				{
					resultedOrderContactPhones = string.Join(",\n", ordersContactPhones);

					if(counterpartyPhones != string.Empty)
					{
						result = resultedOrderContactPhones + ",\n" + counterpartyPhones;
					}
					else
					{
						result = resultedOrderContactPhones;
					}
				}
				else
				{
					result = counterpartyPhones;
				}

				return result;
			}

			private void ProcessLastSale(IEnumerable<OrderItemNode> nomenclatureGroup, TurnoverWithDynamicsReportRow row)
			{
				if(ShowLastSale)
				{
					var lastDelivery = nomenclatureGroup
						.OrderBy(oi => oi.OrderDeliveryDate)
						.Last().OrderDeliveryDate.Value;

					row.LastSaleDetails = new TurnoverWithDynamicsReportLastSaleDetails
					{
						LastSaleDate = lastDelivery,
						DaysFromLastShipment = Math.Floor((CreatedAt - lastDelivery).TotalDays),
						WarhouseResidue = GroupingBy.LastOrDefault() == GroupingType.Nomenclature
							? _warehouseNomenclatureBalanceCallback.Invoke(nomenclatureGroup.First().NomenclatureId)
							: 0
					};
				}
			}

			private void ProcessDynamics(TurnoverWithDynamicsReportRow row)
			{
				if(ShowDynamics)
				{
					row.DynamicColumns = CalculateDynamics(row.SliceColumnValues);
				}
			}

			private IList<string> CalculateDynamics(IList<decimal> sliceColumnValues)
			{
				var columnsCount = Slices.Count * 2;

				IList<string> output = CreateInitializedBy(columnsCount, "");

				for(var i = 0; i < columnsCount; i++)
				{
					if(i % 2 == 0)
					{
						output[i] = sliceColumnValues[i / 2].ToString(MeasurementUnitFormat);
					}
					else
					{
						if(i == 1)
						{
							output[i] = "-";
						}
						else
						{
							if(DynamicsIn == DynamicsInEnum.Percents)
							{
								output[i] = CalculatePercentDynamic(sliceColumnValues[i / 2 - 1], sliceColumnValues[i / 2]);
							}
							else
							{
								output[i] = (sliceColumnValues[i / 2] - sliceColumnValues[i / 2 - 1]).ToString(MeasurementUnitFormat);
							}
						}
					}
				}

				return output;
			}

			private TurnoverWithDynamicsReportRow AddGroupTotals(string title, IList<TurnoverWithDynamicsReportRow> nomenclatureGroupRows)
			{
				var row = new TurnoverWithDynamicsReportRow
				{
					Title = title,
					RowType = TurnoverWithDynamicsReportRow.RowTypes.Totals,
					SliceColumnValues = CreateInitializedBy(Slices.Count, 0m),
				};

				for(var i = 0; i < Slices.Count; i++)
				{
					var index = i;
					row.SliceColumnValues[index] = nomenclatureGroupRows
						.Select(ngr => ngr.SliceColumnValues)
						.Sum(ngvr => ngvr[index]);
				}

				if(ShowDynamics)
				{
					row.DynamicColumns = CalculateDynamics(row.SliceColumnValues);
				}

				if(ShowLastSale)
				{
					row.LastSaleDetails = new TurnoverWithDynamicsReportLastSaleDetails
					{
						DaysFromLastShipment = nomenclatureGroupRows.Sum(ngr => ngr.LastSaleDetails.DaysFromLastShipment),
						WarhouseResidue = nomenclatureGroupRows.Sum(nrg => nrg.LastSaleDetails.WarhouseResidue)
					};

					if(nomenclatureGroupRows.FirstOrDefault()?.RowType == TurnoverWithDynamicsReportRow.RowTypes.Values)
					{
						row.LastSaleDetails.LastSaleDate = nomenclatureGroupRows.Max(nrg => nrg.LastSaleDetails.LastSaleDate);
					}
				}

				return row;
			}

			private IList<decimal> CalculateValuesRow(IEnumerable<OrderItemNode> ordersItemsGroup)
			{
				IList<decimal> result = CreateInitializedBy(Slices.Count, 0m);

				for(var i = 0; i < Slices.Count; i++)
				{
					IDateTimeSlice slice = Slices[i];

					result[i] = ordersItemsGroup.Where(oi => oi.OrderDeliveryDate >= slice.StartDate)
						.Where(oi => oi.OrderDeliveryDate <= slice.EndDate)
						.Distinct()
						.Sum(MeasurementUnitSelector) ?? 0;
				}

				return result;
			}

			private static string CalculatePercentDynamic(decimal firstValue, decimal secondValue)
			{
				return firstValue != 0
					? ((secondValue - firstValue) / firstValue).ToString("P2")
					: "-";
			}

			private decimal? MeasurementUnitSelector(OrderItemNode oi)
			{
				if(MeasurementUnit == MeasurementUnitEnum.Amount)
				{
					return oi.ActualCount.HasValue ? oi.ActualCount : oi.Count;
				}
				else if(MeasurementUnit == MeasurementUnitEnum.Price)
				{
					return oi.ActualSum;
				}
				else
				{
					throw new InvalidOperationException($"Unknown {nameof(MeasurementUnit)} value {MeasurementUnit}");
				}
			}

			public static TurnoverWithDynamicsReport Create(
				DateTime startDate,
				DateTime endDate,
				string filters,
				IEnumerable<GroupingType> groupingBy,
				DateTimeSliceType slicingType,
				MeasurementUnitEnum measurementUnit,
				bool showDynamics,
				DynamicsInEnum dynamicsIn,
				bool showLastSale,
				bool showResidueForNomenclaturesWithoutSales,
				bool showContacts,
				Func<int, decimal> warehouseNomenclatureBalanceCallback,
				Func<TurnoverWithDynamicsReport, IList<OrderItemNode>> dataFetchCallback)
			{
				return new TurnoverWithDynamicsReport(
							startDate,
							endDate,
							filters,
							groupingBy,
							slicingType,
							measurementUnit,
							showDynamics,
							dynamicsIn,
							showLastSale,
							showResidueForNomenclaturesWithoutSales,
							showContacts,
							warehouseNomenclatureBalanceCallback,
							dataFetchCallback);
			}

			#region Excel render properties

			private uint _defaultCellFormatId;
			private uint _defaultBoldFontCellFormatId;
			private uint _tableHeadersCellFormatId;
			private uint _tableHeadersWithRotationCellFormatId;
			private uint _parametersHeadersCellFormatId;
			private uint _parametersValuesCellFormatId;
			private uint _sheetTitleCellFormatId;

			private int _variableDataColumnsCount =>
				ShowDynamics
				? DisplayRows.FirstOrDefault()?.DynamicColumns?.Count() ?? 0
				: DisplayRows.FirstOrDefault()?.SliceColumnValues?.Count() ?? 0;

			private bool _isShowPhone => ShowContacts;

			private bool _isShowEmail => ShowContacts && !ShowDynamics;

			#endregion Excel render properties

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
				sheetData.Append(GetUnitsValuesRow());
				sheetData.Append(GetGroupingValueRow());
				sheetData.Append(GetBlankRow());
				sheetData.Append(GetTableHeadersRow());
				sheetData.Append(GetTableTotalsRow());
				sheetData.Append(GetBlankRow());

				foreach(var node in Rows)
				{
					sheetData.Append(GetTableDataRow(node));
				}

				return sheetData;
			}

			private MergeCells GetMergeCells()
			{
				var mergeCells = new MergeCells();

				mergeCells.Append(new MergeCell() { Reference = new StringValue("A3:B3") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("A4:B4") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("A5:F5") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("J5:R5") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("A6:F6") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("G3:R3") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("G4:R4") });
				mergeCells.Append(new MergeCell() { Reference = new StringValue("A8:B8") });

				return mergeCells;
			}

			private Columns GetColumns()
			{
				var dataColumnsStartIndex = 3;

				if(_isShowPhone)
				{
					dataColumnsStartIndex = 4;
				}

				if(_isShowEmail)
				{
					dataColumnsStartIndex = 5;
				}

				var dataColumnsLastIndex = dataColumnsStartIndex + _variableDataColumnsCount - 1;

				var columns = new Columns();

				var rowIdColumn = CreateColumn(1, 6);
				var rowTitle = CreateColumn(2, 45);
				var rowPhones = CreateColumn(3, 20);
				var rowEmail = CreateColumn(4, 20);
				var rowData = CreateColumn(dataColumnsStartIndex, dataColumnsLastIndex, 7);
				var rowTotal = CreateColumn(dataColumnsLastIndex + 1, 10);
				var rowLastSaleDate = CreateColumn(dataColumnsLastIndex + 2, 12);
				var rowDaysFromLasrSale = CreateColumn(dataColumnsLastIndex + 3, 12);

				columns.Append(rowIdColumn);
				columns.Append(rowTitle);
				columns.Append(rowData);
				columns.Append(rowTotal);
				columns.Append(rowLastSaleDate);
				columns.Append(rowDaysFromLasrSale);

				if(_isShowPhone)
				{
					columns.Append(rowPhones);
				}

				if(_isShowEmail)
				{
					columns.Append(rowEmail);
				}

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

				row.AppendChild(GetSheetTitleStringCell(ReportTitle));

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

				row.AppendChild(GetParametersValuesStringCell($"Разрез: {SliceTypeString}"));
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(GetParametersValuesStringCell(Filters));

				return row;
			}

			private Row GetUnitsValuesRow()
			{
				var row = new Row();

				row.AppendChild(GetParametersValuesStringCell($"Единица измерения: {MeasurementUnitString}"));
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(new Cell());
				row.AppendChild(GetParametersValuesStringCell($"Дата и время формирования:  {CreatedAt:dd.MM.yyyy HH:mm}"));

				return row;
			}

			private Row GetGroupingValueRow()
			{
				var row = new Row();

				row.AppendChild(GetParametersValuesStringCell($"Группировка: {GroupingTitle}"));

				return row;
			}

			private Row GetTableHeadersRow()
			{
				var row = new Row();

				row.AppendChild(GetTableHeaderStringCell(GroupingTitle));
				row.AppendChild(GetTableHeaderStringCell(""));

				if(_isShowPhone)
				{
					row.AppendChild(GetTableHeaderStringCell("Телефоны"));
				}

				if(_isShowEmail)
				{
					row.AppendChild(GetTableHeaderStringCell("E-mail"));
				}

				foreach(var slice in Slices)
				{
					row.AppendChild(GetTableHeaderWithRotationStringCell(slice.StartDate.ToString("dd.MM.yyyy")));

					if(ShowDynamics)
					{
						row.AppendChild(GetTableHeaderWithRotationStringCell($"Измерение в {DynamicsInStringShort}"));
					}
				}

				row.AppendChild(GetTableHeaderStringCell("Всего за период"));

				if(ShowLastSale)
				{
					row.AppendChild(GetTableHeaderStringCell("Дата последней продажи"));
					row.AppendChild(GetTableHeaderStringCell("Дней с последней продажи"));
				}

				return row;
			}

			private Row GetTableTotalsRow()
			{
				var node = ReportTotal;

				var row = new Row();

				row.AppendChild(new Cell());
				row.AppendChild(GetStringCell(node.Title, true));

				if(_isShowPhone)
				{
					row.AppendChild(new Cell());
				}

				if(_isShowEmail)
				{
					row.AppendChild(new Cell());
				}

				if(ShowDynamics)
				{
					foreach(var value in node.DynamicColumns)
					{
						row.AppendChild(GetStringCell(value));
					}
				}
				else
				{
					foreach(var value in node.SliceColumnValues)
					{
						row.AppendChild(GetFloatingPointCell(value));
					}
				}

				row.AppendChild(GetFloatingPointCell(node.RowTotal));

				if(ShowLastSale)
				{
					row.AppendChild(GetStringCell(node.LastSaleDetails.LastSaleDate.ToString("dd.MM.yyyy")));
					row.AppendChild(GetNumericCell((int)node.LastSaleDetails.DaysFromLastShipment));
				}

				return row;
			}

			private Row GetTableDataRow(TurnoverWithDynamicsReportRow node)
			{
				var row = new Row();

				row.AppendChild(GetStringCell(node.Index, node.IsTotalsRow));
				row.AppendChild(GetStringCell(node.Title, node.IsTotalsRow));

				if(_isShowPhone)
				{
					row.AppendChild(GetStringCell(node.Phones, node.IsTotalsRow));
				}

				if(_isShowEmail)
				{
					row.AppendChild(GetStringCell(node.Emails, node.IsTotalsRow));
				}

				if(ShowDynamics)
				{
					foreach(var value in node.DynamicColumns)
					{
						row.AppendChild(GetStringCell(value, node.IsTotalsRow));
					}
				}
				else
				{
					foreach(var value in node.SliceColumnValues)
					{
						row.AppendChild(GetFloatingPointCell(value, node.IsTotalsRow));
					}
				}

				row.AppendChild(GetFloatingPointCell(node.RowTotal, node.IsTotalsRow));

				if(ShowLastSale)
				{
					row.AppendChild(GetStringCell(node.LastSaleDetails.LastSaleDate.ToString("dd.MM.yyyy"), node.IsTotalsRow));
					row.AppendChild(GetNumericCell((int)node.LastSaleDetails.DaysFromLastShipment, node.IsTotalsRow));
				}

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

				var parametersHeadersCellFormat = CreateCellFormat(defaultBoldFontId);

				var parametersValuesCellFormat = CreateCellFormat(defaultFontId, isWrapText: true);
				parametersValuesCellFormat.Alignment.Vertical = VerticalAlignmentValues.Top;

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

				stylesheet.CellFormats.AppendChild(tableHeadersWithRotationCellFormat);
				_tableHeadersWithRotationCellFormatId = 4;

				stylesheet.CellFormats.AppendChild(parametersHeadersCellFormat);
				_parametersHeadersCellFormatId = 5;

				stylesheet.CellFormats.AppendChild(parametersValuesCellFormat);
				_parametersValuesCellFormatId = 6;

				stylesheet.CellFormats.AppendChild(sheetTitleCellFormat);
				_sheetTitleCellFormatId = 7;

				stylesheet.CellFormats.Count = 8;

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

			private Cell GetFloatingPointCell(decimal value, bool isBold = false)
			{
				var cell = new Cell
				{
					CellValue = new CellValue(value),
					DataType = CellValues.Number,
					StyleIndex = isBold ? _defaultBoldFontCellFormatId : _defaultCellFormatId
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
