using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.Default.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Default.Factories
{
	public interface ICustomerOrderFactory
	{
		DetailedOrderInfoDto CreateDetailedOrderInfo(
			Order order, OrderRating orderRating, int? onlineOrderId, DateTime ratingAvailableFrom);
		DetailedOrderInfoDto CreateDetailedOrderInfo(
			OnlineOrder onlineOrder, OrderRating orderRating, int? orderId, DateTime ratingAvailableFrom);
		IEnumerable<OrderRatingReasonDto> GetOrderRatingReasonDtos(IEnumerable<OrderRatingReason> orderRatingReasons);
	}
}
