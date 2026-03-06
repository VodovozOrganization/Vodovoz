using System.Collections.Generic;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders.V5;

namespace VodovozBusiness.Services.Orders.V5
{
	public interface IGoodsPriceCalculatorV5
	{
		decimal CalculateItemPrice(
			IEnumerable<ICalculatingPriceV5> products,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			ICalculatingPriceV5 currentProduct,
			bool hasPermissionsForAlternativePrice);
	}
}
