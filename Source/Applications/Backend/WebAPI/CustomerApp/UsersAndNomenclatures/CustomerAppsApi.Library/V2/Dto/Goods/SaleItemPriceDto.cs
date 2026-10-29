using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Dto для отправки данных о ценах в ИПЗ
	/// </summary>
	public class SaleItemPriceDto
	{
		/// <summary>
		/// Идентификатор сущности с ценой
		/// </summary>
		[JsonIgnore]
		public int SaleItemId { get; set; }
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

		public static SaleItemPriceDto CreatePromoSetItem(int saleItemId, decimal price, decimal? priceWithoutDiscount = null)
		{
			return new SaleItemPriceDto
			{
				SaleItemId = saleItemId,
				MinCount = 1,
				Price = price,
				PriceWithoutDiscount = priceWithoutDiscount
			};
		}
	}
}
