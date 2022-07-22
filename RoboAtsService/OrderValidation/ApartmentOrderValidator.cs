using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace RoboAtsService.OrderValidation
{
	public sealed class ApartmentOrderValidator : OrderValidatorBase
	{
		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"В точке доставки заказа №{x.Id} не выбран тип помещения \"Квартира\"");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				if(order.DeliveryPoint?.RoomType == RoomType.Apartment)
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
