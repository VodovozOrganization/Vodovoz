﻿using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Controllers
{
	public interface IOrderDiscountsController
	{
		void SetCustomDiscountForOrder(DiscountReason reason, decimal discount, DiscountUnits unit, IList<OrderItem> orderItems);
		void SetDiscountFromDiscountReasonForOrder(
			DiscountReason reason, IList<OrderItem> orderItems, bool canChangeDiscountValue, out string messages);
		bool SetDiscountFromDiscountReasonForOrderItem(
			DiscountReason reason, OrderItem orderItem, bool canChangeDiscountValue, out string message);
		void SetDiscountFromDiscountReasonForOrderItemWithoutShipment(
			DiscountReason reason, OrderWithoutShipmentForAdvancePaymentItem orderItem);
		void RemoveDiscountFromOrder(IList<OrderItem> orderItems);
		bool IsApplicableDiscount(DiscountReason reason, Nomenclature nomenclature);
	}
}
