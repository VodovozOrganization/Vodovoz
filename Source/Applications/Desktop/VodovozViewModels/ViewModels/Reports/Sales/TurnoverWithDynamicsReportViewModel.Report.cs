using DateTimeHelpers;
using Gamma.Utilities;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

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
				GroupingByEnum groupingBy,
				DateTimeSliceType slicingType,
				MeasurementUnitEnum measurementUnit,
				bool showDynamics,
				DynamicsInEnum dynamicsIn,
				bool showLastSale,
				bool showResidueForNomenclaturesWithoutSales,
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
				_warehouseNomenclatureBalanceCallback = warehouseNomenclatureBalanceCallback;
				Slices = DateTimeSliceFactory.CreateSlices(slicingType, startDate, endDate).ToList();
				CreatedAt = DateTime.Now;
				Rows = ProcessData(dataFetchCallback(this));
				DisplayRows = ProcessTreeViewDisplay();
			}

			private IList<TurnoverWithDynamicsReportRow> ProcessTreeViewDisplay()
			{
				if(GroupingBy == GroupingByEnum.Nomenclature)
				{
					return new List<TurnoverWithDynamicsReportRow>
					{
						ReportTotal,
						new TurnoverWithDynamicsReportRow
						{
							Title = "Номенклатура",
							RowType = TurnoverWithDynamicsReportRow.RowTypes.Subheader,
							SliceColumnValues = CreateInitializedBy(Slices.Count, 0m),
							DynamicColumns = CreateInitializedBy(ShowDynamics ? Slices.Count * 2 : Slices.Count, ""),
							LastSaleDetails = new TurnoverWithDynamicsReportLastSaleDetails
							{

							}
						}
					}.Union(Rows).ToList();
				}
				if(GroupingBy == GroupingByEnum.Counterparty || GroupingBy == GroupingByEnum.CounterpartyShowContacts)
				{
					return Rows;
				}
				throw new InvalidOperationException($"Unsupported value {GroupingBy} of {nameof(GroupingBy)}");
			}

			#region Parameters
			public DateTime StartDate { get; }

			public DateTime EndDate { get; }

			public string Filters { get; }

			public GroupingByEnum GroupingBy { get; }

			public DateTimeSliceType SliceType { get; }

			public MeasurementUnitEnum MeasurementUnit { get; }

			public bool ShowDynamics { get; }

			public DynamicsInEnum DynamicsIn { get; }

			public bool ShowLastSale { get; }

			public bool ShowResidueForNomenclaturesWithoutSales { get; }

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
				IList<TurnoverWithDynamicsReportRow> rows = new List<TurnoverWithDynamicsReportRow>();

				if(GroupingBy == GroupingByEnum.Nomenclature)
				{
					var productGroups = ordersItemslist
						.GroupBy(oi => oi.NomenclatureId)
						.GroupBy(g => g.First().ProductGroupId);

					rows = ProcessGroups(productGroups);
				}
				else if(GroupingBy == GroupingByEnum.Counterparty || GroupingBy == GroupingByEnum.CounterpartyShowContacts)
				{
					var counterpartyGroups = ordersItemslist
						.GroupBy(oi => oi.CounterpartyId);

					int index = 1;

					rows = ProcessCounterpartyGroups(ref index, counterpartyGroups);
				}

				return rows;
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

			private IList<TurnoverWithDynamicsReportRow> ProcessGroups(IEnumerable<IGrouping<int, IGrouping<int, OrderItemNode>>> productGroups)
			{
				IList<TurnoverWithDynamicsReportRow> rows = new List<TurnoverWithDynamicsReportRow>();

				IList<TurnoverWithDynamicsReportRow> totalsRows = new List<TurnoverWithDynamicsReportRow>();

				int index = 1;

				foreach(var productGroup in productGroups)
				{
					var productGroupTitle = productGroup.First().First().ProductGroupName;

					var productGroupRows = ProcessProductGroup(ref index, productGroup);

					var groupTotal = AddGroupTotals(productGroupTitle, productGroupRows);

					totalsRows.Add(groupTotal);
					productGroupRows.Insert(0, groupTotal);
					rows = rows.Union(productGroupRows).ToList();
				}

				ReportTotal = AddGroupTotals("Сводные данные по отчету", totalsRows);

				return rows;
			}

			private IList<TurnoverWithDynamicsReportRow> ProcessProductGroup(ref int index, IGrouping<int, IGrouping<int, OrderItemNode>> productGroup)
			{
				IList<TurnoverWithDynamicsReportRow> productGroupRows = ProcessSubGroups(ref index, productGroup);

				return productGroupRows;
			}

			private IList<TurnoverWithDynamicsReportRow> ProcessSubGroups(ref int index, IGrouping<int, IGrouping<int, OrderItemNode>> productGroup)
			{
				var result = new List<TurnoverWithDynamicsReportRow>();

				foreach(var nomenclatureGroup in productGroup)
				{
					TurnoverWithDynamicsReportRow row = ProcessNomenclatureGroup(nomenclatureGroup);

					if(ShowResidueForNomenclaturesWithoutSales
						&& row.LastSaleDetails.WarhouseResidue == 0
						&& row.RowTotal == 0)
					{
						continue;
					}

					row.Index = index.ToString();
					index++;

					result.Add(row);
				}

				return result;
			}

			private TurnoverWithDynamicsReportRow ProcessNomenclatureGroup(IGrouping<int, OrderItemNode> nomenclatureGroup)
			{
				var row = new TurnoverWithDynamicsReportRow
				{
					Title = nomenclatureGroup.First().NomenclatureOfficialName,
					RowType = TurnoverWithDynamicsReportRow.RowTypes.Values,
					SliceColumnValues = CreateInitializedBy(Slices.Count, 0m),
				};

				row.SliceColumnValues = CalculateValuesRow(nomenclatureGroup);

				ProcessDynamics(row);
				ProcessLastSale(nomenclatureGroup, row);
				return row;
			}

			private IList<TurnoverWithDynamicsReportRow> ProcessCounterpartyGroups(ref int index, IEnumerable<IGrouping<int, OrderItemNode>> counterpartyGroups)
			{
				var result = new List<TurnoverWithDynamicsReportRow>();

				foreach(var counterpartyGroup in counterpartyGroups)
				{
					TurnoverWithDynamicsReportRow row = ProcessCounterpartyGroup(counterpartyGroup);

					row.Index = index.ToString();
					index++;

					result.Add(row);
				}

				ReportTotal = AddGroupTotals("Сводные данные по отчету", result);

				return result;
			}

			private TurnoverWithDynamicsReportRow ProcessCounterpartyGroup(IGrouping<int, OrderItemNode> counterpartyGroup)
			{
				var row = new TurnoverWithDynamicsReportRow
				{
					Title = counterpartyGroup.First().CounterpartyFullName,
					Phones = ProcessCounterpartyPhones(counterpartyGroup),
					Emails = counterpartyGroup.First().CounterpartyEmails,
					RowType = TurnoverWithDynamicsReportRow.RowTypes.Values,
					SliceColumnValues = CreateInitializedBy(Slices.Count, 0m),
				};

				row.SliceColumnValues = CalculateValuesRow(counterpartyGroup);

				ProcessDynamics(row);
				ProcessLastSale(counterpartyGroup, row);
				return row;
			}

			private string ProcessCounterpartyPhones(IGrouping<int, OrderItemNode> counterpartyGroup)
			{
				var result = string.Empty;

				var counterpartyPhones = counterpartyGroup.First().CounterpartyPhones;

				if(string.IsNullOrWhiteSpace(counterpartyPhones))
				{
					counterpartyPhones = string.Empty;
				}

				var ordersContactPhones = counterpartyGroup
					.Select(cp => cp.OrderContactPhone)
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

			private void ProcessLastSale(IGrouping<int, OrderItemNode> nomenclatureGroup, TurnoverWithDynamicsReportRow row)
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
						WarhouseResidue = GroupingBy == GroupingByEnum.Nomenclature
							? _warehouseNomenclatureBalanceCallback(nomenclatureGroup.Key)
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

			private IList<decimal> CalculateValuesRow(IGrouping<int, OrderItemNode> ordersItemsGroup)
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
				GroupingByEnum groupingBy,
				DateTimeSliceType slicingType,
				MeasurementUnitEnum measurementUnit,
				bool showDynamics,
				DynamicsInEnum dynamicsIn,
				bool showLastSale,
				bool showResidueForNomenclaturesWithoutSales,
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
							warehouseNomenclatureBalanceCallback,
							dataFetchCallback);
			}

			public class TurnoverWithDynamicsReportRow
			{
				public string Title { get; set; }

				public string Phones { get; set; } = string.Empty;
				public string Emails { get; set; } = string.Empty;

				public string Index { get; set; } = string.Empty;

				public RowTypes RowType { get; set; }

				public bool IsTotalsRow => RowType == RowTypes.Totals;

				public bool IsSubheaderRow => RowType == RowTypes.Subheader;

				public IList<decimal> SliceColumnValues { get; set; }

				public IList<string> DynamicColumns { get; set; }

				public decimal RowTotal => SliceColumnValues.Sum();

				public TurnoverWithDynamicsReportLastSaleDetails LastSaleDetails { get; set; }

				public enum RowTypes
				{
					Values,
					Totals,
					Subheader
				}
			}

			public class TurnoverWithDynamicsReportLastSaleDetails
			{
				public DateTime LastSaleDate { get; set; }

				public double DaysFromLastShipment { get; set; }

				public decimal WarhouseResidue { get; set; }
			}

			public class OrderItemNode
			{
				public int Id { get; set; }

				public int OrderId { get; set; }

				public int CounterpartyId { get; set; }

				public string CounterpartyPhones { get; set; }

				public string CounterpartyEmails { get; set; }

				public string CounterpartyFullName { get; set; }

				public DateTime? OrderDeliveryDate { get; set; }

				public int NomenclatureId { get; set; }

				public string NomenclatureOfficialName { get; set; }

				public int ProductGroupId { get; set; }

				public string ProductGroupName { get; set; }

				public decimal? ActualCount { get; set; }

				public decimal Count { get; set; }

				public decimal Price { get; set; }

				public decimal ActualSum { get; set; }

				public string OrderContactPhone { get; set; }
			}
		}
	}
}
