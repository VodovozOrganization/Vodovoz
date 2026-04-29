using System;
using System.Text.Json.Serialization;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Dto.Orders.OrderItem
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public class OnlineOrderItemDto : OnlineOrderItemBaseDto, IOnlineOrderedProduct
	{
		[JsonIgnore]
		public decimal PriceWithDiscount
		{
			get
			{
				if(Discount > 0)
				{
					if(Count == 0)
					{
						return 0;
					}

					return !IsDiscountInMoney
						? Math.Round(Price * (100 - Discount) / 100, 2)
						: Math.Round((Price * Count - Discount) / Count, 2);
				}

				return Price;
			}
		}

		public void ClearDiscount()
		{
			Discount = 0;
			IsDiscountInMoney = false;
			DiscountReasonId = null;
		}
	}
}
