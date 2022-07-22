using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace RoboAtsService.OrderValidation
{
	public sealed class SelfdeliveryOrderValidator : OrderValidatorBase
	{
		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"Заказ №{x.Id} является самовывозом.");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				if(!order.SelfDelivery)
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
