using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Controllers
{
	public interface IOrderDiscountsController
	{
		void SetCustomDiscountForOrder(DiscountReason reason, decimal discount, DiscountUnits unit, IList<OrderItem> orderItems);
		void SetDiscountFromDiscountReasonForOrder(DiscountReason reason, IList<OrderItem> orderItems);
		void SetDiscountFromDiscountReasonForOrderItem(DiscountReason reason, OrderItem orderItem);
		void SetDiscountFromDiscountReasonForOrderItemWithoutShipment(
			DiscountReason reason, OrderWithoutShipmentForAdvancePaymentItem orderItem);
		void RemoveDiscountFromOrder(IList<OrderItem> orderItems);
		bool OrderItemContainsPromoSetOrFixedPrice(OrderItem orderItem);
		bool IsApplicableDiscount(DiscountReason reason, Nomenclature nomenclature);
	}
}
