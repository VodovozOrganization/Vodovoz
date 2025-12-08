using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DateTimeHelpers;
using Gamma.Utilities;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Reports.Editing.Modifiers;

namespace Vodovoz.ViewModels.ViewModels.Reports.WageCalculation.CallCenterMotivation
{
		public partial class CallCenterMotivationReport
		{
			private readonly DateTime _startDate;
			private readonly DateTime _endDate;
			private readonly DateTimeSliceType _sliceType;
			private CallCenterMotivationReportRow _reportTotal;

			private string SliceTypeString => _sliceType.GetEnumTitle();
			
			private CallCenterMotivationReport(
				DateTime startDate,
				DateTime endDate,
				string filters,
				IEnumerable<GroupingType> groupingBy,
				DateTimeSliceType dateTimeSlicingType,
				bool showDynamics,
				Func<CallCenterMotivationReport, IList<CallCenterMotivationReportOrderItemNode>> dataFetchCallback,
				CancellationToken cancellationToken)
			{
				_startDate = startDate;
				_endDate = endDate;
				Filters = filters;
				GroupingBy = groupingBy;
				_sliceType = dateTimeSlicingType;
				ShowDynamics = showDynamics;
				Slices = DateTimeSliceFactory.CreateSlices(dateTimeSlicingType, startDate, endDate).ToList();
				CreatedAt = DateTime.Now;
				Rows = ProcessData(dataFetchCallback(this), cancellationToken);
				DisplayRows = ProcessTreeViewDisplay();
			}

			private IList<CallCenterMotivationReportRow> ProcessTreeViewDisplay()
			{
				return new List<CallCenterMotivationReportRow>
				{
					_reportTotal,
					new CallCenterMotivationReportRow
					{
						Title = GroupingTitle,
						RowType = ReportRowType.Subheader,
						SliceColumnValues = Enumerable.Repeat(new ValueColumn(), Slices.Count).ToList(),
						DynamicColumns = Enumerable.Repeat(new ValueColumn(), Slices.Count - 1).ToList()
					}
				}.Union(Rows).ToList();
			}
			
			private string GroupingTitle => string.Join(" | ", GroupingBy.Select(x => x.GetEnumTitle()));
			
			private string ReportTitle => $"Отчет по оборачиваемости с {_startDate:dd.MM.yyyy} по {_endDate:dd.MM.yyyy}";
			
			public IEnumerable<GroupingType> GroupingBy { get; }
			
			public bool ShowDynamics { get; }

			public DateTime CreatedAt { get; }
			
			public IList<IDateTimeSlice> Slices { get; }
			
			public IList<CallCenterMotivationReportRow> DisplayRows { get; }
			
			public string Filters { get; }
			
			public  IList<CallCenterMotivationReportRow> Rows {get;}

			private IList<CallCenterMotivationReportRow> ProcessData(
				IList<CallCenterMotivationReportOrderItemNode> ordersItemsList,
				CancellationToken cancellationToken)
			{
				var groupingCount = GroupingBy.Count();

				switch(groupingCount)
				{
					case 3:
						var result3 = Process3rdLevelGroups(ordersItemsList, cancellationToken);

						var group3Total = AddGroupTotals("Сводные данные по отчету", result3.Totals);

						_reportTotal = group3Total;

						ProcessIndexes(result3.Rows);

						return result3.Rows;
					case 2:
						var result2nd = Process2ndLevelGroups(ordersItemsList, cancellationToken);

						var group2Total = AddGroupTotals("Сводные данные по отчету", result2nd.Totals);

						_reportTotal = group2Total;

						ProcessIndexes(result2nd.Rows);

						return result2nd.Rows;
					default:
						var result = Process1stLevelGroups(ordersItemsList, cancellationToken);

						result.TotalRow.Title = "Сводные данные по отчету";

						_reportTotal = result.TotalRow;

						ProcessIndexes(result.Rows);

						return result.Rows;
				}
			}

			private void ProcessIndexes(IList<CallCenterMotivationReportRow> Rows)
			{
				var index = 1;

				foreach(var item in Rows)
				{
					if(item.RowType == ReportRowType.Values)
					{
						item.Index = index.ToString();
						index++;
					}
				}
			}

			private (IList<CallCenterMotivationReportRow> Rows, CallCenterMotivationReportRow TotalRow) Process1stLevelGroups(
				IEnumerable<CallCenterMotivationReportOrderItemNode> firstLevelGroup,
				CancellationToken cancellationToken)
			{
				var result = new List<CallCenterMotivationReportRow>();

				var firstSelector = GetSelector(GroupingBy.Last());

				var groupedNodes = (
						from oi in firstLevelGroup
						group oi by firstSelector.Invoke(oi)
						into g
						select new { Key = g.Key, Items = g.ToList() })
					.OrderBy(g => g.Key)
					.ToList();

				foreach(var group in groupedNodes)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var groupTitle = GetGroupTitle(GroupingBy.Last()).Invoke(group.Items.First());

					var row = new CallCenterMotivationReportRow
					{
						RowType = ReportRowType.Values,
						Title = groupTitle,
						MotivationUnitType = group.Items.First().MotivationUnitType,
						SliceColumnValues = CalculateValuesRow(group.Items)
					};

					ProcessDynamics(row);

					result.Add(row);
				}

				var groupTotal = AddGroupTotals("", result);

				return (result, groupTotal);
			}

			private (IList<CallCenterMotivationReportRow> Rows, IList<CallCenterMotivationReportRow> Totals) Process2ndLevelGroups(
				IEnumerable<CallCenterMotivationReportOrderItemNode> secondLevelGroup,
				CancellationToken cancellationToken)
			{
				var result = new List<CallCenterMotivationReportRow>();

				var totalsRows = new List<CallCenterMotivationReportRow>();

				var firstGroupSelector = GroupingBy.Count() - 2;
				var secondGroupSelector = GroupingBy.Count() - 1;

				var firstSelector = GetSelector(GroupingBy.ElementAt(firstGroupSelector));
				var secondSelector = GetSelector(GroupingBy.ElementAt(secondGroupSelector));

				var groupedNodes = (
						from oi in secondLevelGroup
						group oi by new { Key1 = firstSelector.Invoke(oi), Key2 = secondSelector.Invoke(oi) }
						into g
						select new { Key = g.Key, Items = g.ToList() })
					.OrderBy(g => g.Key.Key1)
					.ThenBy(g => g.Key.Key2)
					.ToList();

				var groupsCount = groupedNodes.Count;

				for(var i = 0; i < groupsCount;)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var groupTitle = GetGroupTitle(GroupingBy.ElementAt(firstGroupSelector)).Invoke(groupedNodes[i].Items.First());

					var currentFirstKeyValue = groupedNodes[i].Key.Key1;

					var groupRows = new List<CallCenterMotivationReportRow>();

					while(true)
					{
						if(i == groupsCount || !groupedNodes[i].Key.Key1.Equals(currentFirstKeyValue))
						{
							break;
						}

						var row = CreateCallCenterMotivationReportRow(
							groupedNodes[i].Items,
							GetGroupTitle(GroupingBy.Last()).Invoke(groupedNodes[i].Items.First()));

						groupRows.Add(row);

						i++;
					}

					var groupTotal = AddGroupTotals("", groupRows);
					groupTotal.Title = groupTitle;
					totalsRows.Add(groupTotal);

					result.Add(groupTotal);
					result.AddRange(groupRows);
				}

				return (result, totalsRows);
			}

			private (IList<CallCenterMotivationReportRow> Rows, IList<CallCenterMotivationReportRow> Totals) Process3rdLevelGroups(
				IEnumerable<CallCenterMotivationReportOrderItemNode> secondLevelGroup,
				CancellationToken cancellationToken)
			{
				var result = new List<CallCenterMotivationReportRow>();
				var totalsRows = new List<CallCenterMotivationReportRow>();

				var firstLevelGroupSelector = GroupingBy.Count() - 3;
				var secondLevelGroupSelector = GroupingBy.Count() - 2;
				var thirdLevelGroupSelector = GroupingBy.Count() - 1;

				var firstSelector = GetSelector(GroupingBy.ElementAt(firstLevelGroupSelector));
				var secondSelector = GetSelector(GroupingBy.ElementAt(secondLevelGroupSelector));
				var thirdSelector = GetSelector(GroupingBy.ElementAt(thirdLevelGroupSelector));

				var groupedNodes = (
						from oi in secondLevelGroup
						group oi by new { Key1 = firstSelector.Invoke(oi), Key2 = secondSelector.Invoke(oi), Key3 = thirdSelector.Invoke(oi) }
						into g
						select new { Key = g.Key, Items = g.ToList() })
					.OrderBy(g => g.Key.Key1)
					.ThenBy(g => g.Key.Key2)
					.ThenBy(g => g.Key.Key3)
					.ToList();

				var groupsCount = groupedNodes.Count;

				for(var i = 0; i < groupsCount;)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var groupTitle = GetGroupTitle(GroupingBy.ElementAt(firstLevelGroupSelector))
						.Invoke(groupedNodes[i]
							.Items
							.First());

					var currentFirstKeyValue = groupedNodes[i].Key.Key1;

					var groupRows = new List<CallCenterMotivationReportRow>();
					var secondLevelGroupTotals =
						new List<CallCenterMotivationReportRow>();

					while(true)
					{
						if(i == groupsCount || !groupedNodes[i].Key.Key1.Equals(currentFirstKeyValue))
						{
							break;
						}

						var currentSecondKeyValue = groupedNodes[i].Key.Key2;

						var secondLevelTitle = GetGroupTitle(GroupingBy.ElementAt(secondLevelGroupSelector))
							.Invoke(groupedNodes[i]
								.Items
								.First());

						var secondLevelGroupRows =
							new List<CallCenterMotivationReportRow>();

						while(true)
						{
							if(i == groupsCount
							   || !groupedNodes[i].Key.Key1.Equals(currentFirstKeyValue)
							   || !groupedNodes[i].Key.Key2.Equals(currentSecondKeyValue))
							{
								break;
							}

							var row = CreateCallCenterMotivationReportRow(
								groupedNodes[i].Items,
								GetGroupTitle(GroupingBy.Last()).Invoke(groupedNodes[i].Items.First()));

							secondLevelGroupRows.Add(row);

							i++;
						}

						var secondLevelGroupTotal = AddGroupTotals("", secondLevelGroupRows);
						secondLevelGroupTotal.Title = secondLevelTitle;
						secondLevelGroupTotals.Add(secondLevelGroupTotal);

						groupRows.Add(secondLevelGroupTotal);
						groupRows.AddRange(secondLevelGroupRows);
					}

					var groupTotal = AddGroupTotals("", secondLevelGroupTotals);
					groupTotal.Title = groupTitle;
					totalsRows.Add(groupTotal);

					result.Add(groupTotal);
					result.AddRange(groupRows);
				}

				return (result, totalsRows);
			}

			private CallCenterMotivationReportRow CreateCallCenterMotivationReportRow(
				List<CallCenterMotivationReportOrderItemNode> items,
				string title)
			{
				var row = new CallCenterMotivationReportRow
				{
					RowType = ReportRowType.Values,
					Title = title,
					MotivationUnitType = items.First().MotivationUnitType,
					SliceColumnValues = CalculateValuesRow(items)
				};

				ProcessDynamics(row);

				return row;
			}

			private Func<CallCenterMotivationReportOrderItemNode, object> GetSelector(GroupingType groupingType)
			{
				switch(groupingType)
				{
					case GroupingType.Nomenclature:
						return x => x.NomenclatureId;
					case GroupingType.NomenclatureGroup:
						return x => x.ProductGroupId;
					case GroupingType.OrderAuthor:
						return x => x.OrderAuthorId;
					default:
						return x => x.Id;
				}
			}

			private Func<CallCenterMotivationReportOrderItemNode, string> GetGroupTitle(GroupingType groupingType)
			{
				switch(groupingType)
				{
					case GroupingType.Nomenclature:
						return x => x.NomenclatureOfficialName;
					case GroupingType.NomenclatureGroup:
						return x => x.ProductGroupName;
					case GroupingType.OrderAuthor:
						return x => x.OrderAuthorName;
					default:
						return x => x.Id.ToString();
				}
			}

			private void ProcessDynamics(CallCenterMotivationReportRow row)
			{
				if(ShowDynamics)
				{
					row.DynamicColumns = CalculateDynamics(row.SliceColumnValues);
				}
			}

			private IList<ValueColumn> CalculateDynamics(IList<ValueColumn> sliceColumnValues)
			{
				var dynamics = sliceColumnValues.Zip(sliceColumnValues.Skip(1), (current, next) => new ValueColumn
				{
					Sold = next.Sold - current.Sold,
					Premium = next.Premium - current.Premium
				}).ToList();

				return dynamics;
			}

			private CallCenterMotivationReportRow AddGroupTotals(string title, IList<CallCenterMotivationReportRow> nomenclatureGroupRows)
			{
				var row = new CallCenterMotivationReportRow
				{
					Title = title,
					RowType = ReportRowType.Totals,
				};

				for(var i = 0; i < Slices.Count; i++)
				{
					row.SliceColumnValues.Add(new ValueColumn
					{
						Sold = nomenclatureGroupRows.Sum(x => x.SliceColumnValues[i].Sold),
						Premium = nomenclatureGroupRows.Sum(x => x.SliceColumnValues[i].Premium)
					});
				}

				if(ShowDynamics)
				{
					row.DynamicColumns = CalculateDynamics(row.SliceColumnValues);
				}

				return row;
			}

			private IList<ValueColumn> CalculateValuesRow(IEnumerable<CallCenterMotivationReportOrderItemNode> ordersItemsGroup)
			{
				var result = new List<ValueColumn>();

				foreach(var slice in Slices)
				{
					var node = ordersItemsGroup
						.Where(oi => oi.OrderDeliveryDate >= slice.StartDate)
						.Where(oi => oi.OrderDeliveryDate <= slice.EndDate)
						.Distinct();

					result.Add(new ValueColumn
					{
						Sold = node.Sum(MeasurementUnitSelector) ?? 0,
						Premium = node.Sum(MeasurementCoefficientSelector) ?? 0
					});
				}

				return result;
			}
			
			private decimal? MeasurementUnitSelector(CallCenterMotivationReportOrderItemNode oi)
			{
				if(oi.MotivationUnitType == NomenclatureMotivationUnitType.Item)
				{
					return oi.ActualCount ?? oi.Count;
				}

				if(oi.MotivationUnitType == NomenclatureMotivationUnitType.Percent)
				{
					return oi.ActualSum;
				}
				
				throw new InvalidOperationException($"Unknown {nameof(NomenclatureMotivationUnitType)} value {oi.MotivationUnitType}");
			}

			private decimal? MeasurementCoefficientSelector(CallCenterMotivationReportOrderItemNode oi)
			{
				if(oi.MotivationUnitType == NomenclatureMotivationUnitType.Item)
				{
					return (oi.ActualCount ?? oi.Count) * oi.MotivationCoefficient ?? 0;
				}

				if(oi.MotivationUnitType == NomenclatureMotivationUnitType.Percent)
				{
					return oi.ActualSum / 100 * (oi.MotivationCoefficient ?? 0);
				}
				
				throw new InvalidOperationException($"Unknown {nameof(NomenclatureMotivationUnitType)} value {oi.MotivationUnitType}");
			}

			public static CallCenterMotivationReport Create(
				DateTime startDate,
				DateTime endDate,
				string filters,
				IEnumerable<GroupingType> groupingBy,
				DateTimeSliceType slicingType,
				bool showDynamics,
				Func<CallCenterMotivationReport, IList<CallCenterMotivationReportOrderItemNode>>
					dataFetchCallback,
				CancellationToken cancellationToken)
			{
				return new CallCenterMotivationReport(
					startDate,
					endDate,
					filters,
					groupingBy,
					slicingType,
					showDynamics,
					dataFetchCallback,
					cancellationToken);
			}
		} 
}
