using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace Vodovoz.Services.Logistics
{
	public interface IRouteListTransferService
	{
		Result<IEnumerable<string>> TransferAddressesFrom(IUnitOfWork unitOfWork,
			IWageParameterService wageParameterService,
			int sourceRouteListId,
			int targetRouteListId,
			IDictionary<int, AddressTransferType?> addressIdsAndTransferType);

		Result<IEnumerable<string>> RevertTransferedAddressesFrom(IUnitOfWork unitOfWork,
			int sourceRouteListId,
			int? targetRouteListId,
			IEnumerable<int> addressIds, IWageParameterService wageParameterService);

		void TransferAddressTo(IUnitOfWork unitOfWork, RouteList routeList, RouteListItem transferringAddress, RouteListItem targetAddress);

		void RevertTransferAddress(IUnitOfWork unitOfWork, RouteList routeList,
			RouteListItem targetAddress, RouteListItem revertedAddress, IWageParameterService wageParameterService);

		Result<RouteListItem> FindTransferTarget(IUnitOfWork unitOfWork, RouteListItem routeListAddress);
		Result<RouteListItem> FindTransferSource(IUnitOfWork unitOfWork, RouteListItem routeListAddress);
		Result<RouteListItem> FindPrevious(IUnitOfWork unitOfWork, RouteListItem routeListAddress);
		void ConfirmRouteListAddressTransferRecieved(int routeListAddressId, DateTime actionTime);

		Result<IEnumerable<string>> TransferOrdersTo(IUnitOfWork unitOfWork,
			IWageParameterService wageParameterService,
			int targetRouteListId,
			IDictionary<int, AddressTransferType?> ordersIdsAndTransferType);
	}
}
