using DateTimeHelpers;
using Gamma.Utilities;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Globalization;
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
				DateTimeSliceType slicingType,
				MeasurementUnitEnum measurementUnit,
				bool showDynamics,
				DynamicsInEnum dynamicsIn,
				bool showLastSale,
				Func<int, decimal> warehouseNomenclatureBalanceCallback,
				Func<TurnoverWithDynamicsReport, IList<OrderItemNode>> dataFetchCallback)
			{
				StartDate = startDate;
				EndDate = endDate;
				Filters = filters;
				SliceType = slicingType;
				MeasurementUnit = measurementUnit;
				ShowDynamics = showDynamics;
				DynamicsIn = dynamicsIn;
				ShowLastSale = showLastSale;
				_warehouseNomenclatureBalanceCallback = warehouseNomenclatureBalanceCallback;
				Slices = DateTimeSliceFactory.CreateSlices(slicingType, startDate, endDate).ToList();
				CreatedAt = DateTime.Now;
				Rows = ProcessData(dataFetchCallback(this));
			}

			#region Parameters
			public DateTime StartDate { get; }

			public DateTime EndDate { get; }

			public string Filters { get; }

			public DateTimeSliceType SliceType { get; }

			public MeasurementUnitEnum MeasurementUnit { get; }

			public bool ShowDynamics { get; }

			public DynamicsInEnum DynamicsIn { get; }

			public bool ShowLastSale { get; }

			public DateTime CreatedAt { get; }
			#endregion

			/// <summary>
			/// Временные срезы отчета
			/// </summary>
			public IList<IDateTimeSlice> Slices { get; }

			public IList<TurnoverWithDynamicsReportRow> Rows { get; }

			public IList<TurnoverWithDynamicsReportRow> DisplayRows => new List<TurnoverWithDynamicsReportRow>
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

			public TurnoverWithDynamicsReportRow ReportTotal { get; private set; }

			public string SliceTypeString => SliceType.GetEnumTitle();

			public string MeasurementUnitString => MeasurementUnit.GetEnumTitle();

			public string DynamicsInStringShort => DynamicsIn == DynamicsInEnum.Percents ? "%" :
				MeasurementUnit == MeasurementUnitEnum.Price ? "Руб." : "Шт.";

			/// <summary>
			/// Зависит от текущего значения <see cref="MeasurementUnit"/>
			/// </summary>
			public string MeasurementUnitFormat => MeasurementUnit == MeasurementUnitEnum.Amount ? "# ##0" : "# ##0.00";

			private IList<TurnoverWithDynamicsReportRow> ProcessData(IList<OrderItemNode> ordersItemslist)
			{
				IList<TurnoverWithDynamicsReportRow> rows = new List<TurnoverWithDynamicsReportRow>();

				var productGroups = ordersItemslist
					.GroupBy(oi => oi.NomenclatureId)
					.GroupBy(g => g.First().ProductGroupId);

				rows = ProcessGroups(productGroups);

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
					IList<TurnoverWithDynamicsReportRow> productGroupRows = new List<TurnoverWithDynamicsReportRow>();

					var productGroupTitle = productGroup.First().First().ProductGroupName;

					foreach(var nomenclatureGroup in productGroup)
					{
						var row = new TurnoverWithDynamicsReportRow
						{
							Title = nomenclatureGroup.First().NomenclatureOfficialName,
							RowType = TurnoverWithDynamicsReportRow.RowTypes.Values,
							SliceColumnValues = CreateInitializedBy(Slices.Count, 0m),
						};

						row.SliceColumnValues = CalculateNomenclatureValuesRow(nomenclatureGroup);

						if(ShowDynamics)
						{
							row.DynamicColumns = CalculateDynamics(row.SliceColumnValues);
						}

						row.Index = index.ToString();
						index++;

						if(ShowLastSale)
						{
							var lastDelivery = nomenclatureGroup
								.OrderBy(oi => oi.OrderDeliveryDate)
								.Last().OrderDeliveryDate.Value;

							row.LastSaleDetails = new TurnoverWithDynamicsReportLastSaleDetails
							{
								LastSaleDate = lastDelivery,
								DaysFromLastShipment = Math.Floor((CreatedAt - lastDelivery).TotalDays),
								WarhouseResidue = _warehouseNomenclatureBalanceCallback(nomenclatureGroup.Key)
							};
						}

						productGroupRows.Add(row);
					}

					var groupTotal = AddGroupTotals(productGroupTitle, productGroupRows);
					totalsRows.Add(groupTotal);
					productGroupRows.Insert(0, groupTotal);
					rows = rows.Union(productGroupRows).ToList();
				}

				ReportTotal = AddGroupTotals("Сводные данные по отчету", totalsRows);

				return rows;
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

			private IList<decimal> CalculateNomenclatureValuesRow(IGrouping<int, OrderItemNode> ordersItemsGroup)
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
				DateTimeSliceType slicingType,
				MeasurementUnitEnum measurementUnit,
				bool showDynamics,
				DynamicsInEnum dynamicsIn,
				bool showLastSale,
				Func<int, decimal> warehouseNomenclatureBalanceCallback,
				Func<TurnoverWithDynamicsReport, IList<OrderItemNode>> dataFetchCallback)
			{
				return new TurnoverWithDynamicsReport(
							startDate,
							endDate,
							filters,
							slicingType,
							measurementUnit,
							showDynamics,
							dynamicsIn,
							showLastSale,
							warehouseNomenclatureBalanceCallback,
							dataFetchCallback);
			}

			public class TurnoverWithDynamicsReportRow
			{
				public string Title { get; set; }

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

				public DateTime? OrderDeliveryDate { get; set; }

				public int NomenclatureId { get; set; }

				public string NomenclatureOfficialName { get; set; }

				public int ProductGroupId { get; set; }

				public string ProductGroupName { get; set; }

				public decimal? ActualCount { get; set; }

				public decimal Count { get; set; }

				public decimal Price { get; set; }

				public decimal ActualSum { get; set; }
			}
		}
	}
}
