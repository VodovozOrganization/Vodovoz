using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using CustomerOrdersApi.Library.V5.Dto.Orders.Discounts;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	/// <summary>
	/// Онлайн пакет аренды
	/// </summary>
	public class OnlineRentPackageDto
	{
		/// <summary>
		/// Id пакета аренды
		/// </summary>
		public int RentPackageId { get; set; }
		/// <summary>
		/// Стоимость аренды
		/// </summary>
		public decimal Price { get; set; }
		/// <summary>
		/// Количество
		/// </summary>
		public int Count { get; set; }
		/// <summary>
		/// Стоимость без скидки
		/// </summary>
		public decimal? PriceWithoutDiscount { get; set; }
		/// <summary>
		/// Сумма позиции
		/// </summary>
		public decimal Sum
		{
			get
			{
				var moneyDiscount = Discounts.Sum(x => x.IsDiscountInMoney ? x.Discount : Price * Count * x.Discount / 100);
				var rawSum = Count * Price;
				
				if(moneyDiscount > rawSum)
				{
					moneyDiscount = rawSum;
				}
				
				return rawSum - moneyDiscount;
			}
		}
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
		/// <summary>
		/// Скидки
		/// </summary>
		public IEnumerable<DiscountDto> Discounts { get; set; }
	}
}
