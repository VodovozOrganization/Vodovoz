using System;
using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Domain.Orders
{
	public class ApplicablePromotion : IApplicablePromotion
	{
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; set; }

		/// <inheritdoc/>
		public decimal CurrentRawPrice => Count * Price;

		/// <inheritdoc/>
		public bool IsFixedPrice { get; set; }

		/// <inheritdoc/>
		public Nomenclature Nomenclature { get; set; }

		/// <inheritdoc/>
		public PromotionalSet PromoSet { get; set; }

		/// <inheritdoc/>
		public IEnumerable<DiscountReasonBase> DiscountReasons { get; set; }

		public static IApplicablePromotion Create(ICalculatingTotalMoneyDiscount saleItem)
		{
			switch(saleItem)
			{
				case OnlineOrderPromoSet onlinePromoSet:
					return Create(onlinePromoSet);
				case OnlineOrderTemplateSaleItem templateSaleItem:
					return Create(templateSaleItem);
				case OnlineOrderTemplatePromoSet templatePromoSet:
					return Create(templatePromoSet);
				default:
					throw new InvalidOperationException(
						$"Для типа {saleItem.GetType()} нет реализации создания IApplicablePromotion. Добавьте ее в метод Create");
			}
		}

		public static IApplicablePromotion Create(OnlineOrderPromoSet onlinePromoSet)
		{
			return new ApplicablePromotion
			{
				Price = onlinePromoSet.Price,
				Count = onlinePromoSet.Count,
				IsFixedPrice = false,
				Nomenclature = null,
				PromoSet = onlinePromoSet.PromoSet,
				DiscountReasons = onlinePromoSet.DiscountReasons
			};
		}

		public static IApplicablePromotion Create(OnlineOrderTemplateSaleItem templateSaleItem)
		{
			return new ApplicablePromotion
			{
				Price = templateSaleItem.Price,
				Count = templateSaleItem.Count,
				IsFixedPrice = templateSaleItem.IsFixedPrice,
				Nomenclature = templateSaleItem.Nomenclature,
				PromoSet = null,
				DiscountReasons = templateSaleItem.DiscountReasons
			};
		}

		public static IApplicablePromotion Create(OnlineOrderTemplatePromoSet templatePromoSet)
		{
			return new ApplicablePromotion
			{
				Price = templatePromoSet.Price,
				Count = templatePromoSet.Count,
				IsFixedPrice = false,
				Nomenclature = null,
				PromoSet = templatePromoSet.PromoSet,
				DiscountReasons = templatePromoSet.DiscountReasons
			};
		}
	}
}
