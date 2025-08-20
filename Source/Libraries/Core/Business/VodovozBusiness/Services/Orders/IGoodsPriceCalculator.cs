using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Service
{
	public interface IGoodsPriceCalculator
	{
		decimal CalculateItemPrice(
			IEnumerable<IProduct> products,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IProduct currentProduct,
			bool hasPermissionsForAlternativePrice);
	}
}
