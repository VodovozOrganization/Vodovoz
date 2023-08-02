using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace RoboatsService.OrderValidation
{
	public sealed class ActiveDeliveryPointOrderValidator : OrderValidatorBase
	{
		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"В заказе №{x.Id} точка доставки №{x.DeliveryPoint.Id} деактивирована");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				if(order.DeliveryPoint == null)
				{
					AddValidOrder(order);
				}

				if(order.DeliveryPoint.IsActive)
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
