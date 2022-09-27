using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace RoboAtsService.OrderValidation
{
	public sealed class BottleStockOrderValidator : OrderValidatorBase
	{
		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"К заказу №{x.Id} применена акция \"Бутыль\"");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				if(!order.IsBottleStock)
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
