using CustomerOrdersApi.Library.V4.Dto.Orders;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using DetailedOrderInfoDto = CustomerOrdersApi.Library.V5.Dto.Orders.DetailedOrderInfoDto;

namespace CustomerOrdersApi.Library.V5.Factories
{
	public interface ICustomerOrderFactoryV5
	{
		DetailedOrderInfoDto CreateDetailedOrderInfo(
			Order order, OrderRating orderRating, OnlineOrderTimers timers, OnlineOrder onlineOrder, DateTime ratingAvailableFrom);
		DetailedOrderInfoDto CreateDetailedOrderInfo(
			OnlineOrder onlineOrder, OrderRating orderRating, OnlineOrderTimers timers, int? orderId, DateTime ratingAvailableFrom);
		IEnumerable<OrderRatingReasonDto> GetOrderRatingReasonDtos(IEnumerable<OrderRatingReason> orderRatingReasons);
	}
}
