using System;
using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.V5.Dto.Orders.OrderItem
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
				var moneyDiscount = IsDiscountInMoney ? Discount : Price * Count * Discount / 100;
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
				Discount = orderingItem.Discount,
				DiscountReasonId = orderingItem.DiscountReasonId,
				IsDiscountInMoney = orderingItem.IsDiscountInMoney,
				IsFixedPrice = orderingItem.IsFixedPrice,
				PromoSetId = orderingItem.PromoSetId,
				Status =  status
			};
		}
	}
}
