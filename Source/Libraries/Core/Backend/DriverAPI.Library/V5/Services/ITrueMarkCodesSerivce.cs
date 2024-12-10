using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors;

namespace DriverAPI.Library.V5.Services
{
	public interface ITrueMarkCodesSerivce
	{
		IEnumerable<TrueMarkWaterIdentificationCode> CreateTrueMarkWaterIdentificationCodesFromScannedCodes(IUnitOfWork uow, IEnumerable<string> scannedCodes);
		Result IsAllTrueMarkCodeGtinsMatchesToNomenclatureGtin(IEnumerable<TrueMarkWaterIdentificationCode> codes, OrderItem orderItem);
		Result IsAllTrueMarkCodesAddedToOrderItem(IEnumerable<TrueMarkWaterIdentificationCode> codes, OrderItem orderItem);
		Task<Result> IsAllTrueMarkCodesIntroducedAndHasCorrectInn(IEnumerable<TrueMarkWaterIdentificationCode> codes, CancellationToken cancellationToken);
		Result IsAllTrueMarkCodesHasNoDuplicates(IUnitOfWork uow, IEnumerable<TrueMarkWaterIdentificationCode> codes);
		IList<RouteListItemTrueMarkProductCode> CreateAcceptedNoProblemRouteListItemTrueMarkProductCodesFromIdentificationCodes(
			IEnumerable<TrueMarkWaterIdentificationCode> codes, RouteListItem routeListItem);
		IList<RouteListItemTrueMarkProductCode> GetRouteListItemTrueMarkProductCodesFromIdentificationCodes(IUnitOfWork uow,
			IEnumerable<TrueMarkWaterIdentificationCode> codes, OrderItem orderItem, RouteListItem routeListItem, bool isDefectBottle = false);
	}
}
