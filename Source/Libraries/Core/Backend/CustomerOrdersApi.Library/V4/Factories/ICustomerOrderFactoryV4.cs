using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.V4.Orders;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V4.Factories
{
	public interface ICustomerOrderFactoryV4
	{
		DetailedOrderInfoDto CreateDetailedOrderInfo(
			Order order, OrderRating orderRating, OnlineOrderTimers timers, int? onlineOrderId, DateTime ratingAvailableFrom);
		DetailedOrderInfoDto CreateDetailedOrderInfo(
			OnlineOrder onlineOrder, OrderRating orderRating, OnlineOrderTimers timers, int? orderId, DateTime ratingAvailableFrom);
		IEnumerable<OrderRatingReasonDto> GetOrderRatingReasonDtos(IEnumerable<OrderRatingReason> orderRatingReasons);
	}
}
