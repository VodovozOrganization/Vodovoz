using CustomerOrders.Abstractions.V8.Carts;
using CustomerOrders.Abstractions.V8.Sale;
using CustomerOrders.Contracts.V8.Sale;
using System.Collections.Generic;

namespace CustomerOrders.Contracts.V8.Orders.Items
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public class OnlineOrderItemWithDiscountDetailsDto : OnlineOrderItemBaseDto, IOrderedCartItemWithDiscountDetails
	{
		/// <inheritdoc/>
		public IList<IDiscountAmount> Discounts { get; set; }

		public void AddFixedPrice(decimal fixedPrice)
		{
			if(PriceWithoutDiscount is null)
			{
				PriceWithoutDiscount = fixedPrice;
			}
			
			Price = fixedPrice;
			IsFixedPrice = true;
		}

		public static OnlineOrderItemWithDiscountDetailsDto Create(IOrderedCartItem onlineOrderedItem)
		{
			var discounts = new List<IDiscountAmount>();

			if(onlineOrderedItem.DiscountIds != null)
			{
				foreach(var discountId in onlineOrderedItem.DiscountIds)
				{
					discounts.Add(DiscountAmount.Create(discountId));
				}
			}
			
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = onlineOrderedItem.ErpId,
				Count = onlineOrderedItem.Count,
				Price = onlineOrderedItem.Price,
				CurrentPrice = onlineOrderedItem.CurrentPrice,
				PriceWithoutDiscount = onlineOrderedItem.PriceWithoutDiscount,
				CurrentSum = onlineOrderedItem.CurrentSum,
				IsFixedPrice = onlineOrderedItem.IsFixedPrice,
				ItemType = onlineOrderedItem.ItemType,
				Discounts = discounts
			};
		}
	}
}
