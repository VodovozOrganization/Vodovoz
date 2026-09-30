namespace VodovozBusiness.Domain.Orders
{
	public interface ICanApplyFixedPriceOnline
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		int NomenclatureId { get; }
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Id промонабора
		/// </summary>
		int? PromoSetId { get; }
		/// <summary>
		/// Цена со скидкой
		/// </summary>
		decimal PriceWithDiscount { get; }
	}
}
