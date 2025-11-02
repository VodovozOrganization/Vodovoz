using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace RoboatsService.OrderValidation
{
	public sealed class ReasonForLeavingValidator : OrderValidatorBase
	{
		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"У контрагента в заказе №{x.Id} установлена неизвестная причина выбытия товара");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				if(order.Client.ReasonForLeaving != ReasonForLeaving.Unknown)
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
