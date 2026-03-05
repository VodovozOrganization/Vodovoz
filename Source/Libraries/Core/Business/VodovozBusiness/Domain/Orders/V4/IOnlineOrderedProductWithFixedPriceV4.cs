namespace VodovozBusiness.Domain.Orders.V4
{
	public interface IOnlineOrderedProductWithFixedPriceV4
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
		/// Скидка в деньгах?
		/// </summary>
		bool IsDiscountInMoney { get; set; }
		/// <summary>
		/// Скидка
		/// </summary>
		decimal Discount { get; set; }
		/// <summary>
		/// Id промонабора
		/// </summary>
		int? PromoSetId { get; }
		/// <summary>
		/// Id скидки/промокода
		/// </summary>
		int? DiscountReasonId { get; set; }
	}
}
