using CustomerOrders.Abstractions.V8.Sale;
using CustomerOrders.Contracts.V8.Orders.Items;
using CustomerOrdersApi.Library.V8.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using CustomerApp.Contracts.Sale;
using CustomerOrdersApi.Library.V8.Services;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Nodes;
using VodovozBusiness.Services.Sale;

namespace CustomerOrdersApi.Library.V8.Factories
{
	internal class OnlineOrderItemDtoFactory : IOnlineOrderItemDtoFactory
	{
		private readonly IOnlineOrderDiscountHandler _discountController;
		private readonly IGoodsPriceCalculator _goodsPriceCalculator;
		private readonly IGoodsCountCalculator _goodsCountCalculator;

		public OnlineOrderItemDtoFactory(
			IOnlineOrderDiscountHandler discountController,
			IGoodsPriceCalculator goodsPriceCalculator,
			IGoodsCountCalculator goodsCountCalculator
			)
		{
			_discountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
			_goodsPriceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			_goodsCountCalculator = goodsCountCalculator ?? throw new ArgumentNullException(nameof(goodsCountCalculator));
		}
		
		public OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(IProduct saleItem, GoodsOnlineParameterType parameterType)
		{
			var onlineParameters = saleItem.Nomenclature.NomenclatureOnlineParameters
				.FirstOrDefault(x => x.Type == parameterType);

			var onlinePrice = onlineParameters?.GetOnlinePrice(saleItem.Count);
			
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = saleItem.Nomenclature.Id,
				Count = saleItem.Count,
				Price = saleItem.Price,
				CurrentPrice = Math.Round(saleItem.ActualSum / saleItem.Count, 2),
				PriceWithoutDiscount = onlinePrice?.PriceWithoutDiscount,
				CurrentSum = saleItem.ActualSum,
				IsFixedPrice = saleItem.IsFixedPrice,
				GiftItem = saleItem.GiftItem,
				ItemType = saleItem.Nomenclature.Category.ToSaleItemType(),
				Discounts = new List<IDiscountAmount>(_discountController
					.CalculateTotalDiscountDetails(saleItem as ICalculatingTotalMoneyDiscount)
					.DiscountDetails
					.Values)
			};
		}
		
		public IEnumerable<OnlineOrderItemWithDiscountDetailsDto> CreateWithDiscountDetailsDto(
			IEnumerable<ISaleItem> allSaleItems,
			IEnumerable<NomenclatureFixedPrice> deliveryPointFixedPrices,
			IEnumerable<NomenclatureFixedPrice> counterpartyPointFixedPrices,
			GoodsOnlineParameterType parameterType
		)
		{
			
			return allSaleItems
				.Select(x => CreateWithDiscountDetailsDto(
					allSaleItems, x, deliveryPointFixedPrices, counterpartyPointFixedPrices, parameterType)
				)
				.ToList();
		}
		
		public OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineOrderPromoSet onlinePromoSet)
		{
			var currentPrice = onlinePromoSet.Price;
			var priceWithoutDiscount = onlinePromoSet.PromoSet.SumWithoutDiscount();
			var currentSum = Math.Round(currentPrice * onlinePromoSet.Count, 2);
			
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = onlinePromoSet.PromoSet.Id,
				Count = onlinePromoSet.Count,
				Price = currentPrice,
				CurrentPrice = currentPrice,
				PriceWithoutDiscount = priceWithoutDiscount,
				CurrentSum = currentSum,
				IsFixedPrice = false,
				ItemType = SaleItemType.PromoSet,
				Discounts =  new List<IDiscountAmount>(_discountController
					.CalculateTotalDiscountDetails(onlinePromoSet)
					.DiscountDetails
					.Values)
			};
		}
		
		//TODO-6121 доработать логику по скидкам
		public OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineOrderTemplatePromoSet onlinePromoSet)
		{
			var currentPrice = onlinePromoSet.PromoSet.Sum();
			var priceWithoutDiscount = onlinePromoSet.PromoSet.SumWithoutDiscount();
			var currentSum = Math.Round(currentPrice * onlinePromoSet.Count, 2);
			
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = onlinePromoSet.PromoSet.Id,
				Count = onlinePromoSet.Count,
				Price = currentPrice,
				CurrentPrice = currentPrice,
				PriceWithoutDiscount = priceWithoutDiscount,
				CurrentSum = currentSum,
				IsFixedPrice = false,
				ItemType = SaleItemType.PromoSet,
				Discounts =  new List<IDiscountAmount>(_discountController
					.CalculateTotalDiscountDetails(onlinePromoSet)
					.DiscountDetails
					.Values)
			};
		}
		
		public OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(PromotionalSet promoSet, decimal count)
		{
			var currentPrice = promoSet.Sum();
			var priceWithoutDiscount = promoSet.SumWithoutDiscount();
			var currentSum = Math.Round(promoSet.Sum() * count, 2);
			
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = promoSet.Id,
				Count = count,
				Price = currentPrice,
				CurrentPrice = currentPrice,
				PriceWithoutDiscount = priceWithoutDiscount,
				CurrentSum = currentSum,
				IsFixedPrice = false,
				ItemType = SaleItemType.PromoSet,
				Discounts = new List<IDiscountAmount>()
			};
		}
		
		public IEnumerable<OnlineOrderItemWithDiscountDetailsDto> CreateWithDiscountDetailsDto(IEnumerable<PromotionalSet> promoSets)
		{
			var promoSetsLookup = promoSets.ToLookup(x => x.Id);

			return promoSetsLookup
				.Select(groupedPromoSets => CreateWithDiscountDetailsDto(groupedPromoSets.First(), groupedPromoSets.Count()))
				.ToList();
		}
		
		public OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineFreeRentPackage freeRentPackage)
		{
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = freeRentPackage.Id,
				Count = freeRentPackage.Count,
				Price = freeRentPackage.Price,
				CurrentPrice = freeRentPackage.Price,
				PriceWithoutDiscount = null,
				CurrentSum = freeRentPackage.Sum,
				IsFixedPrice = false,
				ItemType = SaleItemType.RentPackage,
				Discounts = new List<IDiscountAmount>()
			};
		}
		
		public OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineOrderTemplateFreeRentPackage templateRentPackage)
		{
			var rentPackage = templateRentPackage.FreeRentPackage;
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = templateRentPackage.Id,
				Count = templateRentPackage.Count,
				Price = rentPackage.Deposit,
				CurrentPrice = rentPackage.Deposit,
				PriceWithoutDiscount = null,
				CurrentSum = rentPackage.Deposit * templateRentPackage.Count,
				IsFixedPrice = false,
				ItemType = SaleItemType.RentPackage,
				Discounts = new List<IDiscountAmount>()
			};
		}
		
		private OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(
			IEnumerable<ISaleItem> allSaleItems,
			ISaleItem saleItem,
			IEnumerable<NomenclatureFixedPrice> deliveryPointFixedPrices,
			IEnumerable<NomenclatureFixedPrice> counterpartyPointFixedPrices,
			GoodsOnlineParameterType parameterType)
		{
			var priceData = _goodsPriceCalculator.CalculateItemPrice(
				allSaleItems,
				deliveryPointFixedPrices,
				counterpartyPointFixedPrices,
				saleItem,
				false
				);
			
			var onlineParameters = saleItem.Nomenclature.NomenclatureOnlineParameters
				.FirstOrDefault(x => x.Type == parameterType);

			var totalDiscountsData = _discountController
				.CalculateTotalDiscountDetails(CalculatingTotalMoneyDiscountNode.Create(
					saleItem.Count * priceData.Price,
					saleItem.DiscountReasons)
				);

			var onlinePrice = onlineParameters?.GetOnlinePrice(saleItem.Count);
			var currentPrice = saleItem.Count == 0
				? 0m
				: Math.Round(priceData.Price * saleItem.Count - totalDiscountsData.TotalDiscount / saleItem.Count, 2);
			var currentSum = Math.Round(priceData.Price * saleItem.Count - totalDiscountsData.TotalDiscount, 2);
			
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = saleItem.Nomenclature.Id,
				Count = saleItem.Count,
				Price = priceData.Price,
				CurrentPrice = currentPrice,
				PriceWithoutDiscount = onlinePrice?.PriceWithoutDiscount,
				CurrentSum = currentSum,
				IsFixedPrice = saleItem.IsFixedPrice,
				//TODO-6121 уточнить у Кости, нужен ли здесь параметр подарка. Ведь автозаказ не будет повторяться из UI
				GiftItem = false,
				ItemType = saleItem.Nomenclature.Category.ToSaleItemType(),
				Discounts = new List<IDiscountAmount>(totalDiscountsData.DiscountDetails.Values)
			};
		}
	}
}
