using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders.V5;

namespace VodovozBusiness.Services.Orders.V5
{
	/// <summary>
	/// Контракт калькулятора цен товаров
	/// </summary>
	public interface IGoodsPriceCalculatorV5
	{
		/// <summary>
		/// Подсчет цены товара
		/// </summary>
		/// <param name="products">Текущий список товаров</param>
		/// <param name="counterparty">Клиент</param>
		/// <param name="deliveryPoint">Точка доставки</param>
		/// <param name="nomenclature">Номенклатура товара для которого считаем цену</param>
		/// <param name="isPromoSet">Промонабор или нет</param>
		/// <param name="hasPermissionsForAlternativePrice">Есть ли права на установку альтернативных цен</param>
		/// <param name="addingGoodsCount">Добавляемое количество, если товар уже в списке, то 0</param>
		/// <param name="needGetFixedPrice">Нужно ли подбирать фиксу</param>
		/// <returns></returns>
		decimal CalculatePrice(
			IEnumerable<ICalculatingPriceV5> products,
			Counterparty counterparty,
			DeliveryPoint deliveryPoint,
			Nomenclature nomenclature,
			bool isPromoSet,
			bool hasPermissionsForAlternativePrice,
			decimal addingGoodsCount = 0,
			bool needGetFixedPrice = true);
	}
}
