using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Sale;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Controllers;

namespace CustomerOrdersApi.Library.V7.Factories
{
	public class OnlineOrderItemDtoFactory : IOnlineOrderItemDtoFactory
	{
		private readonly IDiscountController _discountController;

		public OnlineOrderItemDtoFactory(IDiscountController discountController)
		{
			_discountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
		}
		
		public OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(IProduct saleItem)
		{
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = saleItem.Id,
				Count = saleItem.Count,
				Price = saleItem.Price,
				CurrentPrice = Math.Round(saleItem.ActualSum / saleItem.Count, 2),
				PriceWithoutDiscount = null,
				CurrentSum = saleItem.ActualSum,
				IsFixedPrice = saleItem.IsFixedPrice,
				ItemType = saleItem.Nomenclature.Category.ToSaleItemType(),
				Discounts = new List<IDiscountAmount>(_discountController
					.CalculateTotalDiscountDetails(saleItem as ICalculatingTotalMoneyDiscount)
					.DiscountDetails)
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
	}
}
