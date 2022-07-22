using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Roboats;

namespace RoboAtsService.OrderValidation
{
	public sealed class FiasStreetOrderValidator : OrderValidatorBase
	{
		private readonly HashSet<Guid> _availableFiasStreetGuids;

		public FiasStreetOrderValidator(IRoboatsRepository roboatsRepository)
		{
			if(roboatsRepository is null)
			{
				throw new ArgumentNullException(nameof(roboatsRepository));
			}

			_availableFiasStreetGuids = roboatsRepository.GetAvailableForRoboatsFiasStreetGuids();
		}

		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"В точке доставки заказа №{x.Id} должна быть выбрана улица соответствующая справочнику улиц Робоатс");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				if(order.DeliveryPoint == null)
				{
					continue;
				}

				if(order.DeliveryPoint.StreetFiasGuid.HasValue && _availableFiasStreetGuids.Contains(order.DeliveryPoint.StreetFiasGuid.Value))
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
