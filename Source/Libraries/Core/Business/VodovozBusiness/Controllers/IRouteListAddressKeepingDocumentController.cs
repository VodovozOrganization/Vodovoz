using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Controllers
{
	public interface IRouteListAddressKeepingDocumentController
	{
		void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, RouteListItem address, RouteListItemStatus oldStatus, RouteListItemStatus newStatus);
		void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, RouteListItem routeListItem, DeliveryFreeBalanceType deliveryFreeBalanceType);
	}
}
