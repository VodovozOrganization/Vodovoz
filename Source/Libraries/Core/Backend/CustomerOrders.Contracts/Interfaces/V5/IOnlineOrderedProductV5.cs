using System.Collections.Generic;

namespace CustomerOrders.Contracts.Interfaces.V5
{
	public interface IOnlineOrderedProductV5
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		int NomenclatureId { get; }
		/// <summary>
		/// Цена
		/// </summary>
		decimal Price { get; set; }
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Id промонабора
		/// </summary>
		int? PromoSetId { get; }
		/// <summary>
		/// Фикса
		/// </summary>
		bool IsFixedPrice { get; set; }
		/// <summary>
		/// Цена со скидкой
		/// </summary>
		decimal PriceWithDiscount { get; }
		/// <summary>
		/// Скидки
		/// </summary>
		IEnumerable<IReceivedDiscountData> Discounts { get; }
	}
}
