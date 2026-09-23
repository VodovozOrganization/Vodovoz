using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Service;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Domain.Service;
using VodovozBusiness.Services.Sale;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class GoodsPriceCalculator : IGoodsPriceCalculator
	{
		private readonly IGoodsCountCalculator _goodsCountCalculator;
		private readonly IFixedPriceGetter _fixedPriceGetter;

		public GoodsPriceCalculator(
			IGoodsCountCalculator goodsCountCalculator,
			IFixedPriceGetter fixedPriceGetter
			)
		{
			_goodsCountCalculator = goodsCountCalculator ?? throw new ArgumentNullException(nameof(goodsCountCalculator));
			_fixedPriceGetter = fixedPriceGetter ?? throw new ArgumentNullException(nameof(fixedPriceGetter));
		}
		
		public (SaleItemPriceType PriceType, decimal Price) CalculateItemPrice(
			IEnumerable<ISaleItem> saleItemsWithCurrent,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			ISaleItem currentSaleItem,
			bool hasPermissionsForAlternativePrice)
		{
			var fixedPrice = _fixedPriceGetter.GetFixedPriceOrNull(
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
			var fixedPrice = _fixedPriceGetter.GetFixedPriceOrNull(
				deliveryPoint,
				counterparty,
				newSaleItem,
				_goodsCountCalculator.GetTotalWater19LCount(saleItemsWithoutNew, doNotCalculatePresentsDiscount: true) + newSaleItem.Count);

			if(fixedPrice != null)
			{
				return fixedPrice.Value;
			}

			decimal count;

			if(newSaleItem.Nomenclature.Category == NomenclatureCategory.water)
			{
				count = newSaleItem.PromoSet is null
					? _goodsCountCalculator.GetTotalWater19LCount(saleItemsWithoutNew, true, true)
					: newSaleItem.Count;
			}
			else
			{
				count = 1m;
			}

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
		
		public decimal GetMasterServiceTypePrice(
			ServiceDistrict serviceDistrict,
			MasterServiceType masterServiceType,
			DateTime? deliveryDate)
		{
			var serviceDistrictRuleByWeekDay = serviceDistrict.GetWeekDayServiceDistrictRuleByDeliveryDate(deliveryDate)
				.Where(x => x.ServiceType == masterServiceType);

			if(serviceDistrictRuleByWeekDay.Any())
			{
				return serviceDistrictRuleByWeekDay.Single().Price;
			}

			var commonServiceDistrictRule = serviceDistrict.GetCommonServiceDistrictRules()
				.Where(x => x.ServiceType == masterServiceType);

			if(commonServiceDistrictRule.Any())
			{
				return commonServiceDistrictRule.Single().Price;
			}

			return 0;
		}
	}
}
