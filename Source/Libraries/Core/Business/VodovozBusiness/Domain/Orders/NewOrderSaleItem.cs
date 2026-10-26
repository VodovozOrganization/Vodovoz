using System.Collections.Generic;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public class NewOrderSaleItem : NewSaleItem
	{
		private NewOrderSaleItem(
			Nomenclature nomenclature,
			decimal count,
			(SaleItemPriceType PriceType, decimal Price) priceData = default,
			decimal discount = 0,
			bool isDiscountInMoney = false,
			IEnumerable<DiscountReasonBase> discountReasons = null,
			PromotionalSet promoSet = null,
			Equipment equipment = null,
			bool giftItem = false
		) : base(nomenclature, count, priceData, discount, isDiscountInMoney, discountReasons, promoSet, giftItem)
		{
			Equipment = equipment;
		}
		
		public Equipment Equipment { get; }

		public static NewOrderSaleItem Create(
			Nomenclature nomenclature,
			decimal count,
			(SaleItemPriceType PriceType, decimal Price) priceData = default,
			decimal discount = 0,
			bool isDiscountInMoney = false,
			IEnumerable<DiscountReasonBase> discountReasons = null,
			PromotionalSet promoSet = null,
			Equipment equipment = null,
			bool giftItem = false) =>
			new NewOrderSaleItem(nomenclature, count, priceData, discount, isDiscountInMoney, discountReasons, promoSet, equipment, giftItem);
	}
}
