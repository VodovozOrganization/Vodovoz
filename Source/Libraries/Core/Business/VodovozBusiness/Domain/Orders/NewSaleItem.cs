using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Domain.Orders
{
	public class NewSaleItem : IGetFixedPrice
	{
		private readonly decimal _count;

		protected NewSaleItem(
			Nomenclature nomenclature,
			decimal count,
			(SaleItemPriceType PriceType, decimal Price) priceData = default,
			decimal discount = 0,
			bool isDiscountInMoney = false,
			IEnumerable<DiscountReasonBase> discountReasons = null,
			PromotionalSet promoSet = null,
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
			GiftItem = giftItem;
		}
		
		public Nomenclature Nomenclature { get; }

		public decimal Count
		{
			get => _count;
			set => throw new InvalidOperationException("Нельзя устанавливать количество для новой позиции не через конструктор");
		}

		public (SaleItemPriceType PriceType, decimal Price) PriceData { get; set; }
		public decimal Discount { get; private set; }
		public bool IsDiscountInMoney { get; private set; }
		public IEnumerable<DiscountReasonBase> DiscountReasons { get; set; }
		public PromotionalSet PromoSet { get; }
		public bool GiftItem { get; }

		public void UpdateDiscount(
			bool isDiscountInMoney,
			decimal discount,
			IEnumerable<DiscountReasonBase> discountReasons)
		{
			Discount = discount;
			DiscountReasons = discountReasons;
			IsDiscountInMoney = isDiscountInMoney;
		}

		public static NewSaleItem Create(
			Nomenclature nomenclature,
			decimal count,
			(SaleItemPriceType PriceType, decimal Price) priceData = default,
			decimal discount = 0,
			bool isDiscountInMoney = false,
			IEnumerable<DiscountReasonBase> discountReasons = null,
			PromotionalSet promoSet = null,
			bool giftItem = false) =>
			new NewSaleItem(nomenclature, count, priceData, discount, isDiscountInMoney, discountReasons, promoSet, giftItem);
	}
}
