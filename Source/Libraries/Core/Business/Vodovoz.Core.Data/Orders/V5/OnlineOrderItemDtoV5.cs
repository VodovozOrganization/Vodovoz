using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Vodovoz.Core.Data.Orders.V5
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public class OnlineOrderItemDtoV5 : IOnlineOrderedProductV5
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int NomenclatureId { get; set; }

		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; set; }
		
		/// <summary>
		/// Id промонабора
		/// </summary>
		public int? PromoSetId { get; set; }

		/// <summary>
		/// Скидки, привязанные к товару
		/// </summary>
		public IList<DiscountData> Discounts { get; set; }

		/// <summary>
		/// Фикса
		/// </summary>
		public bool IsFixedPrice { get; set; }
		[JsonIgnore]
		public decimal PriceWithDiscount
		{
			get
			{
				if(Discounts != null && Discounts.Any())
				{
					if(Count == 0)
					{
						return 0;
					}

					var discountSum = 0m;

					foreach(var discountData in Discounts)
					{
						discountSum += !discountData.IsDiscountInMoney
							? Math.Round(Price * (100 - discountData.Discount) / 100, 2)
							: Math.Round((Price * Count - discountData.Discount) / Count, 2);
					}
					
					return discountSum > Price ? 0 : Price - discountSum;
				}

				return Price;
			}
		}

		public void ClearDiscounts()
		{
			foreach(var discountData in Discounts)
			{
				discountData.Discount = 0;
				discountData.IsDiscountInMoney = false;
				discountData.DiscountReasonId = null;
			}
		}

		public decimal Discount()
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
	}
}
