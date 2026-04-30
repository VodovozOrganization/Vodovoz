using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V5.Orders.OrderItem
{
	/// <summary>
	/// Проверенный товар онлайн заказа из корзины
	/// </summary>
	public class CheckedOnlineOrderItemDto : OnlineOrderItemBaseDto
	{
		/// <summary>
		/// Статус товара <see cref="CartItemStatus"/>
		/// </summary>
		public CartItemStatus Status { get; set; }
		/// <summary>
		/// Цена без скидки
		/// </summary>
		public decimal? PriceWithoutDiscount { get; set; }
		/// <summary>
		/// Сумма позиции
		/// </summary>
		public decimal Sum
		{
			get
			{
				decimal moneyDiscount = 0;
				
				if(Discounts != null)
				{
					moneyDiscount += Discounts.Sum(discount => discount.IsDiscountInMoney
						? discount.Discount
						: Price * Count * discount.Discount / 100);
				}
				
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

		public static CheckedOnlineOrderItemDto Create(OnlineOrderItemDto orderingItem, CartItemStatus status = CartItemStatus.Active)
		{
			return new CheckedOnlineOrderItemDto
			{
				NomenclatureId = orderingItem.NomenclatureId,
				Count =  orderingItem.Count,
				Price = orderingItem.Price,
				Discounts = orderingItem.Discounts,
				IsFixedPrice = orderingItem.IsFixedPrice,
				PromoSetId = orderingItem.PromoSetId,
				Status =  status
			};
		}
	}
}
