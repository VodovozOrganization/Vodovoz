using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace RoboAtsService.OrderValidation
{
	public sealed class PromosetOrderValidator : OrderValidatorBase
	{
		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"В заказа №{x.Id} добавлен промонабор");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				if(!order.PromotionalSets.Any())
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
