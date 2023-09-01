using DateTimeHelpers;
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
				if(GroupingBy.LastOrDefault() == GroupingType.Nomenclature)
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
							LastSaleDetails = new TurnoverWithDynamicsReportLastSaleDetails()
						}
					}.Union(Rows).ToList();
				}
				else
				{
					return Rows;
				}

				throw new InvalidOperationException($"Unsupported value {GroupingBy} of {nameof(GroupingBy)}");
			}

			#region Parameters
			public DateTime StartDate { get; }

			public DateTime EndDate { get; }

			public string Filters { get; }

			public IEnumerable<GroupingType> GroupingBy { get; }

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
				var groupingCount = GroupingBy.Count();

				switch(groupingCount)
				{
					case 3:
						var thirdLevelGroup = ordersItemslist
							.GroupBy(GetSelector(GroupingBy.ElementAt(2)))
							.GroupBy(g => GetSelector(GroupingBy.ElementAt(1)).Invoke(g.First()))
							.GroupBy(g => GetSelector(GroupingBy.ElementAt(0)).Invoke(g.First().First()));

						return Process3rdLevelGroups(
							thirdLevelGroup,
							GetGroupTitle(GroupingBy.ElementAt(2)),
							GetGroupTitle(GroupingBy.ElementAt(1)),
							GetGroupTitle(GroupingBy.ElementAt(0)));
					case 2:
						var secondLevelGroup = ordersItemslist
							.GroupBy(GetSelector(GroupingBy.ElementAt(1)))
							.GroupBy(g => GetSelector(GroupingBy.ElementAt(0)).Invoke(g.First()));

						return Process2ndLevelGroups(secondLevelGroup,
							GetGroupTitle(GroupingBy.ElementAt(1)),
							GetGroupTitle(GroupingBy.ElementAt(0)));
					default:
						var firstLevelGroup = ordersItemslist
							.GroupBy(GetSelector(GroupingBy.ElementAt(0)));
						
						var result = 
						 Process1stLevelGroups(firstLevelGroup,
							GetGroupTitle(GroupingBy.ElementAt(0)));

						ReportTotal = result.First();

						return result;
				}

				//if(GroupingBy.LastOrDefault() == GroupingType.Nomenclature)
				//{
				//	var productGroups = ordersItemslist
				//		.GroupBy(oi => oi.NomenclatureId)
				//		.GroupBy(g => g.First().ProductGroupId);

				//	rows = ProcessGroups(productGroups);
				//}
				//else if(GroupingBy.LastOrDefault() == GroupingType.Counterparty)
				//{
				//	var counterpartyGroups = ordersItemslist
				//		.GroupBy(oi => oi.CounterpartyId);

				//	int index = 1;

				//	rows = ProcessCounterpartyGroups(ref index, counterpartyGroups);
				//}

				//return rows;
			}

			private IList<TurnoverWithDynamicsReportRow> Process1stLevelGroups(
				IEnumerable<IGrouping<object, OrderItemNode>> firstLevelGroup,
				Func<OrderItemNode, string> func)
			{
				var result = new List<TurnoverWithDynamicsReportRow>();

				IList<TurnoverWithDynamicsReportRow> totalsRows = new List<TurnoverWithDynamicsReportRow>();

				var groupTitle = func.Invoke(firstLevelGroup.First().First());

				foreach(var group in firstLevelGroup)
				{
					result.Add(NodeGroup(group, x => x.NomenclatureOfficialName));
				}

				var groupTotal = AddGroupTotals(groupTitle, result);

				result.Insert(0, groupTotal);

				return result;
			}

			private IList<TurnoverWithDynamicsReportRow> Process2ndLevelGroups(
				IEnumerable<IGrouping<object, IGrouping<object, OrderItemNode>>> secondLevelGroup,
				Func<OrderItemNode, string> func1,
				Func<OrderItemNode, string> func2)
			{
				var result = new List<TurnoverWithDynamicsReportRow>();

				IList<TurnoverWithDynamicsReportRow> totalsRows = new List<TurnoverWithDynamicsReportRow>();

				foreach(var group in secondLevelGroup)
				{
					var groupTitle = func2.Invoke(group.First().First());

					var groupRows = Process1stLevelGroups(group, func1);

					var groupTotal = AddGroupTotals(groupTitle, groupRows);

					totalsRows.Add(groupTotal);
					groupRows.Insert(0, groupTotal);
					result = result.Union(groupRows).ToList();
				}

				ReportTotal = AddGroupTotals("Сводные данные по отчету", totalsRows);

				return result;
			}

			private IList<TurnoverWithDynamicsReportRow> Process3rdLevelGroups(
				IEnumerable<IGrouping<object, IGrouping<object, IGrouping<object, OrderItemNode>>>> thirdLevelGroup,
				Func<OrderItemNode, string> func1,
				Func<OrderItemNode, string> func2,
				Func<OrderItemNode, string> func3)
			{
				var result = new List<TurnoverWithDynamicsReportRow>();

				IList<TurnoverWithDynamicsReportRow> totalsRows = new List<TurnoverWithDynamicsReportRow>();

				foreach(var group in thirdLevelGroup)
				{
					var groupTitle = func3.Invoke(group.First().First().First());

					var groupRows = Process2ndLevelGroups(group, func2, func1);

					var groupTotal = AddGroupTotals(groupTitle, groupRows);

					totalsRows.Add(groupTotal);
					groupRows.Insert(0, groupTotal);
					result = result.Union(groupRows).ToList();
				}

				ReportTotal = AddGroupTotals("Сводные данные по отчету", totalsRows);

				return result;
			}

			private TurnoverWithDynamicsReportRow NodeGroup(IGrouping<object, OrderItemNode> group, Func<OrderItemNode, string> titleFunc)
			{
				var row = new TurnoverWithDynamicsReportRow
				{
					RowType = TurnoverWithDynamicsReportRow.RowTypes.Values,
					Title = titleFunc.Invoke(group.First()),
				};

				row.SliceColumnValues = CalculateValuesRow(group);

				ProcessDynamics(row);
				ProcessLastSale(group, row);

				return row;
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
					case GroupingType.NomenclatureGroup1:
						return x => x.ProductGroupId;
					case GroupingType.NomenclatureGroup2:
						return x => x.ProductGroupId;
					case GroupingType.NomenclatureGroup3:
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
						return x => x.OrderDeliveryDate?.ToString("yyyy-MM-dd");
					case GroupingType.RouteList:
						return x => x.RouteListId.ToString();
					case GroupingType.Nomenclature:
						return x => x.NomenclatureOfficialName;
					case GroupingType.NomenclatureType:
						return x => x.NomenclatureCategory.GetEnumTitle();
					case GroupingType.NomenclatureGroup1:
						return x => x.ProductGroupId.ToString();
					case GroupingType.NomenclatureGroup2:
						return x => x.ProductGroupId.ToString();
					case GroupingType.NomenclatureGroup3:
						return x => x.ProductGroupId.ToString();
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

			private IList<TurnoverWithDynamicsReportRow> ProcessGroups(IEnumerable<IGrouping<object, IGrouping<object, OrderItemNode>>> groups)
			{
				IList<TurnoverWithDynamicsReportRow> rows = new List<TurnoverWithDynamicsReportRow>();

				IList<TurnoverWithDynamicsReportRow> totalsRows = new List<TurnoverWithDynamicsReportRow>();

				int index = 1;

				foreach(var group in groups)
				{
					var groupTitle = group.First().First().GroupName;

					var groupRows = ProcessProductGroup(ref index, group);

					var groupTotal = AddGroupTotals(groupTitle, groupRows);

					totalsRows.Add(groupTotal);
					groupRows.Insert(0, groupTotal);
					rows = rows.Union(groupRows).ToList();
				}

				ReportTotal = AddGroupTotals("Сводные данные по отчету", totalsRows);

				return rows;
			}

			private IList<TurnoverWithDynamicsReportRow> ProcessProductGroup(ref int index, IGrouping<object, IGrouping<object, OrderItemNode>> productGroup)
			{
				IList<TurnoverWithDynamicsReportRow> productGroupRows = ProcessSubGroups(ref index, productGroup);

				return productGroupRows;
			}

			private IList<TurnoverWithDynamicsReportRow> ProcessSubGroups(ref int index, IGrouping<object, IGrouping<object, OrderItemNode>> productGroup)
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

			private TurnoverWithDynamicsReportRow ProcessNomenclatureGroup(IGrouping<object, OrderItemNode> nomenclatureGroup)
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

			private IList<TurnoverWithDynamicsReportRow> ProcessCounterpartyGroups(ref int index, IEnumerable<IGrouping<object, OrderItemNode>> counterpartyGroups)
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

			private TurnoverWithDynamicsReportRow ProcessCounterpartyGroup(IGrouping<object, OrderItemNode> counterpartyGroup)
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

			private string ProcessCounterpartyPhones(IGrouping<object, OrderItemNode> counterpartyGroup)
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

			private void ProcessLastSale(IGrouping<object, OrderItemNode> nomenclatureGroup, TurnoverWithDynamicsReportRow row)
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
							? _warehouseNomenclatureBalanceCallback((int)nomenclatureGroup.Key)
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

			private IList<decimal> CalculateValuesRow(IGrouping<object, OrderItemNode> ordersItemsGroup)
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
		}
	}
}
