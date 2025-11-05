using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.Services.Logistics
{
	public interface IRouteListService
	{
		#region Статусы МЛ
		
		bool TrySendEnRoute(
			IUnitOfWork unitOfWork,
			RouteList routeList,ICallTaskWorker callTaskWorker,
			out IList<GoodsInRouteListResult> notLoadedGoods,
			CarLoadDocument withDocument = null);

		void SendEnRoute(
			IUnitOfWork unitOfWork,
			int routeListId,
			ICallTaskWorker callTaskWorker);

		void SendEnRoute(
			IUnitOfWork unitOfWork,
			RouteList routeList,
			ICallTaskWorker callTaskWorker);

		Result TryChangeStatusToNew(IUnitOfWork unitOfWork, RouteList routeList, IWageParameterService wageParameterService, ICallTaskWorker callTaskWorker);
		void CompleteRoute(IUnitOfWork unitOfWork, RouteList routeList, IWageParameterService wageParameterService, ICallTaskWorker callTaskWorker);

		void CompleteRouteAndCreateTask(
			IUnitOfWork unitOfWork,
			RouteList routeList,
			IWageParameterService wageParameterService,
			ICallTaskWorker callTaskWorker);

		Result AcceptCash(IUnitOfWork unitOfWork, RouteList routeList, ICallTaskWorker callTaskWorker);
		bool AcceptMileage(IUnitOfWork unitOfWork, RouteList routeList, IValidator validator, ICallTaskWorker callTaskWorker);
		void ChangeStatus(IUnitOfWork unitOfWork, RouteList routeList, RouteListStatus newStatus);
		void ChangeStatusAndCreateTask(IUnitOfWork unitOfWork, RouteList routeList, RouteListStatus newStatus, ICallTaskWorker callTaskWorker);
		void UpdateStatus(IUnitOfWork unitOfWork, RouteList routeList, bool isIgnoreAdditionalLoadingDocument = false);

		Result ValidateForAccept(
			RouteList routeList,
			IOrderRepository orderRepository,
			bool skipOverfillValidation = false);
		
		#endregion Статусы МЛ

		#region Адреса

		RouteListItem AddAddressFromOrder(IUnitOfWork unitOfWork, RouteList routeList, Order order);
		RouteListItem AddAddressFromOrder(IUnitOfWork unitOfWork, RouteList routeList, int orderId);

		void UpdateStatus(IUnitOfWork uow, RouteListItem address, RouteListItemStatus status);
		void CloseAddresses(IUnitOfWork unitOfWork, RouteList routeList);
		void CloseAddressesAndCreateTask(IUnitOfWork unitOfWork, RouteList routeList, ICallTaskWorker callTaskWorker);
		void ChangeAddressStatus(IUnitOfWork unitOfWork, RouteList routeList, int routeListAddressid, RouteListItemStatus newAddressStatus, ICallTaskWorker callTaskWorker);

		void ChangeAddressStatusAndCreateTask(IUnitOfWork unitOfWork, RouteList routeList, int routeListAddressid,
			RouteListItemStatus newAddressStatus, ICallTaskWorker callTaskWorker,  bool isEditAtCashier = false);

		void SetAddressStatusWithoutOrderChange(IUnitOfWork unitOfWork, RouteList routeList, int routeListAddressid,
			RouteListItemStatus newAddressStatus, bool needCreateDeliveryFreeBalanceOperation = true);
		
		#endregion
	}
}
