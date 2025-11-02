using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Tools.Orders
{
	public abstract class ComparerDeliveryPrice
	{
		public DateTime? DeliveryDate { get; protected set; }
		
		[Display(Name = "Сколько воды многооборотной таре 19л?")]
		decimal NotDisposableWater19LCount { get; set; }

		[Display(Name = "Сколько воды одноразовой таре 19л?")]
		decimal DisposableWater19LCount { get; set; }

		[Display(Name = "Сколько воды одноразовой таре 6л?")]
		decimal DisposableWater6LCount { get; set; }

		[Display(Name = "Сколько воды одноразовой таре 1.5л?")]
		decimal DisposableWater1500mlCount { get; set; }

		[Display(Name = "Сколько воды одноразовой таре 0.6л?")]
		decimal DisposableWater600mlCount { get; set; }

		[Display(Name = "Сколько воды одноразовой таре 0.5л?")]
		decimal DisposableWater500mlCount { get; set; }

		public virtual void InitializeFields(Order order, OrderStatus? requiredStatus = null)
		{
			
		}
		
		public virtual void InitializeFields(OnlineOrder onlineOrder)
		{
			
		}

		public virtual bool CompareWithDeliveryPriceRule(IDeliveryPriceRule rule)
		{
			var totalWater19LCount = DisposableWater19LCount + NotDisposableWater19LCount;
			var deliveryIsFree =
				(totalWater19LCount > 0 && totalWater19LCount >= rule.Water19LCount)
				|| (DisposableWater6LCount > 0 && DisposableWater6LCount >= rule.Water6LCount)
				|| (DisposableWater1500mlCount > 0 && DisposableWater1500mlCount >= rule.Water1500mlCount)
				|| (DisposableWater600mlCount > 0 && DisposableWater600mlCount >= rule.Water600mlCount)
				|| (DisposableWater500mlCount > 0 && DisposableWater500mlCount >= rule.Water500mlCount);

			return !deliveryIsFree;
		}
		
		protected virtual void CalculateAllWaterCount(IEnumerable<IProduct> products)
		{
			ResetCounts();
			CalculatePromoSetWaterCount(products);
			CalculateNotPromoSetWaterCount(products);
		}
		
		protected virtual void ResetCounts()
		{
			DisposableWater19LCount = default;
			NotDisposableWater19LCount = default;
			DisposableWater6LCount = default;
			DisposableWater1500mlCount = default;
			DisposableWater600mlCount = default;
			DisposableWater500mlCount = default;
		}

		protected virtual void CalculatePromoSetWaterCount(IEnumerable<IProduct> products)
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

		protected virtual void CalculateNotPromoSetWaterCount(IEnumerable<IProduct> products)
		{
			var water = products.Where(
					x => x.PromoSet == null
						&& x.DiscountReason?.IsPresent != true
						&& x.Nomenclature != null
						&& x.Nomenclature.Category == NomenclatureCategory.water)
				.ToList();

			foreach(var item in water)
			{
				CalculateWaterCount(item);
			}
		}

		protected virtual void CalculateWaterCount(IProduct item)
		{
			switch(item.Nomenclature.TareVolume)
			{
				case TareVolume.Vol19L:
					if(item.Nomenclature.IsDisposableTare)
					{
						DisposableWater19LCount += item.Count;
					}
					else
					{
						NotDisposableWater19LCount += item.Count;
					}
					break;
				case TareVolume.Vol6L:
					DisposableWater6LCount += item.Count;
					break;
				case TareVolume.Vol1500ml:
					DisposableWater1500mlCount += item.Count;
					break;
				case TareVolume.Vol600ml:
					DisposableWater600mlCount += item.Count;
					break;
				case TareVolume.Vol500ml:
					DisposableWater500mlCount += item.Count;
					break;
			}
		}
	}
}
