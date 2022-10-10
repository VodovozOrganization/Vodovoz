using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace RoboAtsService.OrderValidation
{
	public sealed class WaterRowDuplicateOrderValidator : OrderValidatorBase
	{
		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"В заказе №{x.Id} дублируются строки воды в товарах");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				var hasWaterRowDuplicate = order.OrderItems.GroupBy(x => x.Nomenclature.Id).Any(x => x.Count() > 1);
				if(!hasWaterRowDuplicate)
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
