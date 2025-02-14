using Gamma.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain.Orders;

namespace RoboatsService.OrderValidation
{
	public sealed class StatusOrderValidator : OrderValidatorBase
	{
		private OrderStatus[] _allowedOrderStatuses;
		private string _allowedOrderStatusTittles;

		public StatusOrderValidator()
		{
			_allowedOrderStatuses = new[] { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };
			_allowedOrderStatusTittles = string.Join(", ", _allowedOrderStatuses.Select(x => x.GetEnumTitle()));
		}

		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"Заказ №{x.Id} в статусе {x.OrderStatus.GetEnumTitle()}, а должен быть в одном из статусов: {_allowedOrderStatusTittles}");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				if(_allowedOrderStatuses.Contains(order.OrderStatus))
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
