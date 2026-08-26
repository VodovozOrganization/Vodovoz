using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Domain.Orders
{
	public class NewOrderSaleItem : IGetFixedPrice
	{
		private readonly decimal _count;

		private NewOrderSaleItem(
			Nomenclature nomenclature,
			decimal count,
			(SaleItemPriceType PriceType, decimal Price) priceData = default,
			decimal discount = 0,
			bool isDiscountInMoney = false,
			IEnumerable<DiscountReason> discountReasons = null,
			PromotionalSet promoSet = null,
			Equipment equipment = null,
			bool giftItem = false
		)
		{
			Nomenclature = nomenclature;
			_count = count;
			PriceData = priceData;
			Discount = discount;
			IsDiscountInMoney = isDiscountInMoney;
			DiscountReasons = discountReasons;
			PromoSet = promoSet;
			Equipment = equipment;
			GiftItem = giftItem;
		}
		
		public Nomenclature Nomenclature { get; }

		public decimal Count
		{
			get => _count;
			set => throw new InvalidOperationException("Нельзя устанавливать количество для новой позиции не через конструктор");
		}

		public (SaleItemPriceType PriceType, decimal Price) PriceData { get; set; }
		public decimal Discount { get; }
		public bool IsDiscountInMoney { get; }
		//public bool needGetFixedPrice = true,
		public IEnumerable<DiscountReason> DiscountReasons { get; set; }
		public PromotionalSet PromoSet { get; }
		public Equipment Equipment { get; }
		public bool GiftItem { get; }

		public static NewOrderSaleItem Create(
			Nomenclature nomenclature,
			decimal count,
			(SaleItemPriceType PriceType, decimal Price) priceData = default,
			decimal discount = 0,
			bool isDiscountInMoney = false,
			IEnumerable<DiscountReason> discountReasons = null,
			PromotionalSet promoSet = null,
			Equipment equipment = null,
			bool giftItem = false) =>
				new NewOrderSaleItem(nomenclature, count, priceData, discount, isDiscountInMoney, discountReasons, promoSet, equipment, giftItem);
	}
}
