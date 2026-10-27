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
		
		protected virtual void Initialize(IEnumerable<ISaleItem> saleItems, DateTime? deliveryDate)
		{
			DeliveryDate = deliveryDate;
			CalculateAllWaterCount(saleItems);
			
			Initialized = true;
		}
		
		protected virtual void CalculateAllWaterCount(IEnumerable<ISaleItem> saleItems)
		{
			ResetCounts();
			CalculatePromoSetWaterCount(saleItems);
			CalculateNotPromoSetWaterCount(saleItems);
		}

		protected virtual void CalculatePromoSetWaterCount(IEnumerable<ISaleItem> saleItems)
		{
			var water = saleItems.Where(
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

		protected virtual void CalculateNotPromoSetWaterCount(IEnumerable<ISaleItem> saleItems)
		{
			var water = saleItems.Where(
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
