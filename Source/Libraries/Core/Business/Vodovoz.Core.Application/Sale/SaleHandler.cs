using System;
using System.Linq;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain.Service;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Extensions;
using VodovozBusiness.Specifications.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class SaleHandler : ISaleHandler
	{
		public SaleHandler(
			SaleItemHandler saleItemHandler,
			IGoodsPriceCalculator goodsPriceCalculator
			)
		{
			GoodsPriceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			SaleItemHandler = saleItemHandler ?? throw new ArgumentNullException(nameof(saleItemHandler));
		}
		
		protected SaleItemHandler SaleItemHandler { get; }
		protected ISaleSource Source { get; set; }
		protected IGoodsPriceCalculator GoodsPriceCalculator { get; }
		
		public void SetSource(ISaleSource source)
		{
			Source = source;
		}
		
		public virtual void Recalculate()
		{
			ThrowIfSaleItemsIsNull();
			RecalculateItemsPrice();
		}
		
		public virtual void RecalculateDiscounts(IDataContext context)
		{
			SaleItemHandler.RecalculateDiscounts(context);
		}
		
		public virtual bool SetCount(INomenclatureCount saleItem, decimal count)
		{
			if(!SaleItemHandler.SetCount(saleItem, count))
			{
				return false;
			}

			Recalculate();
			var discountItem = saleItem.ToApplyDiscountReasonItem();
			RecalculateDiscounts(CommonRecalculateDiscount.CreateDataContext(discountItem));
			return true;
		}
		
		public virtual void SetPrice(
			ISaleItem saleItem,
			(SaleItemPriceType PriceType, decimal Price) priceData
			)
		{
			var priceByTotalCount = GoodsPriceCalculator
				.CalculateItemPrice(
					Source.SaleItems,
					Source.DeliveryPoint,
					Source.Counterparty,
					saleItem,
					Source.HasPermissionsForAlternativePrice);

			var discountItem = saleItem.ToApplyDiscountReasonItem();
			UpdatePriceType(saleItem, priceData, priceByTotalCount, SaleItemSpecification.Create(priceByTotalCount));

			SaleItemHandler.SetPrice(
				CommonRecalculateDiscount.CreateDataContext(discountItem),
				priceData.Price);
		}
		
		public virtual void SetPriceForNewSaleItem(
			ISaleItem newItem,
			(SaleItemPriceType PriceType, decimal Price) priceData
			)
		{
			var priceByTotalCount = GoodsPriceCalculator
				.CalculateItemPrice(
					Source.SaleItems,
					Source.DeliveryPoint,
					Source.Counterparty,
					newItem,
					Source.HasPermissionsForAlternativePrice);
			
			var discountItem = newItem.ToApplyDiscountReasonItem();
			UpdatePriceType(newItem, priceData, priceByTotalCount, SaleItemSpecification.Create(priceByTotalCount));
			
			SaleItemHandler.SetPriceForNewSaleItem(
				CommonRecalculateDiscount.CreateDataContext(discountItem),
				priceData.Price);
		}
		
		public virtual void RecalculatePrice(ISaleItem saleItem)
		{
			if(saleItem.IsUserPrice || saleItem.PromoSet != null)
			{
				return;
			}

			SetPrice(
				saleItem,
				GoodsPriceCalculator.CalculateItemPrice(
					Source.SaleItems,
					Source.DeliveryPoint,
					Source.Counterparty,
					saleItem,
					Source.HasPermissionsForAlternativePrice
				)
			);
		}

		protected virtual void RecalculateItemsPrice()
		{
			foreach(var saleItem in Source.SaleItems.ToList())
			{
				if(saleItem.Nomenclature.Category == NomenclatureCategory.water)
				{
					RecalculatePrice(saleItem);
				}
			}
		}

		protected void ThrowIfSaleItemsIsNull()
		{
			if(Source?.SaleItems is null)
			{
				throw new InvalidOperationException("SaleItems cannot be null.");
			}
		}
		
		protected virtual void UpdatePriceType(
			ISaleItem saleItem,
			(SaleItemPriceType PriceType, decimal Price) receivedPriceData,
			(SaleItemPriceType PriceType, decimal Price) calculatedPriceData,
			ISpecificationTwoArgs specification)
		{
			switch(receivedPriceData.PriceType)
			{
				case SaleItemPriceType.Fixed:
					saleItem.IsFixedPrice = true;
					saleItem.IsUserPrice = false;
					saleItem.IsAlternativePrice = false;
					break;
				case SaleItemPriceType.User:
					CheckUserPrice(saleItem, receivedPriceData, specification);
					break;
				case SaleItemPriceType.Alternative:
					saleItem.IsAlternativePrice = true;
					saleItem.IsUserPrice = false;
					saleItem.IsFixedPrice = false;
					break;
				default:
					CheckUserPrice(saleItem, receivedPriceData, specification);
					break;
			}
		}

		private static void CheckUserPrice(
			ISaleItem saleItem,
			(SaleItemPriceType PriceType, decimal Price) receivedPriceData,
			ISpecificationTwoArgs specification
			)
		{
			if(specification.IsSatisfiedBy(saleItem, receivedPriceData))
			{
				saleItem.IsUserPrice = true;
				saleItem.IsFixedPrice = false;
				saleItem.IsAlternativePrice = false;
			}
			else
			{
				saleItem.IsUserPrice = false;
				saleItem.IsFixedPrice = false;
				saleItem.IsAlternativePrice = false;
			}
		}
	}
}
