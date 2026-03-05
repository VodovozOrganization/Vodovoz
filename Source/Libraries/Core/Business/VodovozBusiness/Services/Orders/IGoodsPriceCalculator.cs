using System.Collections.Generic;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Domain.Service
{
	public interface IGoodsPriceCalculator
	{
		decimal CalculateItemPrice(
			IEnumerable<ICalculatingPrice> products,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			ICalculatingPrice currentProduct,
			bool hasPermissionsForAlternativePrice);
	}
}
