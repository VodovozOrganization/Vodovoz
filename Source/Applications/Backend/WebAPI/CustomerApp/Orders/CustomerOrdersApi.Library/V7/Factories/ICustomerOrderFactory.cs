using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V7.Dto.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Mango;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Factories
{
	public interface ICustomerOrderFactory
	{
		Task<DetailedOrderInfoDto> CreateDetailedOrderInfo(
			IUnitOfWork uow,
			Order order,
			OrderRating orderRating,
			OnlineOrderTimers timers,
			OnlineOrder onlineOrder,
			DateTime ratingAvailableFrom,
			DriverMangoExtensionNumber driversMangoExtensionNumber,
			bool establishedRoute,
			bool isOrderWasSelectedAsNext,
			DateTime? driversCoordinatesLastUpdateTime,
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

		ActiveOrderDto CreateActiveOrderInfo(
			OrderDto orderDto,
			bool establishedRoute,
			bool isOrderWasSelectedAsNext,
			DateTime? driversCoordinatesLastUpdateTime
			);

		IEnumerable<OrderRatingReasonDto> GetOrderRatingReasonDtos(IEnumerable<OrderRatingReason> orderRatingReasons);
	}
}
