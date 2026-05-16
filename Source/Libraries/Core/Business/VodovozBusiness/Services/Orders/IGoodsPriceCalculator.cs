using System.Collections.Generic;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Domain.Service
{
	/// <summary>
	/// Калькулятор цены товара
	/// </summary>
	public interface IGoodsPriceCalculator
	{
		/// <summary>
		/// Получение цены товара
		/// </summary>
		/// <param name="products">Все товары заказа</param>
		/// <param name="deliveryPoint">Точка доставки</param>
		/// <param name="counterparty">Клиент</param>
		/// <param name="currentProduct">Товар, которому нужно посчитать цену</param>
		/// <param name="hasPermissionsForAlternativePrice">Есть права на выставление альтернативной цены</param>
		/// <returns></returns>
		decimal CalculateItemPrice(
			IEnumerable<IGoods> products,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IGoods currentProduct,
			bool hasPermissionsForAlternativePrice);
	}
}
