using CustomerOrdersApi.Library.V6.Dto.Orders;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Mango;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V6.Factories
{
	public interface ICustomerOrderFactoryV6
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
