using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Controllers
{
	public interface IRouteListAddressKeepingDocumentController
	{
		void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, RouteListItem address, RouteListItemStatus oldStatus, RouteListItemStatus newStatus);
		void CreateOrUpdateRouteListKeepingDocument(
			IUnitOfWork uow,
			RouteListItem routeListItem,
			DeliveryFreeBalanceType deliveryFreeBalanceType,
			bool isFullRecreation = false,
			bool isActualCount = false,
			RouteListItemStatus? oldStatus = null,
			RouteListItemStatus? newStatus = null,
			bool needRouteListUpdate = false,
			Employee employee = null);
		void CreateDeliveryFreeBalanceTransferItems(IUnitOfWork uow, AddressTransferDocumentItem addressTransferDocumentItem);
		HashSet<RouteListAddressKeepingDocumentItem> CreateOrUpdateRouteListKeepingDocumentByDiscrepancy(
			IUnitOfWork uow, IUnitOfWorkFactory unitOfWorkFactory, RouteListItem changedRouteListItem, HashSet<RouteListAddressKeepingDocumentItem> itemsCacheList = null, bool isBottlesDiscrepancy = false,
			bool forceUsePlanCount = false, bool isFromRouteListClosingUndelivery = false);
		void RemoveRouteListKeepingDocument(IUnitOfWork uow, RouteListItem routeListItem, bool needRouteListUpdate = false);
	}
}
