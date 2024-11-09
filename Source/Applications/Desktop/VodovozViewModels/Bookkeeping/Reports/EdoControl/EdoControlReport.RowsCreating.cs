using ClosedXML.Report.Utils;
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
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Reports.Editing.Modifiers;
using static Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters.IncludeExcludeBookkeepingReportsFilterFactory;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl
{
	public partial class EdoControlReport
	{
		private IEnumerable<Func<EdoControlReportRow, object>> _groupingsSelectors = new List<Func<EdoControlReportRow, object>>();

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

		private void SetGroupingSelectors(IEnumerable<GroupingType> groupingTypes)
		{
			var groupingsSelectors = new List<Func<EdoControlReportRow, object>>();

			foreach(var groupingType in groupingTypes)
			{
				groupingsSelectors.Add(GetSelector(groupingType));
			}

			_groupingsSelectors = groupingsSelectors;
		}

		private async Task SetReportRows(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			try
			{
				Rows = await CreateReportRows(uow, cancellationToken);
			}
			catch(Exception ex)
			{
				throw;
			}
		}

		private async Task<IList<EdoControlReportRow>> CreateReportRows(IUnitOfWork uow, CancellationToken cancellationToken)
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

				select new EdoControlReportRow
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

		private Func<EdoControlReportRow, object> GetSelector(GroupingType groupingType)
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
	}
}
