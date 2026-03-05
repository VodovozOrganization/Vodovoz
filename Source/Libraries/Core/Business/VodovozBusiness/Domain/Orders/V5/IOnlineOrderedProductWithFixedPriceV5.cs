using System.Collections.Generic;
using Vodovoz.Core.Data.Orders.V5;

namespace VodovozBusiness.Domain.Orders.V5
{
	public interface IOnlineOrderedProductWithFixedPriceV5
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		int NomenclatureId { get; }
		/// <summary>
		/// Старая цена
		/// </summary>
		decimal OldPrice { get; }
		/// <summary>
		/// Новая цена(фикса)
		/// </summary>
		decimal? NewPrice { get; }
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Id промонабора
		/// </summary>
		int? PromoSetId { get; }
		/// <summary>
		/// Скидки, привязанные к товару
		/// </summary>
		IEnumerable<DiscountData> Discounts { get; }
	}
}
