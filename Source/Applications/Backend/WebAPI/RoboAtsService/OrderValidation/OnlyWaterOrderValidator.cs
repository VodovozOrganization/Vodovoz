using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;
using Vodovoz.Settings.Nomenclature;

namespace RoboatsService.OrderValidation
{
	public sealed class OnlyWaterOrderValidator : OrderValidatorBase
	{
		private readonly INomenclatureSettings _nomenclatureParametersProvider;

		public OnlyWaterOrderValidator(INomenclatureSettings nomenclatureParametersProvider)
		{
			_nomenclatureParametersProvider = nomenclatureParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureParametersProvider));
		}

		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"В заказе №{x.Id} добавлены товары не относящиеся к воде (кроме платной и экспресс доставки)");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				var hasOnlyWater = !order.OrderItems
					.Where(x => x.Nomenclature.Id != _nomenclatureParametersProvider.PaidDeliveryNomenclatureId)
					.Where(x => x.Nomenclature.Id != _nomenclatureParametersProvider.FastDeliveryNomenclatureId)
					.Any(x => x.Nomenclature.Category != NomenclatureCategory.water);

				if(hasOnlyWater)
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
