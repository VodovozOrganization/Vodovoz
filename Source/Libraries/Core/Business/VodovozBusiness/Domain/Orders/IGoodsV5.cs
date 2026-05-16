using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	/// <summary>
	/// Общий контракт товара
	/// </summary>
	public interface IGoodsV5
	{
		/// <summary>
		/// Цена
		/// </summary>
		decimal Price { get; }
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Фикса
		/// </summary>
		bool IsFixedPrice { get; }
		/// <summary>
		/// Номенклатура <see cref="Vodovoz.Domain.Goods.Nomenclature"/>
		/// </summary>
		Nomenclature Nomenclature { get; }
		/// <summary>
		/// Промо набор <see cref="Vodovoz.Domain.Orders.PromotionalSet"/>
		/// </summary>
		PromotionalSet PromoSet { get; set; }
		/// <summary>
		/// Скидки
		/// </summary>
		IEnumerable<IDiscountData> Discounts { get; set; }
	}
}
