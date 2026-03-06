using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Vodovoz.Core.Data.Orders.V5
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public class OrderTemplateProductDto
	{
		private OrderTemplateProductDto(
			int nomenclatureId, decimal price, decimal count, int? promoSetId, IEnumerable<DiscountData> discounts)
		{
			NomenclatureId = nomenclatureId;
			Price = price;
			Count = count;
			PromoSetId = promoSetId;
			Discounts = discounts;
		}
		
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int NomenclatureId { get; }
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; }
		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; }
		/// <summary>
		/// Id промонабора
		/// </summary>
		public int? PromoSetId { get; }
		/// <summary>
		/// Скидки
		/// </summary>
		public IEnumerable<DiscountData> Discounts { get; }
		
		[JsonIgnore]
		public decimal Sum => Price * Count - CalculateDiscount();

		private decimal CalculateDiscount()
		{
			var discountSum = 0m;

			if(Discounts is null)
			{
				return discountSum;
			}

			foreach(var discountData in Discounts)
			{
				discountSum += !discountData.IsDiscountInMoney
					? Math.Round(Price * Count * (100 - discountData.Discount) / 100, 2)
					: Math.Round(Price * Count - discountData.Discount, 2);
			}

			return discountSum > Price * Count ? Price * Count : discountSum;
		}

		public static OrderTemplateProductDto Create(
			int nomenclatureId,
			decimal price,
			decimal count,
			int? promoSetId,
			IEnumerable<DiscountData> discounts
			) =>
			new OrderTemplateProductDto(nomenclatureId, price, count, promoSetId, discounts);
	}
}
