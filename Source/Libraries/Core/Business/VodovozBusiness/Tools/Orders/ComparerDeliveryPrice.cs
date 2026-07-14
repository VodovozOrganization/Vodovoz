using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Tools.Orders
{
	public abstract class ComparerDeliveryPrice : IWaterCount
	{
		protected bool Initialized;
		
		[Display(Name = "Сколько воды многооборотной таре 19л?")]
		public decimal NotDisposableWater19LCount { get; protected set; }

		[Display(Name = "Сколько воды одноразовой таре 19л?")]
		public decimal DisposableWater19LCount { get; private set; }

		[Display(Name = "Сколько воды одноразовой таре 6л?")]
		public decimal DisposableWater6LCount { get; private set; }

		[Display(Name = "Сколько воды одноразовой таре 1.5л?")]
		public decimal DisposableWater1500mlCount { get; private set; }

		[Display(Name = "Сколько воды одноразовой таре 0.6л?")]
		public decimal DisposableWater600mlCount { get; private set; }

		[Display(Name = "Сколько воды одноразовой таре 0.5л?")]
		public decimal DisposableWater500mlCount { get; private set; }

		public virtual bool CompareWithDeliveryPriceRule(IDeliveryPriceRule rule)
		{
			if(!Initialized)
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
		
		protected virtual void ResetCounts()
		{
			DisposableWater19LCount = 0;
			NotDisposableWater19LCount = 0;
			DisposableWater6LCount = 0;
			DisposableWater1500mlCount = 0;
			DisposableWater600mlCount = 0;
			DisposableWater500mlCount = 0;
		}
		
		protected virtual void CalculateWaterCount(INomenclatureCount item)
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
