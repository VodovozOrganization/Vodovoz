using ClosedXML.Report.Utils;
using DynamicData;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Extensions;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Reports.Editing.Modifiers;
using static Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters.IncludeExcludeBookkeepingReportsFilterFactory;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl
{
	public partial class EdoControlReport
	{
		private IEnumerable<GroupingType> _groupingTypes = new List<GroupingType>();

		private readonly IEnumerable<OrderStatus> _orderStatuses = new List<OrderStatus>
		{
			OrderStatus.Shipped,
			OrderStatus.UnloadingOnStock,
			OrderStatus.Closed
		};

		public int _closingDocumentDeliveryScheduleId;

		private IEnumerable<CounterpartyType> _includedCounterpartyTypes = new List<CounterpartyType>();
		private IEnumerable<CounterpartyType> _excludedCounterpartyTypes = new List<CounterpartyType>();
		private IEnumerable<int> _includedCounterpartySubtypes = new List<int>();
		private IEnumerable<int> _excludedCounterpartySubtypes = new List<int>();

		private IEnumerable<int> _includedCounterparties = new List<int>();
		private IEnumerable<int> _excludedCounterparties = new List<int>();

		private IEnumerable<PersonType> _includedPersonTypes = new List<PersonType>();
		private IEnumerable<PersonType> _excludedPersonTypes = new List<PersonType>();

		private IEnumerable<PaymentType> _includedPaymentTypes = new List<PaymentType>();
		private IEnumerable<PaymentType> _excludedPaymentTypes = new List<PaymentType>();
		private IEnumerable<int> _includedPaymentFroms = new List<int>();
		private IEnumerable<int> _excludedPaymentFroms = new List<int>();
		private IEnumerable<PaymentByTerminalSource> _includedPaymentByTerminalSources = new List<PaymentByTerminalSource>();
		private IEnumerable<PaymentByTerminalSource> _excludedPaymentByTerminalSources = new List<PaymentByTerminalSource>();

		private IEnumerable<EdoDocFlowStatus> _includedEdoDocFlowStatuses = new List<EdoDocFlowStatus>();
		private IEnumerable<EdoDocFlowStatus> _excludedEdoDocFlowStatuses = new List<EdoDocFlowStatus>();

		private IEnumerable<EdoControlReportOrderDeliveryType> _includedOrderDeliveryTypes = new List<EdoControlReportOrderDeliveryType>();
		private IEnumerable<EdoControlReportOrderDeliveryType> _excludedOrderDeliveryTypes = new List<EdoControlReportOrderDeliveryType>();

		private IEnumerable<EdoControlReportAddressTransferType> _includedAddressTransferTypes = new List<EdoControlReportAddressTransferType>();
		private IEnumerable<EdoControlReportAddressTransferType> _excludedAddressTransferTypes = new List<EdoControlReportAddressTransferType>();

		#region Restrictions

		private void SetRequestRestrictions(
			IncludeExludeFiltersViewModel filterViewModel,
			int closingDocumentDeliveryScheduleId)
		{
			_closingDocumentDeliveryScheduleId = closingDocumentDeliveryScheduleId;

			#region CounterpartyType

			var includedCounterpartyTypeElements = filterViewModel.GetIncludedElements<CounterpartyType>();
			var excludedCounterpartyTypeElements = filterViewModel.GetExcludedElements<CounterpartyType>();

			_includedCounterpartyTypes = includedCounterpartyTypeElements
				.Where(x => x is IncludeExcludeElement<CounterpartyType, CounterpartyType>)
				.Select(x => (x as IncludeExcludeElement<CounterpartyType, CounterpartyType>).Id)
				.ToArray();

			_excludedCounterpartyTypes = excludedCounterpartyTypeElements
				.Where(x => x is IncludeExcludeElement<CounterpartyType, CounterpartyType>)
				.Select(x => (x as IncludeExcludeElement<CounterpartyType, CounterpartyType>).Id)
				.ToArray();

			_includedCounterpartySubtypes = includedCounterpartyTypeElements
				.Where(x => x is IncludeExcludeElement<int, CounterpartySubtype>)
				.Select(x => (x as IncludeExcludeElement<int, CounterpartySubtype>).Id)
				.ToArray();

			_excludedCounterpartySubtypes = excludedCounterpartyTypeElements
				.Where(x => x is IncludeExcludeElement<int, CounterpartySubtype>)
				.Select(x => (x as IncludeExcludeElement<int, CounterpartySubtype>).Id)
				.ToArray();

			#endregion CounterpartyType

			#region Counterparty

			var counterpartiesFilter = filterViewModel.GetFilter<IncludeExcludeEntityFilter<Counterparty>>();
			_includedCounterparties = counterpartiesFilter.GetIncluded().ToArray();
			_excludedCounterparties = counterpartiesFilter.GetExcluded().ToArray();

			#endregion Counterparty

			#region PersonType

			var personTypesFilter = filterViewModel.GetFilter<IncludeExcludeEnumFilter<PersonType>>();
			_includedPersonTypes = personTypesFilter.GetIncluded().ToArray();
			_excludedPersonTypes = personTypesFilter.GetExcluded().ToArray();

			#endregion

			#region PaymentTypes

			var includedPaymentTypeElements = filterViewModel.GetIncludedElements<PaymentType>();
			var excludedPaymentTypeElements = filterViewModel.GetExcludedElements<PaymentType>();

			_includedPaymentTypes = includedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<PaymentType, PaymentType>)
				.Select(x => (x as IncludeExcludeElement<PaymentType, PaymentType>).Id)
				.ToArray();

			_excludedPaymentTypes = excludedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<PaymentType, PaymentType>)
				.Select(x => (x as IncludeExcludeElement<PaymentType, PaymentType>).Id)
				.ToArray();

			_includedPaymentFroms = includedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<int, PaymentFrom>)
				.Select(x => (x as IncludeExcludeElement<int, PaymentFrom>).Id)
				.ToArray();

			_excludedPaymentFroms = excludedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<int, PaymentFrom>)
				.Select(x => (x as IncludeExcludeElement<int, PaymentFrom>).Id)
				.ToArray();

			_includedPaymentByTerminalSources = includedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>)
				.Select(x => (x as IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>).Id)
				.ToArray();

			_excludedPaymentByTerminalSources = excludedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>)
				.Select(x => (x as IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>).Id)
				.ToArray();

			#endregion PaymentTypes

			#region EdoDocFlowStatus

			var edoDocFlowStatusFilter = filterViewModel.GetFilter<IncludeExcludeEnumFilter<EdoDocFlowStatus>>();
			_includedEdoDocFlowStatuses = edoDocFlowStatusFilter.GetIncluded().ToArray();
			_excludedEdoDocFlowStatuses = edoDocFlowStatusFilter.GetExcluded().ToArray();

			#endregion EdoDocFlowStatus

			#region EdoControlReportOrderDeliveryType

			var edoControlReportOrderDeliveryTypeFilter = filterViewModel.GetFilter<IncludeExcludeEnumFilter<EdoControlReportOrderDeliveryType>>();
			_includedOrderDeliveryTypes = edoControlReportOrderDeliveryTypeFilter.GetIncluded().ToArray();
			_excludedOrderDeliveryTypes = edoControlReportOrderDeliveryTypeFilter.GetExcluded().ToArray();

			#endregion EdoDocFlowStatus

			#region EdoControlReportAddressTransferType

			var edoControlReportAddressTransferTypeFilter = filterViewModel.GetFilter<IncludeExcludeEnumFilter<EdoControlReportAddressTransferType>>();
			_includedAddressTransferTypes = edoControlReportAddressTransferTypeFilter.GetIncluded().ToArray();
			_excludedAddressTransferTypes = edoControlReportAddressTransferTypeFilter.GetExcluded().ToArray();

			#endregion EdoControlReportAddressTransferType
		}

		#endregion Restrictions

		#region Grouping

		private IList<EdoControlReportRow> Process1stLevelGroups(
			IEnumerable<EdoControlReportData> dataNodes,
			CancellationToken cancellationToken)
		{
			if(_groupingTypes.Count() != 1)
			{
				throw new InvalidOperationException("Количество выбранных группировок должно быть 1");
			}

			var result = new List<EdoControlReportData>();

			var firstGroupingType = _groupingTypes.ElementAt(0);
			var firstSelector = GetSelector(firstGroupingType);

			var groupedRows =
				(from node in dataNodes
				 group node by firstSelector.Invoke(node) into g
				 select new { Key = g.Key, Items = g.ToList() })
				 .OrderBy(g => g.Key)
				 .ToList();

			var rows = new List<EdoControlReportRow>();

			var groupsSummaryInfo = $"Группировка по: {firstGroupingType.GetEnumDisplayName()}";
			rows.Add(new EdoControlReportRow(groupsSummaryInfo));

			foreach(var group in groupedRows)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var groupTitle = GetGroupTitle(firstGroupingType).Invoke(group.Items.FirstOrDefault());

				var rootRow = new EdoControlReportRow(groupTitle);

				rows.Add(rootRow);
				rows.AddRange(ConvertDataNodesToToReportRows(group.Items, cancellationToken));
			}

			return rows;
		}

		private IList<EdoControlReportRow> Process2ndLevelGroups(
			IEnumerable<EdoControlReportData> dataNodes,
			CancellationToken cancellationToken)
		{
			if(_groupingTypes.Count() != 2)
			{
				throw new InvalidOperationException("Количество выбранных группировок должно быть 2");
			}

			var result = new List<EdoControlReportRow>();

			var firstGroupingType = _groupingTypes.ElementAt(0);
			var secondGroupingType = _groupingTypes.ElementAt(1);

			var firstSelector = GetSelector(firstGroupingType);
			var secondSelector = GetSelector(secondGroupingType);

			var groupedNodes =
				(from node in dataNodes
				 group node by new { Key1 = firstSelector.Invoke(node), Key2 = secondSelector.Invoke(node) } into g
				 select new { Key = g.Key, Items = g.ToList() })
				 .OrderBy(g => g.Key.Key1)
				 .ThenBy(g => g.Key.Key2)
				 .ToList();

			var groupsCount = groupedNodes.Count;

			var groupsSummaryInfo =
				$"Группировка по: {firstGroupingType.GetEnumDisplayName()} | {secondGroupingType.GetEnumDisplayName()}";

			result.Add(new EdoControlReportRow(groupsSummaryInfo));

			for(var i = 0; i < groupsCount;)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var firstLevelGroupTitle = GetGroupTitle(firstGroupingType).Invoke(groupedNodes[i].Items.First());
				result.Add(new EdoControlReportRow(firstLevelGroupTitle));

				var lastSecondLevelGroupTitle = string.Empty;

				var currentFirstKeyValue = groupedNodes[i].Key?.Key1;

				while(true)
				{
					if(i >= groupsCount || !groupedNodes[i].Key.Key1.Equals(currentFirstKeyValue))
					{
						break;
					}

					var secondLevelGroupTitle = GetGroupTitle(secondGroupingType).Invoke(groupedNodes[i].Items.First());

					if(secondLevelGroupTitle != lastSecondLevelGroupTitle)
					{
						result.Add(new EdoControlReportRow(secondLevelGroupTitle));
					}

					var dataRows = ConvertDataNodesToToReportRows(groupedNodes[i].Items, cancellationToken);

					result.Add(dataRows);

					i++;

					lastSecondLevelGroupTitle = secondLevelGroupTitle;
				}
			}

			return result;
		}

		private IList<EdoControlReportRow> ConvertDataNodesToToReportRows(
			IEnumerable<EdoControlReportData> nodes,
			CancellationToken cancellationToken)
		{
			var rows = new List<EdoControlReportRow>();

			foreach(var node in nodes)
			{
				var dataRow = new EdoControlReportRow(node);

				rows.Add(dataRow);
			}

			return rows;
		}

		//private (IList<TurnoverWithDynamicsReportRow> Rows, IList<TurnoverWithDynamicsReportRow> Totals) Process3rdLevelGroups(
		//	IEnumerable<OrderItemNode> secondLevelGroup,
		//	CancellationToken cancellationToken)
		//{
		//	var result = new List<TurnoverWithDynamicsReportRow>();
		//	var totalsRows = new List<TurnoverWithDynamicsReportRow>();

		//	var firstLevelGroupSelector = GroupingBy.Count() - 3;
		//	var secondLevelGroupSelector = GroupingBy.Count() - 2;
		//	var thirdLevelGroupSelector = GroupingBy.Count() - 1;

		//	var firstSelector = GetSelector(GroupingBy.ElementAt(firstLevelGroupSelector));
		//	var secondSelector = GetSelector(GroupingBy.ElementAt(secondLevelGroupSelector));
		//	var thirdSelector = GetSelector(GroupingBy.ElementAt(thirdLevelGroupSelector));

		//	var groupedNodes = (from oi in secondLevelGroup
		//						group oi by new { Key1 = firstSelector.Invoke(oi), Key2 = secondSelector.Invoke(oi), Key3 = thirdSelector.Invoke(oi) } into g
		//						select new { Key = g.Key, Items = g.ToList() })
		//						.OrderBy(g => g.Key.Key1)
		//						.ThenBy(g => g.Key.Key2)
		//						.ThenBy(g => g.Key.Key3)
		//						.ToList();
		//	var groupsCount = groupedNodes.Count;

		//	for(var i = 0; i < groupsCount;)
		//	{
		//		cancellationToken.ThrowIfCancellationRequested();

		//		var groupTitle = GetGroupTitle(GroupingBy.ElementAt(firstLevelGroupSelector))
		//			.Invoke(groupedNodes[i]
		//			.Items
		//			.First());

		//		var currentFirstKeyValue = groupedNodes[i].Key.Key1;

		//		var groupRows = new List<TurnoverWithDynamicsReportRow>();
		//		var secondLevelGroupTotals = new List<TurnoverWithDynamicsReportRow>();

		//		while(true)
		//		{
		//			if(i == groupsCount || !groupedNodes[i].Key.Key1.Equals(currentFirstKeyValue))
		//			{
		//				break;
		//			}

		//			var currentSecondKeyValue = groupedNodes[i].Key.Key2;

		//			var secondLevelTitle = GetGroupTitle(GroupingBy.ElementAt(secondLevelGroupSelector))
		//				.Invoke(groupedNodes[i]
		//				.Items
		//				.First());

		//			var secondLevelGroupRows = new List<TurnoverWithDynamicsReportRow>();

		//			while(true)
		//			{
		//				if(i == groupsCount
		//					|| !groupedNodes[i].Key.Key1.Equals(currentFirstKeyValue)
		//					|| !groupedNodes[i].Key.Key2.Equals(currentSecondKeyValue))
		//				{
		//					break;
		//				}

		//				var row = CreateTurnoverWithDynamicsReportRow(
		//					groupedNodes[i].Items,
		//					GetGroupTitle(GroupingBy.Last()).Invoke(groupedNodes[i].Items.First()),
		//					ShowContacts);

		//				secondLevelGroupRows.Add(row);

		//				i++;
		//			}

		//			var secondLevelGroupTotal = AddGroupTotals("", secondLevelGroupRows);
		//			secondLevelGroupTotal.Title = secondLevelTitle;
		//			secondLevelGroupTotals.Add(secondLevelGroupTotal);

		//			groupRows.Add(secondLevelGroupTotal);
		//			groupRows.AddRange(secondLevelGroupRows);
		//		}

		//		var groupTotal = AddGroupTotals("", secondLevelGroupTotals);
		//		groupTotal.Title = groupTitle;
		//		totalsRows.Add(groupTotal);

		//		result.Add(groupTotal);
		//		result.AddRange(groupRows);
		//	}

		//	return (result, totalsRows);
		//}

		#endregion Grouping

		private void SetGroupingTypes(IEnumerable<GroupingType> groupingTypes)
		{
			_groupingTypes = groupingTypes;
		}

		private async Task SetReportRows(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			try
			{
				var dataNodes = await CreateReportRows(uow, cancellationToken);
				//Rows = Process1stLevelGroups(dataNodes, cancellationToken);
				Rows = Process2ndLevelGroups(dataNodes, cancellationToken);
			}
			catch(Exception ex)
			{
				throw;
			}
		}

		private async Task<IList<EdoControlReportData>> CreateReportRows(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var rows =
				from order in uow.Session.Query<Order>()
				join client in uow.Session.Query<Counterparty>() on order.Client.Id equals client.Id
				join rli in uow.Session.Query<RouteListItem>() on order.Id equals rli.Order.Id into routeListItems
				from routeListItem in routeListItems.DefaultIfEmpty()
				join ec in uow.Session.Query<EdoContainer>() on order.Id equals ec.Order.Id into edoContainers
				from edoContainer in edoContainers.DefaultIfEmpty()
				where
					order.DeliveryDate >= StartDate && order.DeliveryDate < EndDate.Date.AddDays(1)
					&& _orderStatuses.Contains(order.OrderStatus)

					&& (_includedCounterpartyTypes.Count() == 0 || _includedCounterpartyTypes.Contains(client.CounterpartyType))
					&& (_excludedCounterpartyTypes.Count() == 0 || !_excludedCounterpartyTypes.Contains(client.CounterpartyType))

					&& (_includedCounterpartySubtypes.Count() == 0
						|| _includedCounterpartySubtypes.Contains(client.CounterpartySubtype.Id))
					&& (client.CounterpartySubtype.Id == null
						|| _excludedCounterpartySubtypes.Count() == 0 || !_excludedCounterpartySubtypes.Contains(client.CounterpartySubtype.Id))

					&& (_includedCounterparties.Count() == 0 || _includedCounterparties.Contains(client.Id))
					&& (_excludedCounterparties.Count() == 0 || !_excludedCounterparties.Contains(client.Id))

					&& (_includedPersonTypes.Count() == 0 || _includedPersonTypes.Contains(client.PersonType))
					&& (_excludedPersonTypes.Count() == 0 || !_excludedPersonTypes.Contains(client.PersonType))

					&& (_includedPaymentTypes.Count() == 0 || _includedPaymentTypes.Contains(order.PaymentType))
					&& (_excludedPaymentTypes.Count() == 0 || !_excludedPaymentTypes.Contains(order.PaymentType))

					&& (_includedPaymentFroms.Count() == 0
						|| (order.PaymentType == PaymentType.PaidOnline && order.PaymentByCardFrom.Id != null && _includedPaymentFroms.Contains(order.PaymentByCardFrom.Id)))

					&& (_excludedPaymentFroms.Count() == 0
						|| !(order.PaymentType == PaymentType.PaidOnline && (order.PaymentByCardFrom.Id == null || _excludedPaymentFroms.Contains(order.PaymentByCardFrom.Id))))

					&& (_includedPaymentByTerminalSources.Count() == 0
						|| (order.PaymentType == PaymentType.Terminal && order.PaymentByTerminalSource != null && _includedPaymentByTerminalSources.Contains(order.PaymentByTerminalSource.Value)))

					&& (_excludedPaymentByTerminalSources.Count() == 0
						|| !(order.PaymentType == PaymentType.Terminal && order.PaymentByTerminalSource == null && _excludedPaymentByTerminalSources.Contains(order.PaymentByTerminalSource.Value)))

					&& (_includedEdoDocFlowStatuses.Count() == 0 || _includedEdoDocFlowStatuses.Contains(edoContainer.EdoDocFlowStatus))
					&& (edoContainer.EdoDocFlowStatus == null || _excludedEdoDocFlowStatuses.Count() == 0 || !_excludedEdoDocFlowStatuses.Contains(edoContainer.EdoDocFlowStatus))

					&& (_includedOrderDeliveryTypes.Count() == 0
						|| (_includedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.FastDelivery) && order.IsFastDelivery)
						|| (_includedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.SelfDelivery) && order.SelfDelivery)
						|| (_includedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.CloseDocument) && order.DeliverySchedule.Id == _closingDocumentDeliveryScheduleId)
						|| (_includedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.CommonDelivery) && order.DeliverySchedule != null && order.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId))
					&& (_excludedOrderDeliveryTypes.Count() == 0
						|| !((_excludedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.FastDelivery) && order.IsFastDelivery)
							|| (_excludedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.SelfDelivery) && order.SelfDelivery)
							|| (_excludedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.CloseDocument) && order.DeliverySchedule.Id == _closingDocumentDeliveryScheduleId)
							|| (_excludedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.CommonDelivery) && order.DeliverySchedule != null && order.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId)))

					&& (_includedAddressTransferTypes.Count() == 0
						|| (_includedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.NeedToReload) && routeListItem.AddressTransferType == AddressTransferType.NeedToReload)
						|| (_includedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.FromHandToHand) && routeListItem.AddressTransferType == AddressTransferType.FromHandToHand)
						|| (_includedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.FromFreeBalance) && routeListItem.AddressTransferType == AddressTransferType.FromFreeBalance)
						|| (_includedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.NoTransfer) && routeListItem.AddressTransferType == null))
					&& (_excludedAddressTransferTypes.Count() == 0
						|| !((_excludedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.NeedToReload) && routeListItem.AddressTransferType == AddressTransferType.NeedToReload)
							|| (_excludedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.FromHandToHand) && routeListItem.AddressTransferType == AddressTransferType.FromHandToHand)
							|| (_excludedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.FromFreeBalance) && routeListItem.AddressTransferType == AddressTransferType.FromFreeBalance)
							|| (_excludedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.NoTransfer) && routeListItem.AddressTransferType == null)))

				select new EdoControlReportData
				{
					EdoContainerId = edoContainer.Id,
					ClientId = client.Id,
					ClientName = client.Name,
					OrderId = order.Id,
					RouteListId = routeListItem.RouteList.Id,
					DeliveryDate = order.DeliveryDate.Value,
					EdoStatus = edoContainer.EdoDocFlowStatus,
					OrderDeliveryType =
						order.IsFastDelivery
						? EdoControlReportOrderDeliveryType.FastDelivery
						: order.SelfDelivery
							? EdoControlReportOrderDeliveryType.SelfDelivery
							: order.DeliverySchedule.Id == _closingDocumentDeliveryScheduleId
								? EdoControlReportOrderDeliveryType.CloseDocument
								: EdoControlReportOrderDeliveryType.CommonDelivery,
					AddressTransferType =
						routeListItem == null || routeListItem.AddressTransferType == null
						? EdoControlReportAddressTransferType.NoTransfer
						: routeListItem.AddressTransferType.Value.ToString().ToEnum<EdoControlReportAddressTransferType>()
				};

			return await rows.ToListAsync(cancellationToken);
		}

		private Func<EdoControlReportData, object> GetSelector(GroupingType groupingType)
		{
			switch(groupingType)
			{
				case GroupingType.DeliveryDate:
					return x => x.DeliveryDate;
				case GroupingType.Counterparty:
					return x => x.ClientId;
				case GroupingType.EdoDocFlowStatus:
					return x => x.EdoStatus;
				case GroupingType.OrderDeliveryType:
					return x => x.OrderDeliveryType;
				case GroupingType.OrderTransferType:
					return x => x.AddressTransferType;
				default:
					throw new InvalidOperationException("Неизвестный тип группировки");
			}
		}

		public Func<EdoControlReportData, string> GetGroupTitle(GroupingType groupingType)
		{
			switch(groupingType)
			{
				case GroupingType.DeliveryDate:
					return x => x.DeliveryDate.ToString("dd.MM.yyyy");
				case GroupingType.Counterparty:
					return x => x.ClientName;
				case GroupingType.EdoDocFlowStatus:
					return x => x.EdoStatus is null ? "" : x.EdoStatus.Value.GetEnumDisplayName();
				case GroupingType.OrderDeliveryType:
					return x => x.OrderDeliveryType.GetEnumDisplayName();
				case GroupingType.OrderTransferType:
					return x => x.AddressTransferType.GetEnumDisplayName();
				default:
					throw new InvalidOperationException("Неизвестный тип группировки");
			}
		}
	}
}
