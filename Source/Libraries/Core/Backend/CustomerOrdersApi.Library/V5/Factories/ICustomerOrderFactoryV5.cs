using CustomerOrdersApi.Library.V5.Dto.Orders;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Factories
{
	public interface ICustomerOrderFactoryV5
	{
		Task<DetailedOrderInfoDto> CreateDetailedOrderInfo(
			IUnitOfWork uow,
			Order order,
			OrderRating orderRating,
			OnlineOrderTimers timers,
			OnlineOrder onlineOrder,
			DateTime ratingAvailableFrom,
			CancellationToken cancellationToken
			);
		Task<DetailedOrderInfoDto> CreateDetailedOrderInfo(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			OrderRating orderRating,
			OnlineOrderTimers timers,
			int? orderId,
			DateTime ratingAvailableFrom,
			CancellationToken cancellationToken
			);
		IEnumerable<OrderRatingReasonDto> GetOrderRatingReasonDtos(IEnumerable<OrderRatingReason> orderRatingReasons);
	}
}
