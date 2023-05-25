using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Controllers
{
	public interface IRouteListAddressKeepingDocumentController
	{
		void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, Order order, DeliveryFreeBalanceType deliveryFreeBalanceType);
		void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, RouteListItem address, RouteListItemStatus oldStatus, RouteListItemStatus newStatus);
		void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, RouteListItem routeListItem,
			DeliveryFreeBalanceType deliveryFreeBalanceType, bool isFullRecreation = false, bool isActualCount = false,
			RouteListItemStatus? oldStatus = null, RouteListItemStatus? newStatus = null);
		void CreateOrUpdateRouteListKeepingDocument(
			IUnitOfWork uoW, RouteList routeList, DeliveryFreeBalanceType deliveryFreeBalanceType, bool isFullRecreation, bool isActualCount);
		HashSet<RouteListAddressKeepingDocumentItem> CreateOrUpdateRouteListKeepingDocumentByDiscrepancy(
			IUnitOfWork uow, RouteListItem changedRouteListItem, HashSet<RouteListAddressKeepingDocumentItem> itemsCacheList = null);
		void RemoveRouteListKeepingDocument(IUnitOfWork uow, RouteListItem routeListItem);
	}
}
