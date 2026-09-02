using System.Collections.Generic;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Sale;

namespace Vodovoz.Core.Data.Sale
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

			foreach(var discountId in onlineOrderedItem.DiscountIds)
			{
				discounts.Add(DiscountAmount.Create(discountId));
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
