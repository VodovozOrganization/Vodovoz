using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Service
{
	public interface IGoodsPriceCalculator
	{
		decimal CalculateItemPrice(
			IEnumerable<IProduct> products,
			DeliveryPoint deliveryPoint,
			CounterpartyContract contract,
			Nomenclature nomenclature,
			PromotionalSet promoSet,
			decimal bottlesCount,
			bool hasPermissionsForAlternativePrice);
	}
}
