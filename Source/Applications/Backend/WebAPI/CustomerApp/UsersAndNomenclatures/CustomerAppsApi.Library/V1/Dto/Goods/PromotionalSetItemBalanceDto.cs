namespace CustomerAppsApi.Library.V1.Dto.Goods
{
	/// <summary>
	/// Данные по позиции промонабора с остатками на складах
	/// </summary>
	public class PromotionalSetItemBalanceDto
	{
		/// <summary>
		/// Идентификатор промонабора
		/// </summary>
		public int PromotionalSetId { get; set; }
		/// <summary>
		/// Идентификатор номенклатуры
		/// </summary>
		public int NomenclatureId { get; set; }
		/// <summary>
		/// Количество позиции
		/// </summary>
		public int Count { get; set; }
		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; set; }
		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		public bool IsDiscountMoney { get; set; }
		/// <summary>
		/// Баланс на складах
		/// </summary>
		public decimal Stock { get; set; }
	}
}
