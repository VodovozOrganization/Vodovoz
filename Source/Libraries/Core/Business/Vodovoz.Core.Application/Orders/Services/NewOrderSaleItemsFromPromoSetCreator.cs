using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Factories;

namespace Vodovoz.Core.Application.Orders.Services
{
	/// <inheritdoc/>
	public class NewOrderSaleItemsFromPromoSetCreator : INewOrderSaleItemsFromPromoSetCreator
	{
		private readonly IOrderDiscountsController _discountsController;
		private readonly IApplicablePromotionFactory _applicablePromotionFactory;

		public NewOrderSaleItemsFromPromoSetCreator(
			IOrderDiscountsController discountsController,
			IApplicablePromotionFactory applicablePromotionFactory
			)
		{
			_discountsController = discountsController ?? throw new ArgumentNullException(nameof(discountsController));
			_applicablePromotionFactory = applicablePromotionFactory ?? throw new ArgumentNullException(nameof(applicablePromotionFactory));
		}
		
		/// <inheritdoc/>
		public IEnumerable<NewOrderSaleItem> Create(
			IUnitOfWork uow,
			OnlineOrderPromoSet onlineOrderPromoSet,
			bool hasPermissionsForAlternativePrice
			)
		{
			if(!onlineOrderPromoSet.DiscountReasons.Any())
			{
				return onlineOrderPromoSet.PromoSet.PromotionalSetItems
					.Select(proSetItem =>
						NewOrderSaleItem.Create(
							proSetItem.Nomenclature,
							proSetItem.Count,
							default,
							proSetItem.IsDiscountInMoney ? proSetItem.DiscountMoney : proSetItem.Discount,
							proSetItem.IsDiscountInMoney,
							null,
							proSetItem.PromoSet
						)
					)
					.ToList();
			}
			
			var newSaleItems = new List<NewOrderSaleItem>();
			var promoSaleItem = _applicablePromotionFactory.CreateApplicablePromotion(onlineOrderPromoSet);
			var totalPromoSetItemsDiscount = _discountsController.CalculatePromoSetItemsTotalDiscount(
				uow,
				promoSaleItem,
				onlineOrderPromoSet.DiscountReasons,
				hasPermissionsForAlternativePrice);
			
			foreach(var promoItem in onlineOrderPromoSet.PromoSet.PromotionalSetItems)
			{
				var newSaleItem = NewOrderSaleItem.Create(
					promoItem.Nomenclature,
					promoItem.Count,
					promoSet: onlineOrderPromoSet.PromoSet);
				
				newSaleItems.Add(newSaleItem);
				
				if(totalPromoSetItemsDiscount.TryGetValue(promoItem.Id, out var totalDiscountFromItem))
				{
					newSaleItem.UpdateDiscount(
						totalDiscountFromItem.DiscountValue.IsDiscountMoney,
						totalDiscountFromItem.DiscountValue.GetDiscount,
						totalDiscountFromItem.DiscountReasons);
				}
			}
			
			return newSaleItems;
		}
	}
}
