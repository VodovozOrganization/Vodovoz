using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Service;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Services.Sale;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class GoodsPriceCalculator : IGoodsPriceCalculator
	{
		private readonly IGoodsCountCalculator _goodsCountCalculator;

		public GoodsPriceCalculator(
			IGoodsCountCalculator goodsCountCalculator
			)
		{
			_goodsCountCalculator = goodsCountCalculator ?? throw new ArgumentNullException(nameof(goodsCountCalculator));
		}
		
		public (SaleItemPriceType PriceType, decimal Price) CalculateItemPrice(
			IEnumerable<ISaleItem> saleItemsWithCurrent,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			ISaleItem currentSaleItem,
			bool hasPermissionsForAlternativePrice)
		{
			//TODO-5967 действительно ли для фиксы мы считаем воду из промонаборов?
			var fixedPrice = GetFixedPriceOrNull(
				deliveryPoint,
				counterparty,
				currentSaleItem,
				_goodsCountCalculator.GetTotalWater19LCount(saleItemsWithCurrent, doNotCalculatePresentsDiscount: true));

			if(fixedPrice != null)
			{
				return fixedPrice.Value;
			}

			var count = currentSaleItem.PromoSet is null
				? _goodsCountCalculator.GetTotalWater19LCount(saleItemsWithCurrent, true, true)
				: currentSaleItem.Count;

			var canApplyAlternativePrice =
				hasPermissionsForAlternativePrice
				&& currentSaleItem.Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			return currentSaleItem.Nomenclature.GetPrice(count, canApplyAlternativePrice);
		}
		
		public (SaleItemPriceType PriceType, decimal Price) CalculateItemPrice(
			IEnumerable<ISaleItem> saleItemsWithoutNew,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IGetFixedPrice newSaleItem,
			bool hasPermissionsForAlternativePrice)
		{
			var fixedPrice = GetFixedPriceOrNull(
				deliveryPoint,
				counterparty,
				newSaleItem,
				_goodsCountCalculator.GetTotalWater19LCount(saleItemsWithoutNew, doNotCalculatePresentsDiscount: true) + newSaleItem.Count);

			if(fixedPrice != null)
			{
				return fixedPrice.Value;
			}

			var count = newSaleItem.PromoSet is null
				? _goodsCountCalculator.GetTotalWater19LCount(saleItemsWithoutNew, true, true)
				: newSaleItem.Count;

			var canApplyAlternativePrice =
				hasPermissionsForAlternativePrice
				&& newSaleItem.Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			return newSaleItem.Nomenclature.GetPrice(count, canApplyAlternativePrice);
		}
		
		public (SaleItemPriceType PriceType, decimal Price) GetPriceByTotalCount(
			IEnumerable<ISaleItem> allSaleItems,
			INomenclatureCount saleItem,
			bool hasPermissionsForAlternativePrice,
			bool doNotCalculateWaterFromPromoSets = true,
			bool doNotCalculatePresentsDiscount = true
			)
		{
			var nomenclature = saleItem.Nomenclature;
			
			if(nomenclature != null)
			{
				var curCount = nomenclature.IsWater19L
					? _goodsCountCalculator.GetTotalWater19LCount(allSaleItems, doNotCalculateWaterFromPromoSets, doNotCalculatePresentsDiscount)
					: saleItem.Count;
				
				var canApplyAlternativePrice =
					hasPermissionsForAlternativePrice
					&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= curCount);

				if(nomenclature.DependsOnNomenclature == null)
				{
					return nomenclature.GetPrice(curCount, canApplyAlternativePrice);
				}

				if(nomenclature.IsWater19L)
				{
					return nomenclature.DependsOnNomenclature.GetPrice(curCount, canApplyAlternativePrice);
				}
			}
			
			return (SaleItemPriceType.General, 0m);
		}

		private (SaleItemPriceType PriceType, decimal Price)? GetFixedPriceOrNull(
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IEnumerable<ISaleItem> allSaleItems,
			ISaleItem saleItem
		)
		{
			var bottlesCount = _goodsCountCalculator.TotalItemCount(saleItem, allSaleItems);
			return GetFixedPriceOrNull(deliveryPoint, counterparty, saleItem, bottlesCount);
		}

		private (SaleItemPriceType PriceType, decimal Price)? GetFixedPriceOrNull(
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IGetFixedPrice saleItem,
			decimal bottlesCount
			)
		{
			IList<NomenclatureFixedPrice> fixedPrices;

			if(saleItem.PromoSet != null)
			{
				return null;
			}

			//TODO-5967 и проверка на скидку, ведь может прийти без фиксы, но и без скидки и мы должны взять фиксу в этом случае
			/*if(!currentProduct.IsFixedPrice)
			{
				return null;
			}*/
			
			if(deliveryPoint is null)
			{
				if(counterparty is null)
				{
					return null;
				}

				fixedPrices = counterparty.NomenclatureFixedPrices;
			}
			else
			{
				fixedPrices = deliveryPoint.NomenclatureFixedPrices;
			}

			var influentialNomenclature = saleItem.Nomenclature.DependsOnNomenclature;
			decimal? fixedPrice = null;

			if(influentialNomenclature is null
				&& fixedPrices.Any(x =>
					x.Nomenclature.Id == saleItem.Nomenclature.Id
					&& bottlesCount >= x.MinCount))
			{
				fixedPrice = fixedPrices
					.OrderBy(x=> x.MinCount)
					.Last(x => x.Nomenclature.Id == saleItem.Nomenclature.Id && bottlesCount >= x.MinCount)
					.Price;
			}

			if(influentialNomenclature != null
				&& fixedPrices.Any(x =>
					x.Nomenclature.Id == influentialNomenclature.Id
					&& bottlesCount >= x.MinCount))
			{
				fixedPrice = fixedPrices
					.OrderBy(x => x.MinCount)
					.Last(x => x.Nomenclature.Id == influentialNomenclature.Id && bottlesCount >= x.MinCount)
					.Price;
			}

			return fixedPrice.HasValue
				? (SaleItemPriceType.Fixed, fixedPrice.Value)
				: null;
		}
	}
}
