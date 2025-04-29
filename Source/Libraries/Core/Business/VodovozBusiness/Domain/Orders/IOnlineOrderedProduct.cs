namespace VodovozBusiness.Domain.Orders
{
	public interface IOnlineOrderedProduct
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		int NomenclatureId { get; }
		/// <summary>
		/// Цена
		/// </summary>
		decimal Price { get; }
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
		/// <summary>
		/// Расчет скидки в деньгах
		/// </summary>
		/// <returns>Скидка в деньгах</returns>
		decimal MoneyDiscount { get; }
		/// <summary>
		/// Расчет скидки в процентах
		/// </summary>
		/// <returns>Скидка в процентах</returns>
		decimal PercentDiscount { get; }
	}
}
