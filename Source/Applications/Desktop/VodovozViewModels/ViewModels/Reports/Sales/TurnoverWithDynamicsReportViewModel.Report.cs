using DateTimeHelpers;
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
				Slices = DateTimeSliceFactory.CreateSlices(slicingType, startDate, endDate);
				DynamicColumns = CreateDynamicColumnsHeaders();
				var columns = new List<string> { "#", "Периоды продаж" };
				columns.AddRange(DynamicColumns);
				columns.Add("Всего за период");
				if(ShowLastSale)
				{
					var createDate = DateTime.Now;
					columns.Add("Дата последней продажи");
					columns.Add("Кол-во дней с момента\n" +
						" последней отгрузки");
					columns.Add($"Остатки по всем складам\n" +
						$" на {createDate:dd.MM.yyyy HH:mm:ss}");
				}
				Columns = columns;
				Rows = ProcessData(dataFetchCallback(this));
				var header2 = MakeEmptyList(Columns.Count + 1);
				header2[0] = "№";
				header2[1] = "Номенклатура";
				Rows.Insert(1, header2);
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
			public IEnumerable<IDateTimeSlice> Slices { get; }

			public IList<string> Columns { get; }

			public IList<string> DynamicColumns { get; }

			public IList<IList<string>> Rows { get; }

			public string SlicingType => Enum.GetName(typeof(DateTimeSliceType), SliceType);

			/// <summary>
			/// Зависит от текущего значения <see cref="MeasurementUnit"/>
			/// </summary>
			private string MeasurementUnitFormat => MeasurementUnit == MeasurementUnitEnum.Amount ? "0" : "0.000";

			/// <summary>
			/// Расчитывается из:
			/// <list type="bullet">
			///		<item>Номер строки</item>
			///		<item>Колонка названий номерклатур</item>
			/// </list>
			/// </summary>
			private int HeadColumnsCount => 2;

			/// <summary>
			/// Расчитывается из:
			/// <list type="bullet">
			/// <item>Колонка суммы
			/// <list type="number">
			///		<item>Всего за период</item>
			/// </list>
			/// </item>
			/// <item>ShowLastSale - добалят 3 колонки:
			/// <list type="number">
			///		<item>Дата последней продажи</item>
			///		<item>Кол-во дней с момента последней отгрузки</item>
			///		<item>Остатки по всем складам на {Дата формирования отчета}</item>
			///	</list>
			/// </item>
			/// </list>
			/// </summary>
			private int TailColumnsCount => 1 + (ShowLastSale ? 3 : 0);

			private List<string> CreateDynamicColumnsHeaders()
			{
				var slices = Slices.Select(s => s.ToString()).ToList();
				var columns = new List<string>();

				if(ShowDynamics)
				{
					foreach(var slice in slices)
					{
						columns.Add(slice);

						if(DynamicsIn == DynamicsInEnum.Percents)
						{
							columns.Add(_dynamicsInPercentageColumnTitle);
						}
						else if(DynamicsIn == DynamicsInEnum.MeasurementUnit)
						{
							columns.Add(_dynamicsInMeasurementUnitColumnTitle);
						}
						else
						{
							throw new InvalidOperationException($"Unsupported value {DynamicsIn} in {nameof(DynamicsIn)}");
						}
					}

					return columns;
				}

				return slices;
			}

			private IList<IList<string>> ProcessData(IList<OrderItem> ordersItemslist)
			{
				IList<IList<string>> rows = new List<IList<string>>();

				var nomenclatureGroups = ordersItemslist
					.GroupBy(oi => oi.Nomenclature)
					.GroupBy(g => g.Key.ProductGroup);

				rows = ProcessGroups(nomenclatureGroups);

				return rows;
			}

			private IList<IList<string>> ProcessGroups(IEnumerable<IGrouping<ProductGroup, IGrouping<Nomenclature, OrderItem>>> productGroups)
			{
				IList<IList<string>> rows = new List<IList<string>>();

				IList<IList<string>> totalsRows = new List<IList<string>>();

				int index = 1;

				foreach(var productGroup in productGroups)
				{
					string productGroupTitle = GetProductGroupFullName(productGroup.Key);

					IList<IList<string>> productGroupRows = new List<IList<string>>();

					foreach(var nomenclatureGroup in productGroup)
					{
						productGroupRows.Add(CalculateNomenclatureRow(nomenclatureGroup, ref index));
					}

					var groupTotal = AddGroupTotals(productGroupTitle, productGroupRows);
					productGroupRows.Insert(0, groupTotal);
					totalsRows.Add(groupTotal);

					rows = rows.Union(productGroupRows).ToList();
				}

				rows.Insert(0, AddGroupTotals("Сводные данные по отчету", totalsRows));

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

				return GetProductGroupFullName(productGroup.Parent) + " \\ " + productGroup.Name;
			}

			private IList<string> AddGroupTotals(string title, IList<IList<string>> nomenclatureGroupRows)
			{
				var totals = MakeEmptyList(Columns.Count + 1);

				totals[0] = "Group";
				totals[1] = title;

				for(int i = HeadColumnsCount; i < Columns.Count; i++)
				{
					if(ShowLastSale && i == Columns.Count - TailColumnsCount + 1)
					{
						totals[i] = string.Empty;
						continue;
					}
					totals[i] = ColumnTotal(nomenclatureGroupRows, i);
				}

				return totals;
			}

			private IList<string> CalculateNomenclatureRow(IGrouping<Nomenclature, OrderItem> ordersItemsGroup, ref int index)
			{
				List<string> columnsValues = MakeEmptyList(Columns.Count + 1);
				var nomenclature = ordersItemsGroup.Key;
				columnsValues[0] = index.ToString();
				columnsValues[1] = nomenclature.OfficialName;

				for(var i = HeadColumnsCount; i < Columns.Count - TailColumnsCount; i++)
				{
					if(ShowDynamics)
					{
						if((i - HeadColumnsCount) % 2 == 0)
						{
							columnsValues[i] = CalculateCellValue(ordersItemsGroup, i);
						}
						else
						{
							columnsValues[i] = CalculateDynamicsValue(columnsValues, i);
						}
					}
					else
					{
						columnsValues[i] = CalculateCellValue(ordersItemsGroup, i);
					}
				}

				columnsValues[Columns.Count - TailColumnsCount] = CalculateRowTotal(columnsValues).ToString(MeasurementUnitFormat);

				if(ShowLastSale)
				{
					var lastDelivery = ordersItemsGroup
						.OrderBy(oi => oi.Order.DeliveryDate)
						.Last().Order.DeliveryDate.Value;

					columnsValues[Columns.Count - TailColumnsCount + 1] = lastDelivery.ToString("dd.MM.yyyy");
					columnsValues[Columns.Count - TailColumnsCount + 2] = (DateTime.Now - lastDelivery).TotalDays.ToString("0");
					columnsValues[Columns.Count - TailColumnsCount + 3] = _warehouseNomenclatureBalanceCallback(nomenclature).ToString("0.000");
				}

				index++;

				return columnsValues;
			}

			private string CalculateCellValue(IGrouping<Nomenclature, OrderItem> ordersItemsGroup, int i)
			{
				var slice = Slices.First(sl => sl.ToString() == Columns[i]);

				var value = ordersItemsGroup.Where(oi => oi.Order.DeliveryDate >= slice.StartDate)
					.Where(oi => oi.Order.DeliveryDate <= slice.EndDate)
					.Sum(MeasurementUnitSelector);

				return value?.ToString(MeasurementUnitFormat) ?? "0";
			}

			private List<string> MakeEmptyList(int capacity)
			{
				var columnsValues = new List<string>(capacity);
				for(var i = 1; i < capacity; i++)
				{
					columnsValues.Add(string.Empty);
				}

				return columnsValues;
			}

			private string CalculateDynamicsValue(List<string> columnsValues, int i)
			{
				string dynamicValue;
				if(i > HeadColumnsCount + 2)
				{
					var firstValue = decimal.Parse(columnsValues[i - 3]);
					var secondValue = decimal.Parse(columnsValues[i - 1]);

					if(DynamicsIn == DynamicsInEnum.Percents)
					{
						dynamicValue = CalculatePercentDynamic(firstValue, secondValue);
					}
					else
					{
						dynamicValue = (secondValue - firstValue).ToString();
					}
				}
				else
				{
					dynamicValue = "-";
				}

				return dynamicValue;
			}

			private static string CalculatePercentDynamic(decimal firstValue, decimal secondValue)
			{
				return secondValue != 0
					? ((secondValue - firstValue) / secondValue).ToString("P2")
					: (firstValue == 0) ? "0,00%" : "100,00%";
			}

			private decimal CalculateRowTotal(List<string> columnsValues)
			{
				decimal nomenclatureTotal = 0;

				var evenCheckerNumber = HeadColumnsCount % 2;

				for(int i = HeadColumnsCount; i < Columns.Count; i++)
				{
					if(ShowLastSale
						&& i == (Columns.Count - TailColumnsCount))
					{
						continue;
					}

					if(ShowDynamics && i % 2 == evenCheckerNumber)
					{
						continue;
					}

					decimal.TryParse(columnsValues[i], out decimal result);
					nomenclatureTotal += result;
				}

				return nomenclatureTotal;
			}

			private string ColumnTotal(IList<IList<string>> rows, int index)
			{
				string totalValue = string.Empty;

				var evenCheckerNumber = HeadColumnsCount % 2 + 1;

				if(ShowDynamics)
				{
					if(index == HeadColumnsCount + 1)
					{
						return "-";
					}
					if(index > HeadColumnsCount && index % 2 == evenCheckerNumber)
					{
						if(DynamicsIn == DynamicsInEnum.Percents)
						{
							decimal sum = rows.Sum(items =>
							{
								if(decimal.TryParse(items[index].TrimEnd('%'), out decimal result))
								{
									return result;
								}
								return 0;
							}) / rows.Count;

							totalValue = sum.ToString("0.00") + "%";

							return totalValue;
						}
						else if(DynamicsIn == DynamicsInEnum.MeasurementUnit)
						{
							decimal sum = rows.Sum(items =>
							{
								if(decimal.TryParse(items[index], out decimal result))
								{
									return result;
								}
								return 0;
							});

							totalValue = sum.ToString("0.00");

							return totalValue;
						}
					}
				}
				
				return rows.Sum(items =>
				{
					if(decimal.TryParse(items[index], out decimal result))
					{
						return result;
					}
					return 0;
				}).ToString();
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
		}
	}
}
