using QS.DomainModel.UoW;
using QS.Validation;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Errors;

namespace Vodovoz.Services.Logistics
{
	public interface IRouteListService
	{
		void AcceptConditions(
			IUnitOfWork unitOfWork,
			int driverId, IEnumerable<int> specialConditionsIds);

		IDictionary<int, string> GetSpecialConditionsDictionaryFor(
			IUnitOfWork unitOfWork,
			int routeListId);

		IEnumerable<RouteListSpecialCondition> GetSpecialConditionsFor(
			IUnitOfWork unitOfWork,
			int routeListId);

		void SendEnRoute(IUnitOfWork unitOfWork, int routeListId);

		void SendEnRoute(IUnitOfWork unitOfWork, RouteList routeList);

		bool TrySendEnRoute(
			IUnitOfWork unitOfWork,
			RouteList routeList,
			out IList<GoodsInRouteListResult> notLoadedGoods,
			CarLoadDocument withDocument = null);

		Result<IEnumerable<string>> TransferAddressesFrom(
			IUnitOfWork unitOfWork,
			int sourceRouteListId,
			int targetRouteListId,
			IDictionary<int, AddressTransferType?> addressIdsAndTransferType);

		Result<IEnumerable<string>> RevertTransferedAddressesFrom(
			IUnitOfWork unitOfWork,
			int sourceRouteListId,
			int? targetRouteListId,
			IEnumerable<int> addressIds);

		Result<IEnumerable<string>> TransferOrdersTo(
			IUnitOfWork unitOfWork,
			int targetRouteListId,
			IDictionary<int, AddressTransferType?> ordersIdsAndTransferType);

		Result ValidateForAccept(RouteList routeList, bool skipOverfillValidation = false);

		Result TryChangeStatusToNew(
			IUnitOfWork unitOfWork,
			RouteList routeList);

		Result<IEnumerable<string>> TryChangeStatusToAccepted(
			IUnitOfWork unitOfWork,
			RouteList routeList,
			Action<bool> disableItemsUpdate,
			IValidator validationService,
			bool skipOverfillValidation = false,
			bool confirmRecalculateRoute = false,
			bool confirmSendOnClosing = false,
			bool confirmSenEnRoute = false);
	}
}
