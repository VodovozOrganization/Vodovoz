using VodovozBusiness.Domain.Orders.V4;

namespace VodovozBusiness.Nodes.V4
{
	/// <summary>
	/// Позиция онлайн заказа с фиксой
	/// </summary>
	public class OnlineOrderItemWithFixedPriceV4 :  IOnlineOrderedProductWithFixedPriceV4
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int NomenclatureId { get; set; }
		/// <summary>
		/// Старая цена
		/// </summary>
		public decimal OldPrice { get; set; }
		/// <summary>
		/// Новая цена(фикса)
		/// </summary>
		public decimal? NewPrice { get; set; }
		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; set; }
		/// <summary>
		/// Скидка в деньгах?
		/// </summary>
		public bool IsDiscountInMoney { get; set; }
		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; set; }
		/// <summary>
		/// Id промонабора
		/// </summary>
		public int? PromoSetId { get; set; }
		/// <summary>
		/// Id скидки/промокода
		/// </summary>
		public int? DiscountReasonId { get; set; }
	}
}
