using System;
using System.Text.Json.Serialization;

namespace Vodovoz.Core.Data.Orders.V5
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public class OrderTemplateProductDto
	{
		private OrderTemplateProductDto(
			int nomenclatureId, decimal price, decimal count, bool isDiscountInMoney, decimal discount, int? promoSetId, int? discountReasonId)
		{
			NomenclatureId = nomenclatureId;
			Price = price;
			Count = count;
			IsDiscountInMoney = isDiscountInMoney;
			Discount = discount;
			PromoSetId = promoSetId;
			DiscountReasonId = discountReasonId;
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
		/// Скидка в деньгах?
		/// </summary>
		public bool IsDiscountInMoney { get; }
		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; }
		/// <summary>
		/// Id промонабора
		/// </summary>
		public int? PromoSetId { get; }
		/// <summary>
		/// Id скидки/промокода
		/// </summary>
		public int? DiscountReasonId { get; }
		
		[JsonIgnore]
		public decimal Sum => Price * Count - CalculateDiscount();

		private decimal CalculateDiscount()
		{
			if(IsDiscountInMoney)
			{
				return Discount > Price * Count
					? Price * Count
					: (Discount < 0 ? 0 : Discount);
			}
			
			return Discount > 100
				? Price * Count
				: Price * Count * Discount / 100;
		}

		public static OrderTemplateProductDto Create(
			int nomenclatureId,
			decimal price,
			decimal count,
			bool isDiscountInMoney,
			decimal discount,
			int? promoSetId,
			int? discountReasonId) =>
			new OrderTemplateProductDto(nomenclatureId, price, count, isDiscountInMoney, discount, promoSetId, discountReasonId);
	}
}
