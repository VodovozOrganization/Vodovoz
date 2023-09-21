using DateTimeHelpers;
using Gamma.Utilities;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Reports.Editing.Modifiers;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
	{
		public partial class TurnoverWithDynamicsReport
		{
			private readonly Func<List<int>, List<NomenclatureStockNode>> _warehouseNomenclatureBalanceCallback;
			private List<NomenclatureStockNode> _nomenclatureStockNodes;

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
				Func<List<int>, List<NomenclatureStockNode>> warehouseNomenclatureBalanceCallback,
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
				var nomenclatureIds = GroupingBy.LastOrDefault() == GroupingType.Nomenclature
					? ordersItemslist.Select(r => r.NomenclatureId).Distinct().ToList()
					: new List<int>();

				_nomenclatureStockNodes = _warehouseNomenclatureBalanceCallback.Invoke(nomenclatureIds);

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

				var groupedNodes = from oi in firstLevelGroup
							  group oi by firstSelector.Invoke(oi) into g
							  select new { Key = g.Key, Items = g.ToList() };

				foreach(var group in groupedNodes)
				{
					var groupTitle = GetGroupTitle(GroupingBy.Last()).Invoke(group.Items.First());

					string phones = string.Empty;
					string emails = string.Empty;

					if(ShowContacts)
					{
						phones = ProcessCounterpartyPhones(group.Items.First());
						emails = group.Items.First().CounterpartyEmails;
					}

					var row = new TurnoverWithDynamicsReportRow
					{
						RowType = TurnoverWithDynamicsReportRow.RowTypes.Values,
						Title = groupTitle.ToString(),
						Phones = phones,
						Emails = emails
					};

					row.SliceColumnValues = CalculateValuesRow(group.Items);

					ProcessDynamics(row);
					ProcessLastSale(group.Items, row);

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

				var groupedNodes1 = from oi in secondLevelGroup
								   group oi by firstSelector.Invoke(oi) into g
								   select new { Key = g.Key, Items = g.ToList() };

				var groupedNodes = groupedNodes1.ToList();

				foreach(var group in groupedNodes)
				{
					var groupTitle = GetGroupTitle(GroupingBy.ElementAt(preLast)).Invoke(group.Items.First());

					var groupRows = Process1stLevelGroups(group.Items);

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

				var groupedNodes = from oi in thirdLevelGroup
								   group oi by firstSelector.Invoke(oi) into g
								   select new { Key = g.Key, Items = g.ToList() };

				foreach(var group in groupedNodes)
				{
					var groupTitle = GetGroupTitle(GroupingBy.ElementAt(prePreLast)).Invoke(group.Items.First());

					var groupRows = Process2ndLevelGroups(group.Items);

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
					.OrderContactPhone?
					.Where(ocp => !counterpartyPhones.Contains(ocp))
					.Distinct();

				string resultedOrderContactPhones = string.Empty;

				if(ordersContactPhones != null && ordersContactPhones.Any())
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
							? _nomenclatureStockNodes
								.Where(n => n.NomenclatureId == nomenclatureGroup.First().NomenclatureId)
								.Select(n => n.Stock).FirstOrDefault()
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
				Func<List<int>, List<NomenclatureStockNode>> warehouseNomenclatureBalanceCallback,
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
		}
	}
}
