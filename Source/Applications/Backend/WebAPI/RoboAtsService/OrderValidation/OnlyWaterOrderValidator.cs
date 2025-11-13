using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;
using Vodovoz.Settings.Nomenclature;

namespace RoboatsService.OrderValidation
{
	public sealed class OnlyWaterOrderValidator : OrderValidatorBase
	{
		private readonly INomenclatureSettings _nomenclatureSettings;

		public OnlyWaterOrderValidator(INomenclatureSettings nomenclatureSettings)
		{
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
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
					.Where(x => x.Nomenclature.Id != _nomenclatureSettings.PaidDeliveryNomenclatureId)
					.Where(x => x.Nomenclature.Id != _nomenclatureSettings.FastDeliveryNomenclatureId)
					.Any(x => x.Nomenclature.Category != NomenclatureCategory.water);

				if(hasOnlyWater)
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
