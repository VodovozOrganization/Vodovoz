namespace CustomerOrdersApi.Library.Dto.Orders
{
	/// <summary>
	/// Позиция онлайн заказа
	/// </summary>
	public class OnlineOrderItemDto
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int NomenclatureId { get; set; }
		
		
		/// <summary>
		///  id основания скидки из справочника основания скидок
		/// </summary>
		public int? DiscountBasisId { get; set; }

		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; set; }

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
	}
}
