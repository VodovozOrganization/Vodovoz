using System.Collections.Generic;
using Vodovoz.Core.Domain.Goods;

namespace VodovozBusiness.Domain.Orders
{
	public interface IOnlineOrderedProductWithFixedPriceV7
	{
		/// <summary>
		/// Id товара/услуги ДВ
		/// </summary>
		int ErpId { get; }
		/// <summary>
		/// Тип товара/услуги
		/// </summary>
		SaleItemType ItemType { get; }
		/// <summary>
		/// Цена без скидки
		/// </summary>
		decimal? PriceWithoutDiscount { get; }
		/// <summary>
		/// Текущая цена(или фикса, или переданная цена)
		/// </summary>
		decimal Price { get; }
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Скидки
		/// </summary>
		IEnumerable<int> DiscountIds { get; }
	}
}
