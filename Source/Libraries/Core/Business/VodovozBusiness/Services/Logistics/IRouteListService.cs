using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.Services.Logistics
{
	/// <summary>
	/// Интерфейс для управления статусами маршрутных листов.
	/// Отвечает за изменение статусов маршрутов и управление их жизненным циклом.
	/// </summary>
	public interface IRouteListService
	{
		bool TrySendEnRoute(
			IUnitOfWork unitOfWork,
			RouteList routeList,
			out IList<GoodsInRouteListResult> notLoadedGoods,
			CarLoadDocument withDocument = null);

		void SendEnRoute(
			IUnitOfWork unitOfWork,
			int routeListId);

		void SendEnRoute(
			IUnitOfWork unitOfWork,
			RouteList routeList);

		Result TryChangeStatusToNew(IUnitOfWork unitOfWork, RouteList routeList);
		void CompleteRoute(IUnitOfWork unitOfWork, RouteList routeList);

		void CompleteRouteAndCreateTask(
			IUnitOfWork unitOfWork,
			RouteList routeList);

		void AcceptCash(IUnitOfWork unitOfWork, RouteList routeList);
		bool AcceptMileage(IUnitOfWork unitOfWork, RouteList routeList, IValidator validator);
		void ChangeStatus(IUnitOfWork unitOfWork, RouteList routeList, RouteListStatus newStatus);
		void ChangeStatusAndCreateTask(IUnitOfWork unitOfWork, RouteList routeList, RouteListStatus newStatus);
		void UpdateStatus(IUnitOfWork unitOfWork, RouteList routeList, bool isIgnoreAdditionalLoadingDocument = false);

		Result ValidateForAccept(
			RouteList routeList,
			IOrderRepository orderRepository,
			bool skipOverfillValidation = false);

		//-------------------------------------------

		RouteListItem AddAddressFromOrder(IUnitOfWork unitOfWork, RouteList routeList, Order order);
		RouteListItem AddAddressFromOrder(IUnitOfWork unitOfWork, RouteList routeList, int orderId);

		void UpdateStatus(IUnitOfWork uow, RouteListItem address, RouteListItemStatus status);
		void CloseAddresses(IUnitOfWork unitOfWork, RouteList routeList);
		void CloseAddressesAndCreateTask(IUnitOfWork unitOfWork, RouteList routeList);
		void ChangeAddressStatus(IUnitOfWork unitOfWork, RouteList routeList, int routeListAddressid, RouteListItemStatus newAddressStatus);

		void ChangeAddressStatusAndCreateTask(IUnitOfWork unitOfWork, RouteList routeList, int routeListAddressid,
			RouteListItemStatus newAddressStatus, bool isEditAtCashier = false);

		void SetAddressStatusWithoutOrderChange(IUnitOfWork unitOfWork, RouteList routeList, int routeListAddressid,
			RouteListItemStatus newAddressStatus, bool needCreateDeliveryFreeBalanceOperation = true);
	}
}
