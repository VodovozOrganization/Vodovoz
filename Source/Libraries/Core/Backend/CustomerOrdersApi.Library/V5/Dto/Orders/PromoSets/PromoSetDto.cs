namespace CustomerOrdersApi.Library.V5.Dto.Orders.PromoSets
{
	/// <summary>
	/// Данные по промонабору
	/// </summary>
	public abstract class PromoSetDto
	{
		/// <summary>
		/// Id промонабора в Erp
		/// </summary>
		public int PromoSetId { get; set; }
		
		/// <summary>
		/// Количество промонаборов
		/// </summary>
		public int Count { get; set; }
		
		/// <summary>
		/// Цена промика
		/// </summary>
		public decimal Price { get; set; }
	}
}
