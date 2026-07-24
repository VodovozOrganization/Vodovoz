using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Tools.Orders
{
	public class OnlineOrderStateKey : ComparerDeliveryPrice
	{
		private OnlineOrder OnlineOrder { get; set; }

		public override void InitializeFields(OnlineOrder onlineOrder)
		{
			OnlineOrder = onlineOrder;
			DeliveryDate = onlineOrder.DeliveryDate;

			var onlineOrderV2 = onlineOrder.As<OnlineOrderV2>();

			if(onlineOrderV2 is null)
			{
				CalculateAllWaterCount(OnlineOrder.OnlineOrderItems);
			}
			else
			{
				CalculateAllWaterCount(GetOnlineOrderV2Items(onlineOrderV2));
			}
		}

		private IList<IProduct> GetOnlineOrderV2Items(OnlineOrderV2 onlineOrderV2)
		{
			var products = new List<IProduct>();
			products.AddRange(OnlineOrder.OnlineOrderItems);

			foreach(var onlineOrderPromoSet in onlineOrderV2.PromoSets)
			{
				var promoSet = onlineOrderPromoSet.PromoSet;

				if(promoSet is null)
				{
					continue;
				}

				products
					.AddRange(promoSet.PromotionalSetItems
						.Select(promoSetItem => OnlineOrderItem.Create(
							promoSetItem.Nomenclature.Id,
							promoSetItem.Count * onlineOrderPromoSet.Count,
							promoSetItem.IsDiscountInMoney,
							false,
							promoSetItem.IsDiscountInMoney ? promoSetItem.DiscountMoney : promoSetItem.Discount,
							promoSetItem.Price(),
							promoSet.Id,
							new List<DiscountReason>(),
							promoSetItem.Nomenclature,
							promoSet,
							onlineOrderV2)
						)
					);
			}
			
			return products;
		}
	}
}
