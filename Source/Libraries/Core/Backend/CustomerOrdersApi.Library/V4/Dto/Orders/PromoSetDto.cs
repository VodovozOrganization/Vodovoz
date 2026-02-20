namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	public class PromoSetDto
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

		public static PromoSetDto Create(
			int promoSetId,
			int count,
			decimal price)
		{
			return new PromoSetDto
			{
				PromoSetId = promoSetId,
				Count = count,
				Price = price
			};
		}
	}
}
