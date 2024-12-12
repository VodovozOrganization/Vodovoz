using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Roboats;

namespace RoboatsService.OrderValidation
{
	public sealed class RoboatsWaterOrderValidator : OrderValidatorBase
	{
		private readonly IEnumerable<int> _availableWaterNomenclatures;

		public RoboatsWaterOrderValidator(IRoboatsRepository roboatsRepository)
		{
			if(roboatsRepository is null)
			{
				throw new ArgumentNullException(nameof(roboatsRepository));
			}

			_availableWaterNomenclatures = roboatsRepository.GetWaterTypes().Select(x => x.Nomenclature.Id);
		}

		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"В заказе №{x.Id} имеется вода не соответствующая справочнику типов воды для Робоатс");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				var hasOnlyRoboatsWater = order.OrderItems
					.Where(x => x.Nomenclature.Category == NomenclatureCategory.water)
					.All(x => _availableWaterNomenclatures.Contains(x.Nomenclature.Id));

				if(hasOnlyRoboatsWater)
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
