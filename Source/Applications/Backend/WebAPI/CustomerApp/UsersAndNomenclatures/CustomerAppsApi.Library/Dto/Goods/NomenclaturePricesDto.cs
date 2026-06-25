namespace CustomerAppsApi.Library.Dto.Goods
{
	/// <summary>
	/// Dto для отправки данных о ценах в ИПЗ
	/// </summary>
	public class NomenclaturePricesDto
	{
		/// <summary>
		/// Минимальное количество
		/// </summary>
		public decimal MinCount { get; set; }
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; set; }
		/// <summary>
		/// Цена без скидки
		/// </summary>
		public decimal? PriceWithoutDiscount { get; set; }
	}
}
