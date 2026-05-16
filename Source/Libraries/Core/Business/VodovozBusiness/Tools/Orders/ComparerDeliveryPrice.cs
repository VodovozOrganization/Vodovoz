using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Tools.Orders
{
	public abstract class ComparerDeliveryPrice
	{
		private bool _initialized;
		
		public DateTime? DeliveryDate { get; protected set; }
		
		[Display(Name = "Количество воды в многооборотной таре 19л")]
		public decimal NotDisposableWater19LCount { get; protected set; }

		[Display(Name = "Количество воды в одноразовой таре 19л")]
		public decimal DisposableWater19LCount { get; protected set; }

		[Display(Name = "Количество воды в одноразовой таре 6л")]
		public decimal DisposableWater6LCount { get; protected set; }

		[Display(Name = "Количество воды в одноразовой таре 1.5л")]
		public decimal DisposableWater1500mlCount { get; protected set; }

		[Display(Name = "Количество воды в одноразовой таре 0.6л")]
		public decimal DisposableWater600mlCount { get; protected set; }

		[Display(Name = "Количество воды в одноразовой таре 0.5л")]
		public decimal DisposableWater500mlCount { get; protected set; }

		public virtual bool CompareWithDeliveryPriceRule(IDeliveryPriceRule rule)
		{
			if(!_initialized)
			{
				throw new InvalidOperationException($"Не произведена инициализация класса {typeof(ComparerDeliveryPrice)}");
			}

			var totalWater19LCount = DisposableWater19LCount + NotDisposableWater19LCount;
			var deliveryIsFree =
				(totalWater19LCount > 0 && totalWater19LCount >= rule.Water19LCount)
				|| (DisposableWater6LCount > 0 && DisposableWater6LCount >= rule.Water6LCount)
				|| (DisposableWater1500mlCount > 0 && DisposableWater1500mlCount >= rule.Water1500mlCount)
				|| (DisposableWater600mlCount > 0 && DisposableWater600mlCount >= rule.Water600mlCount)
				|| (DisposableWater500mlCount > 0 && DisposableWater500mlCount >= rule.Water500mlCount);

			return !deliveryIsFree;
		}

		protected virtual void Initialize(IEnumerable<IGoods> products, DateTime? deliveryDate)
		{
			DeliveryDate = deliveryDate;
			CalculateAllWaterCount(products);
			
			_initialized = true;
		}
		
		protected virtual void CalculateAllWaterCount(IEnumerable<IGoods> products)
		{
			ResetCounts();
			CalculatePromoSetWaterCount(products);
			CalculateNotPromoSetWaterCount(products);
		}
		
		protected virtual void ResetCounts()
		{
			DisposableWater19LCount = 0;
			NotDisposableWater19LCount = 0;
			DisposableWater6LCount = 0;
			DisposableWater1500mlCount = 0;
			DisposableWater600mlCount = 0;
			DisposableWater500mlCount = 0;
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

		protected virtual void CalculateWaterCount(IGoods item)
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
