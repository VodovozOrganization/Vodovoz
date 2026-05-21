using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V5.Orders.OrderItem
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public class OnlineOrderItemDto : OnlineOrderItemBaseDto
	{
		[JsonIgnore]
		public decimal PriceWithDiscount
		{
			get
			{
				if(Discounts != null)
				{
					decimal priceWithoutDiscount = 0;
					
					if(Count == 0)
					{
						return priceWithoutDiscount;
					}

					priceWithoutDiscount = Discounts.Sum(discount => !discount.IsDiscountInMoney
						? Math.Round(Price * (100 - discount.Discount) / 100, 2)
						: Math.Round((Price * Count - discount.Discount) / Count, 2));
					
					return priceWithoutDiscount;
				}

				return Price;
			}
		}
	}
}
