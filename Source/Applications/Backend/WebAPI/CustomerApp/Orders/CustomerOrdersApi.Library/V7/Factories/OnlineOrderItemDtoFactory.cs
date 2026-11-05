using CustomerOrders.Abstractions.V7.Sale;
using CustomerOrders.Contracts.V7.Orders.Items;
using CustomerOrdersApi.Library.V7.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using CustomerApp.Contracts.Sale;
using CustomerOrdersApi.Library.V7.Services;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Factories
{
	internal class OnlineOrderItemDtoFactory : IOnlineOrderItemDtoFactory
	{
		private readonly IOnlineOrderDiscountHandler _discountHandler;

		public OnlineOrderItemDtoFactory(IOnlineOrderDiscountHandler discountHandler)
		{
			_discountHandler = discountHandler ?? throw new ArgumentNullException(nameof(discountHandler));
		}
		
		public OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(IProduct saleItem)
		{
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = saleItem.Nomenclature.Id,
				Count = Math.Round(saleItem.Count, 2),
				Price = Math.Round(saleItem.Price, 2),
				CurrentPrice = saleItem.Count == 0
					? 0m
					: Math.Round(saleItem.ActualSum / saleItem.Count, 2),
				PriceWithoutDiscount = null,
				CurrentSum = saleItem.ActualSum,
				IsFixedPrice = saleItem.IsFixedPrice,
				GiftItem = saleItem.GiftItem,
				ItemType = saleItem.Nomenclature.Category.ToSaleItemType(),
				Discounts = new List<IDiscountAmount>(_discountHandler
					.CalculateTotalDiscountDetails(saleItem as ICalculatingTotalMoneyDiscount)
					.DiscountDetails
					.Values)
			};
		}
		
		public IEnumerable<OnlineOrderItemWithDiscountDetailsDto> CreateWithDiscountDetailsDto(
			IEnumerable<(PromotionalSet PromoSet, IEnumerable<IProduct> SaleItems)> promoSetSaleItems
			)
		{
			var list = new List<OnlineOrderItemWithDiscountDetailsDto>();
			
			foreach(var promoSetData in promoSetSaleItems)
			{
				var promoSet = promoSetData.PromoSet;
				const int count = 1;
				var price = promoSetData.SaleItems.Sum(p => p.CurrentCount * p.Price);
				var currentPrice = price - promoSetData.SaleItems
					.Sum(p => p.IsDiscountInMoney ? p.GetDiscount : p.Price * p.CurrentCount * p.GetDiscount / 100);
			
				list.Add(new OnlineOrderItemWithDiscountDetailsDto
				{
					ErpId = promoSet.Id,
					Count = count,
					Price = Math.Round(price, 2),
					CurrentPrice = Math.Round(currentPrice, 2),
					PriceWithoutDiscount = promoSet.SumWithoutDiscount(),
					CurrentSum = Math.Round(currentPrice * count, 2),
					IsFixedPrice = false,
					ItemType = SaleItemType.PromoSet,
					Discounts = new List<IDiscountAmount>()
				});
			}
			
			return list;
		}

		public IEnumerable<OnlineOrderItemWithDiscountDetailsDto> CreateWithDiscountDetailsDto(
			IEnumerable<(PromotionalSet PromoSet, IEnumerable<OrderItem> OrderItems)> promoSetOrderItems
			)
		{
			var list = new List<OnlineOrderItemWithDiscountDetailsDto>();
			
			foreach(var promoSetData in promoSetOrderItems)
			{
				var promoSet = promoSetData.PromoSet;

				const int count = 1;
				var price = promoSetData.OrderItems.Sum(o => o.CurrentRawPrice);
				/*var discountData = _discountController.CalculateTotalDiscountDetails(
					promoSet.PromotionalSetItems,
					promoSetData.OrderItems);*/
				var currentPrice = price - promoSetData.OrderItems.Sum(o => o.DiscountMoney);
			
				list.Add(new OnlineOrderItemWithDiscountDetailsDto
				{
					ErpId = promoSet.Id,
					Count = count,
					Price = Math.Round(price, 2),
					CurrentPrice = Math.Round(currentPrice, 2),
					PriceWithoutDiscount = promoSet.SumWithoutDiscount(),
					CurrentSum = Math.Round(currentPrice * count, 2),
					IsFixedPrice = false,
					ItemType = SaleItemType.PromoSet,
					Discounts = new List<IDiscountAmount>()
				});
			}
			
			return list;
		}

		public OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineOrderPromoSet onlinePromoSet)
		{
			var promoSet = onlinePromoSet.PromoSet;
			var discountData = _discountHandler.CalculateTotalDiscountDetails(onlinePromoSet);
			var price = onlinePromoSet.Price;
			var count = onlinePromoSet.Count;
			var totalSumWithoutDiscount = price * count;
			var currentSum = totalSumWithoutDiscount - discountData.TotalDiscount;
			var currentPrice = count == 0 
				? 0m 
				: currentSum / count;
			
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = promoSet.Id,
				Count = Math.Round(count, 2),
				Price = Math.Round(price, 2),
				CurrentPrice = Math.Round(currentPrice, 2),
				PriceWithoutDiscount = promoSet.SumWithoutDiscount(),
				CurrentSum = Math.Round(currentSum, 2),
				IsFixedPrice = false,
				ItemType = SaleItemType.PromoSet,
				Discounts = new List<IDiscountAmount>(discountData
					.DiscountDetails
					.Values)
			};
		}
		
		public OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineFreeRentPackage freeRentPackage)
		{
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = freeRentPackage.FreeRentPackage.Id,
				Count = freeRentPackage.Count,
				Price = Math.Round(freeRentPackage.Price, 2),
				CurrentPrice = Math.Round(freeRentPackage.Price, 2),
				PriceWithoutDiscount = null,
				CurrentSum = freeRentPackage.Sum,
				IsFixedPrice = false,
				ItemType = SaleItemType.RentPackage,
				Discounts = new List<IDiscountAmount>()
			};
		}
	}
}
