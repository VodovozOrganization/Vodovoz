using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace RoboAtsService.OrderValidation
{
	public sealed class DateOrderValidator : OrderValidatorBase
	{
		private int _months = 4;
		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"Заказ №{x.Id} был оформлен более {_months} месяцев назад");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				if(order.DeliveryDate >= DateTime.Today.AddMonths(-_months))
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
