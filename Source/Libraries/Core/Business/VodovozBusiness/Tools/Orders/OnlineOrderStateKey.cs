using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Tools.Orders
{
	public class OnlineOrderStateKey : DeliveryDateComparerDeliveryPrice
	{
		private OnlineOrder OnlineOrder { get; set; }

		public virtual void InitializeFields(OnlineOrder onlineOrder)
		{
			OnlineOrder = onlineOrder;
			var onlineOrderV2 = onlineOrder.As<OnlineOrderV2>();

			if(onlineOrderV2 is null)
			{
				Initialize(OnlineOrder.OnlineOrderItems, onlineOrder.DeliveryDate);
			}
			else
			{
				Initialize(GetOnlineOrderV2Items(onlineOrderV2), onlineOrder.DeliveryDate);
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

				
				//TODO проверить нужно ли здесь разгонять скидки по позициям промонабора
				products
					.AddRange(promoSet.PromotionalSetItems
						.Select(promoSetItem => OnlineOrderItem.Create(
							promoSetItem.Nomenclature.Id,
							promoSetItem.Count * onlineOrderPromoSet.Count,
							promoSetItem.IsDiscountInMoney,
							false,
							promoSetItem.IsDiscountInMoney ? promoSetItem.DiscountMoney : promoSetItem.Discount,
							promoSetItem.Price().Price,
							promoSet.Id,
							new List<DiscountReasonBase>(),
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
