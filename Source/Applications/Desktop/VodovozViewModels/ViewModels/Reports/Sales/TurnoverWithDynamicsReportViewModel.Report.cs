using DateTimeHelpers;
using Gamma.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
	{
		public partial class TurnoverWithDynamicsReport
		{
			private readonly string _dynamicsInPercentageColumnTitle = "Изменение в %";
			private readonly string _dynamicsInMeasurementUnitColumnTitle = "Изменение в еденицах измерения";
			private readonly Func<Nomenclature, decimal> _warehouseNomenclatureBalanceCallback;

			private TurnoverWithDynamicsReport(
				DateTime startDate,
				DateTime endDate,
				DateTimeSliceType slicingType,
				MeasurementUnitEnum measurementUnit,
				bool showDynamics,
				DynamicsInEnum dynamicsIn,
				bool showLastSale,
				Func<Nomenclature, decimal> warehouseNomenclatureBalanceCallback,
				Func<TurnoverWithDynamicsReport, IList<OrderItem>> dataFetchCallback)
			{
				StartDate = startDate;
				EndDate = endDate;
				SliceType = slicingType;
				MeasurementUnit = measurementUnit;
				ShowDynamics = showDynamics;
				DynamicsIn = dynamicsIn;
				ShowLastSale = showLastSale;
				_warehouseNomenclatureBalanceCallback = warehouseNomenclatureBalanceCallback;
				Slices = DateTimeSliceFactory.CreateSlices(slicingType, startDate, endDate).ToList();
				var createDate = DateTime.Now;
				Rows = ProcessData(dataFetchCallback(this));
				CreatedAt = createDate;
			}

			#region Parameters
			public DateTime StartDate { get; }

			public DateTime EndDate { get; }

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

			public string SliceTypeString => SliceType.GetEnumTitle();

			public string MeasurementUnitString => MeasurementUnit.GetEnumTitle();

			/// <summary>
			/// Зависит от текущего значения <see cref="MeasurementUnit"/>
			/// </summary>
			private string MeasurementUnitFormat => MeasurementUnit == MeasurementUnitEnum.Amount ? "0" : "0.000";

			private IList<TurnoverWithDynamicsReportRow> ProcessData(IList<OrderItem> ordersItemslist)
			{
				IList<TurnoverWithDynamicsReportRow> rows = new List<TurnoverWithDynamicsReportRow>();

				var nomenclatureGroups = ordersItemslist
					.GroupBy(oi => oi.Nomenclature)
					.GroupBy(g => g.Key.ProductGroup);

				rows = ProcessGroups(nomenclatureGroups);

				return rows;
			}

			private IList<decimal> CreateInitializedBy(int length, decimal initializer = 0)
			{
				var result = new List<decimal>(length);

				for(var i = 0; i < length; i++)
				{
					result.Add(initializer);
				}

				return result;
			}

			private IList<TurnoverWithDynamicsReportRow> ProcessGroups(IEnumerable<IGrouping<ProductGroup, IGrouping<Nomenclature, OrderItem>>> productGroups)
			{
				IList<TurnoverWithDynamicsReportRow> rows = new List<TurnoverWithDynamicsReportRow>();

				IList<TurnoverWithDynamicsReportRow> totalsRows = new List<TurnoverWithDynamicsReportRow>();

				int index = 1;

				foreach(var productGroup in productGroups)
				{
					IList<TurnoverWithDynamicsReportRow> productGroupRows = new List<TurnoverWithDynamicsReportRow>();

					var productGroupTitle = GetProductGroupFullName(productGroup.Key);

					foreach(var nomenclatureGroup in productGroup)
					{
						var row = new TurnoverWithDynamicsReportRow
						{
							Title = nomenclatureGroup.Key.OfficialName,
							RowType = TurnoverWithDynamicsReportRow.RowTypes.Values,
							SliceColumnValues = CreateInitializedBy(Slices.Count),
						};

						row.SliceColumnValues = CalculateNomenclatureValuesRow(nomenclatureGroup);
						row.Index = index.ToString();
						index++;

						if(ShowLastSale)
						{
							var lastDelivery = nomenclatureGroup
								.OrderBy(oi => oi.Order.DeliveryDate)
								.Last().Order.DeliveryDate.Value;

							row.LastSaleDetails = new TurnoverWithDynamicsReportLastSaleDetails
							{
								LastSaleDate = lastDelivery,
								WarhouseResidue = _warehouseNomenclatureBalanceCallback(nomenclatureGroup.Key)
							};
						}

						productGroupRows.Add(row);
					}

					var groupTotal = AddGroupTotals(productGroupTitle, productGroupRows.Select(r => r.SliceColumnValues));
					totalsRows.Add(groupTotal);
					productGroupRows.Insert(0, groupTotal);
					rows = rows.Union(productGroupRows).ToList();
				}

				rows.Insert(0, AddGroupTotals("Сводные данные по отчету", totalsRows.Select(r => r.SliceColumnValues)));

				return rows;
			}

			private static string GetProductGroupFullName(ProductGroup productGroup)
			{
				if(productGroup == null)
				{
					return "Без группы";
				}

				if(productGroup.Parent == null)
				{
					return productGroup?.Name ?? "Без группы";
				}

				return GetProductGroupFullName(productGroup.Parent) + " / " + productGroup.Name;
			}

			private TurnoverWithDynamicsReportRow AddGroupTotals(string title, IEnumerable<IList<decimal>> nomenclatureGroupValuesRows)
			{
				var row = new TurnoverWithDynamicsReportRow
				{
					Title = title,
					RowType = TurnoverWithDynamicsReportRow.RowTypes.Totals,
					SliceColumnValues = CreateInitializedBy(Slices.Count),
				};

				for(var i = 0; i < Slices.Count; i++)
				{
					var index = i;
					row.SliceColumnValues[index] = nomenclatureGroupValuesRows.Sum(
						ngvr => ngvr[index]);
				}

				row.LastSaleDetails = new TurnoverWithDynamicsReportLastSaleDetails();

				return row;
			}

			private IList<decimal> CalculateNomenclatureValuesRow(IGrouping<Nomenclature, OrderItem> ordersItemsGroup)
			{
				IList<decimal> result = CreateInitializedBy(Slices.Count);

				Nomenclature nomenclature = ordersItemsGroup.Key;

				for(var i = 0; i < Slices.Count; i++)
				{
					IDateTimeSlice slice = Slices[i];

					result[i] = ordersItemsGroup.Where(oi => oi.Order.DeliveryDate >= slice.StartDate)
						.Where(oi => oi.Order.DeliveryDate <= slice.EndDate)
						.Sum(MeasurementUnitSelector) ?? 0;
				}

				return result;
			}

			//private string CalculateCellValue(IGrouping<Nomenclature, OrderItem> ordersItemsGroup, int i)
			//{
			//	var slice = Slices.First(sl => sl.ToString() == Columns[i]);

			//	var value = ordersItemsGroup.Where(oi => oi.Order.DeliveryDate >= slice.StartDate)
			//		.Where(oi => oi.Order.DeliveryDate <= slice.EndDate)
			//		.Sum(MeasurementUnitSelector);

			//	return value?.ToString(MeasurementUnitFormat) ?? "0";
			//}

			//private string CalculateDynamicsValue(List<string> columnsValues, int i)
			//{
			//	string dynamicValue;
			//	if(i > HeadColumnsCount + 2)
			//	{
			//		var firstValue = decimal.Parse(columnsValues[i - 3]);
			//		var secondValue = decimal.Parse(columnsValues[i - 1]);

			//		if(DynamicsIn == DynamicsInEnum.Percents)
			//		{
			//			dynamicValue = CalculatePercentDynamic(firstValue, secondValue);
			//		}
			//		else
			//		{
			//			dynamicValue = (secondValue - firstValue).ToString();
			//		}
			//	}
			//	else
			//	{
			//		dynamicValue = "-";
			//	}

			//	return dynamicValue;
			//}

			private static string CalculatePercentDynamic(decimal firstValue, decimal secondValue)
			{
				return secondValue != 0
					? ((secondValue - firstValue) / secondValue).ToString("P2")
					: (firstValue == 0) ? "0,00%" : "100,00%";
			}

			private decimal? MeasurementUnitSelector(OrderItem oi)
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
				DateTimeSliceType slicingType,
				MeasurementUnitEnum measurementUnit,
				bool showDynamics,
				DynamicsInEnum dynamicsIn,
				bool showLastSale,
				Func<Nomenclature, decimal> warehouseNomenclatureBalanceCallback,
				Func<TurnoverWithDynamicsReport, IList<OrderItem>> dataFetchCallback)
			{
				return new TurnoverWithDynamicsReport(
							startDate,
							endDate,
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

				public IList<decimal> SliceColumnValues { get; set; }

				public IList<decimal> SliceColumnValuesWithDynamics { get; set; }

				public decimal RowTotal => SliceColumnValues.Sum();

				public TurnoverWithDynamicsReportLastSaleDetails LastSaleDetails { get; set; }

				public enum RowTypes
				{
					Values,
					Totals
				}
			}

			public class TurnoverWithDynamicsReportLastSaleDetails
			{
				public DateTime LastSaleDate { get; set; }

				public double DaysFromLastShipment => (DateTime.Now - LastSaleDate).TotalDays;

				public decimal WarhouseResidue { get; set; }
			}
		}
	}
}
