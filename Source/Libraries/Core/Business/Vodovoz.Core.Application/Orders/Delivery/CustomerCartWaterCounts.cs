using System.Collections.Generic;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Tools.Orders;
using VodovozBusiness.Domain.Orders.Cart;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	/// <summary>
	/// Класс для расчета стоимости доставки для товаров корзины из ИПЗ
	/// </summary>
	public class CustomerCartWaterCounts : ComparerDeliveryPrice
	{
		public virtual void Initialize(IEnumerable<ICartItem> cartItems)
		{
			CalculateAllWaterCount(cartItems);
			Initialized = true;
		}
		
		private void CalculateAllWaterCount(IEnumerable<ICartItem> cartItems)
		{
			ResetCounts();

			foreach(var cartItem in cartItems)
			{
				switch (cartItem)
				{
					case IPromoSetCartItem promoSetCartItem:
						CalculatePromoSetWaterCount(promoSetCartItem);
						break;
					case INomenclatureCartItem nomenclatureCartItem:
						CalculateWaterCount(nomenclatureCartItem);
						break;
				}
			}
		}

		private void CalculatePromoSetWaterCount(IPromoSetCartItem cartItem)
		{
			var promoSet = cartItem.PromoSet;
			
			if(promoSet.BottlesCountForCalculatingDeliveryPrice.HasValue)
			{
				NotDisposableWater19LCount = promoSet.BottlesCountForCalculatingDeliveryPrice.Value * cartItem.Count;
				return;
			}
			
			foreach(var promoSetItem in promoSet.PromotionalSetItems)
			{
				promoSetItem.Count *= (int)cartItem.Count;
				CalculateWaterCount(promoSetItem);
			}
		}
		
		private void CalculateWaterCount(INomenclatureCartItem cartItem)
		{
			if(cartItem.Nomenclature.Category != NomenclatureCategory.water)
			{
				return;
			}

			base.CalculateWaterCount(cartItem);
		}
	}
}
