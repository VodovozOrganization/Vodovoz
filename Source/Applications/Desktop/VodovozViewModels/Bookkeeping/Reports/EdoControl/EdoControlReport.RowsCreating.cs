using ClosedXML.Report.Utils;
using DynamicData;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
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

		private IEnumerable<EdoControlReportDocFlowStatus> _includedEdoDocFlowStatuses = new List<EdoControlReportDocFlowStatus>();
		private IEnumerable<EdoControlReportDocFlowStatus> _excludedEdoDocFlowStatuses = new List<EdoControlReportDocFlowStatus>();

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

			var edoDocFlowStatusFilter = filterViewModel.GetFilter<IncludeExcludeEnumFilter<EdoControlReportDocFlowStatus>>();
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

		private IList<EdoControlReportRow> ProcessNoGroups(
			IEnumerable<EdoControlReportOrderData> dataNodes,
			CancellationToken cancellationToken)
		{
			if(_groupingTypes.Count() != 0)
			{
				throw new InvalidOperationException("Группировка не должна быть выбрана");
			}

			var rows = new List<EdoControlReportRow>();

			rows.AddRange(ConvertDataNodesToToReportRows(dataNodes, cancellationToken));

			return rows;
		}

		private IList<EdoControlReportRow> Process1stLevelGroups(
			IEnumerable<EdoControlReportOrderData> dataNodes,
			CancellationToken cancellationToken)
		{
			if(_groupingTypes.Count() != 1)
			{
				throw new InvalidOperationException("Количество выбранных группировок должно быть 1");
			}

			var result = new List<EdoControlReportOrderData>();

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
			IEnumerable<EdoControlReportOrderData> dataNodes,
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
					if(i >= groupsCount || !IsKeysEqualsOrBothNull(groupedNodes[i].Key.Key1, currentFirstKeyValue))
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

		private IList<EdoControlReportRow> Process3rdLevelGroups(
			IEnumerable<EdoControlReportOrderData> dataNodes,
			CancellationToken cancellationToken)
		{
			if(_groupingTypes.Count() != 3)
			{
				throw new InvalidOperationException("Количество выбранных группировок должно быть 3");
			}

			var result = new List<EdoControlReportRow>();

			var firstGroupingType = _groupingTypes.ElementAt(0);
			var secondGroupingType = _groupingTypes.ElementAt(1);
			var thirdGroupingType = _groupingTypes.ElementAt(2);

			var firstSelector = GetSelector(firstGroupingType);
			var secondSelector = GetSelector(secondGroupingType);
			var thirdSelector = GetSelector(thirdGroupingType);

			var groupedNodes =
				(from node in dataNodes
				 group node by new { Key1 = firstSelector.Invoke(node), Key2 = secondSelector.Invoke(node), Key3 = thirdSelector.Invoke(node) } into g
				 select new { Key = g.Key, Items = g.ToList() })
				 .OrderBy(g => g.Key.Key1)
				 .ThenBy(g => g.Key.Key2)
				 .ThenBy(g => g.Key.Key3)
				 .ToList();

			var groupsCount = groupedNodes.Count;

			var groupsSummaryInfo =
				$"Группировка по: " +
				$"{firstGroupingType.GetEnumDisplayName()} " +
				$"| {secondGroupingType.GetEnumDisplayName()} " +
				$"| {thirdGroupingType.GetEnumDisplayName()}";

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
					if(i >= groupsCount || !IsKeysEqualsOrBothNull(groupedNodes[i].Key.Key1, currentFirstKeyValue))
					{
						break;
					}

					var currentSecondKeyValue = groupedNodes[i].Key.Key2;

					var secondLevelGroupTitle = GetGroupTitle(secondGroupingType).Invoke(groupedNodes[i].Items.First());

					if(secondLevelGroupTitle != lastSecondLevelGroupTitle)
					{
						result.Add(new EdoControlReportRow(secondLevelGroupTitle));
					}

					var lastThirdLevelGroupTitle = string.Empty;

					while(true)
					{
						if(i == groupsCount
							|| !IsKeysEqualsOrBothNull(groupedNodes[i].Key.Key1, currentFirstKeyValue)
							|| !IsKeysEqualsOrBothNull(groupedNodes[i].Key.Key2, currentSecondKeyValue))
						{
							break;
						}

						var thirdLevelGroupTitle = GetGroupTitle(thirdGroupingType).Invoke(groupedNodes[i].Items.First());

						if(thirdLevelGroupTitle != lastThirdLevelGroupTitle)
						{
							result.Add(new EdoControlReportRow(thirdLevelGroupTitle));
						}

						var dataRows = ConvertDataNodesToToReportRows(groupedNodes[i].Items, cancellationToken);

						result.Add(dataRows);

						i++;

						lastThirdLevelGroupTitle = thirdLevelGroupTitle;
					}

					lastSecondLevelGroupTitle = secondLevelGroupTitle;
				}
			}

			return result;
		}

		private IList<EdoControlReportRow> ConvertDataNodesToToReportRows(
			IEnumerable<EdoControlReportOrderData> nodes,
			CancellationToken cancellationToken)
		{
			var rows = new List<EdoControlReportRow>();

			foreach(var node in nodes)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var dataRow = new EdoControlReportRow(node);

				rows.Add(dataRow);
			}

			return rows;
		}

		private bool IsKeysEqualsOrBothNull(object key1, object key2)
		{
			var result =
				(key1 is null && key2 is null)
				|| (key1 != null && key2 != null && key1.Equals(key2));

			return result;
		}

		#endregion Grouping

		private void SetGroupingTypes(IEnumerable<GroupingType> groupingTypes)
		{
			_groupingTypes = groupingTypes;
		}

		private async Task SetReportRows(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			try
			{
				var dataNodes = await GetOrdersData(uow, cancellationToken);

				switch(_groupingTypes.Count())
				{
					case 0:
						Rows = ProcessNoGroups(dataNodes, cancellationToken);
						break;
					case 1:
						Rows = Process1stLevelGroups(dataNodes, cancellationToken);
						break;
					case 2:
						Rows = Process2ndLevelGroups(dataNodes, cancellationToken);
						break;
					case 3:
						Rows = Process3rdLevelGroups(dataNodes, cancellationToken);
						break;
					default:
						throw new InvalidOperationException("Выбрано недопустимое количество группировок");
				}
			}
			catch(Exception ex)
			{
				throw;
			}
		}

		private async Task<IList<EdoControlReportOrderData>> GetOrdersData(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var rows =
				from order in uow.Session.Query<Order>()
				join client in uow.Session.Query<Counterparty>() on order.Client.Id equals client.Id
				join rli in uow.Session.Query<RouteListItem>() on order.Id equals rli.Order.Id into routeListItems
				from routeListItem in routeListItems.DefaultIfEmpty()
				join ec in uow.Session.Query<EdoContainer>() on order.Id equals ec.Order.Id into edoContainers
				from edoContainer in edoContainers.DefaultIfEmpty()

				let lastEdoByOrder =
					(int?)(from container in uow.Session.Query<EdoContainer>()
						   where container.Order.Id == order.Id
						   orderby container.Id descending
						   select container.Id)
						   .FirstOrDefault()

				let edoDocumentStatus =
					(EdoDocumentStatus?)(from edoRequest in uow.Session.Query<FormalEdoRequest>()
										 join orderEdoDocument in uow.Session.Query<OrderEdoDocument>()
										 on edoRequest.Task.Id equals orderEdoDocument.DocumentTaskId
										 where edoRequest.Order.Id == order.Id
										 orderby edoRequest.Id descending, orderEdoDocument.Id descending
										 select orderEdoDocument.Status)
										.FirstOrDefault()

				where
					order.DeliveryDate >= StartDate && order.DeliveryDate < EndDate.Date.AddDays(1)
					&& _orderStatuses.Contains(order.OrderStatus)
					&& (routeListItem == null || routeListItem.Status != RouteListItemStatus.Transfered)
					&& (edoContainer == null || edoContainer.Id == lastEdoByOrder.Value)

					&& ((_includedCounterpartyTypes.Count() == 0 && _includedCounterpartySubtypes.Count() == 0)
						|| _includedCounterpartyTypes.Contains(client.CounterpartyType)
						|| _includedCounterpartySubtypes.Contains(client.CounterpartySubtype.Id))
					&& !_excludedCounterpartyTypes.Contains(client.CounterpartyType)
					&& (client.CounterpartySubtype.Id == null || !_excludedCounterpartySubtypes.Contains(client.CounterpartySubtype.Id))

					&& (_includedCounterparties.Count() == 0 || _includedCounterparties.Contains(client.Id))
					&& !_excludedCounterparties.Contains(client.Id)

					&& (_includedPersonTypes.Count() == 0 || _includedPersonTypes.Contains(client.PersonType))
					&& !_excludedPersonTypes.Contains(client.PersonType)

					&& ((_includedPaymentTypes.Count() == 0 && _includedPaymentFroms.Count() == 0 && _includedPaymentByTerminalSources.Count() == 0)
						|| _includedPaymentTypes.Contains(order.PaymentType)
						|| (order.PaymentType == PaymentType.PaidOnline && order.PaymentByCardFrom.Id != null && _includedPaymentFroms.Contains(order.PaymentByCardFrom.Id))
						|| (order.PaymentType == PaymentType.Terminal && order.PaymentByTerminalSource != null && _includedPaymentByTerminalSources.Contains(order.PaymentByTerminalSource.Value)))

					&& !_excludedPaymentTypes.Contains(order.PaymentType)
					&& !(order.PaymentType == PaymentType.PaidOnline && order.PaymentByCardFrom.Id != null && _excludedPaymentFroms.Contains(order.PaymentByCardFrom.Id))
					&& !(order.PaymentType == PaymentType.Terminal && order.PaymentByTerminalSource != null && _excludedPaymentByTerminalSources.Contains(order.PaymentByTerminalSource.Value))

					&& (_includedOrderDeliveryTypes.Count() == 0
						|| (_includedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.FastDelivery) && order.IsFastDelivery)
						|| (_includedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.SelfDelivery) && order.SelfDelivery)
						|| (_includedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.CloseDocument) && order.DeliverySchedule.Id == _closingDocumentDeliveryScheduleId)
						|| (_includedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.CommonDelivery) && order.DeliverySchedule != null && order.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId))
					&& !(_excludedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.FastDelivery) && order.IsFastDelivery)
					&& !(_excludedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.SelfDelivery) && order.SelfDelivery)
					&& !(_excludedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.CloseDocument) && !order.SelfDelivery && order.DeliverySchedule.Id == _closingDocumentDeliveryScheduleId)
					&& !(_excludedOrderDeliveryTypes.Contains(EdoControlReportOrderDeliveryType.CommonDelivery) && !order.SelfDelivery && order.DeliverySchedule != null && order.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId)

					&& (_includedAddressTransferTypes.Count() == 0
						|| (_includedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.NeedToReload) && routeListItem.AddressTransferType == AddressTransferType.NeedToReload)
						|| (_includedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.FromHandToHand) && routeListItem.AddressTransferType == AddressTransferType.FromHandToHand)
						|| (_includedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.FromFreeBalance) && routeListItem.AddressTransferType == AddressTransferType.FromFreeBalance)
						|| (_includedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.NoTransfer) && routeListItem.AddressTransferType == null))
					&& !(_excludedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.NeedToReload) && routeListItem.AddressTransferType != null && routeListItem.AddressTransferType == AddressTransferType.NeedToReload)
					&& !(_excludedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.FromHandToHand) && routeListItem.AddressTransferType != null && routeListItem.AddressTransferType == AddressTransferType.FromHandToHand)
					&& !(_excludedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.FromFreeBalance) && routeListItem.AddressTransferType != null && routeListItem.AddressTransferType == AddressTransferType.FromFreeBalance)
					&& !(_excludedAddressTransferTypes.Contains(EdoControlReportAddressTransferType.NoTransfer) && routeListItem.AddressTransferType == null)

				select new EdoControlReportOrderData
				{
					EdoContainerId = edoContainer.Id,
					ClientId = client.Id,
					ClientName = client.Name,
					OrderId = order.Id,
					RouteListId = routeListItem.RouteList.Id,
					DeliveryDate = order.DeliveryDate.Value,
					OldEdoDocflowStatus = edoContainer.EdoDocFlowStatus,
					NewEdoDocflowStatus = edoDocumentStatus,
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

			var reportRows = await rows.ToListAsync(cancellationToken);

			if(_includedEdoDocFlowStatuses.Count() > 0)
			{
				reportRows = reportRows
					.Where(x => _includedEdoDocFlowStatuses.Contains(x.EdoStatus))
					.ToList();
			}

			if(_excludedEdoDocFlowStatuses.Count() > 0)
			{
				reportRows = reportRows
					.Where(x => !_excludedEdoDocFlowStatuses.Contains(x.EdoStatus))
					.ToList();
			}

			return reportRows;
		}

		private Func<EdoControlReportOrderData, object> GetSelector(GroupingType groupingType)
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

		private Func<EdoControlReportOrderData, string> GetGroupTitle(GroupingType groupingType)
		{
			switch(groupingType)
			{
				case GroupingType.DeliveryDate:
					return x => x.DeliveryDate.ToString("dd.MM.yyyy");
				case GroupingType.Counterparty:
					return x => x.ClientName;
				case GroupingType.EdoDocFlowStatus:
					return x => x.EdoStatus.GetEnumDisplayName();
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
