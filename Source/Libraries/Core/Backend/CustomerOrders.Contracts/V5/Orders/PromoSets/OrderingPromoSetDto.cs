using System;
using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V5.Orders.PromoSets
{
	/// <summary>
	/// Заказываемый промонабор
	/// </summary>
	public class OrderingPromoSetDto : PromoSetDto
	{
		/// <summary>
		/// Цена без скидки
		/// </summary>
		public decimal? PriceWithoutDiscount { get; set; }
		/// <summary>
		/// Сумма позиции
		/// </summary>
		public decimal Sum => Count * Price;
		/// <summary>
		/// Сумма позиции без скидок
		/// </summary>
		[JsonIgnore]
		public decimal SumWithoutDiscount
		{
			get
			{
				var priceWithoutDiscount = PriceWithoutDiscount ?? Price;
				return Math.Round(priceWithoutDiscount * Count, 2);
			}
		}
	}
}
