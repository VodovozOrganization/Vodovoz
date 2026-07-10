using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Tools.Orders
{
	public abstract class DeliveryDateComparerDeliveryPrice : ComparerDeliveryPrice
	{
		public DateTime? DeliveryDate { get; protected set; }
		
		protected virtual void Initialize(IEnumerable<IGoods> products, DateTime? deliveryDate)
		{
			DeliveryDate = deliveryDate;
			CalculateAllWaterCount(products);
			
			Initialized = true;
		}
		
		protected virtual void CalculateAllWaterCount(IEnumerable<IGoods> products)
		{
			ResetCounts();
			CalculatePromoSetWaterCount(products);
			CalculateNotPromoSetWaterCount(products);
		}

		protected virtual void CalculatePromoSetWaterCount(IEnumerable<IGoods> products)
		{
			var water = products.Where(
					x => x.PromoSet != null &&
						x.Nomenclature != null &&
						x.Nomenclature.Category == NomenclatureCategory.water)
				.ToList();

			foreach(var item in water)
			{
				if(item.PromoSet.BottlesCountForCalculatingDeliveryPrice.HasValue)
				{
					NotDisposableWater19LCount = item.PromoSet.BottlesCountForCalculatingDeliveryPrice.Value;
					break;
				}
				
				CalculateWaterCount(item);
			}
		}

		protected virtual void CalculateNotPromoSetWaterCount(IEnumerable<IGoods> products)
		{
			var water = products.Where(
					x => x.PromoSet == null
						&& !x.DiscountReasons.Any(r => r.IsPresent)
						&& x.Nomenclature != null
						&& x.Nomenclature.Category == NomenclatureCategory.water)
				.ToList();

			foreach(var item in water)
			{
				CalculateWaterCount(item);
			}
		}
	}
}
