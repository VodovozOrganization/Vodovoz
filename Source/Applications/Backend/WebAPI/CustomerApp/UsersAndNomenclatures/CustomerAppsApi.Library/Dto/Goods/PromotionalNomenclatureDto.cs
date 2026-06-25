namespace CustomerAppsApi.Library.Dto.Goods
{
	/// <summary>
	/// Номенклатура промонабора
	/// </summary>
	public class PromotionalNomenclatureDto
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int ErpNomenclatureId { get; set; }
		/// <summary>
		/// Количество
		/// </summary>
		public int Count { get; set; }
		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; set; }
		/// <summary>
		/// Скидка в рублях
		/// </summary>
		public bool IsDiscountMoney { get; set; }
	}
}
