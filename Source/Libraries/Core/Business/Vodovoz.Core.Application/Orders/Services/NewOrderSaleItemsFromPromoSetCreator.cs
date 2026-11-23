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
		public IEnumerable<IEnumerable<NewOrderSaleItem>> Create(
			IUnitOfWork uow,
			OnlineOrderPromoSet onlineOrderPromoSet,
			bool hasPermissionsForAlternativePrice
			)
		{
			var newSaleItemsList = new List<List<NewOrderSaleItem>>();
			
			if(!onlineOrderPromoSet.DiscountReasons.Any())
			{
				for(var i = 0; i < onlineOrderPromoSet.Count; i++)
				{
					var newSaleItems = new List<NewOrderSaleItem>();
					newSaleItemsList.Add(newSaleItems);

					foreach(var promoItem in onlineOrderPromoSet.PromoSet.PromotionalSetItems)
					{
						var newSaleItem = NewOrderSaleItem.Create(
							promoItem.Nomenclature,
							promoItem.Count,
							default,
							promoItem.IsDiscountInMoney ? promoItem.DiscountMoney : promoItem.Discount,
							promoItem.IsDiscountInMoney,
							null,
							promoItem.PromoSet
						);
						
						newSaleItems.Add(newSaleItem);
					}
				}
				
				return newSaleItemsList;
			}
			
			//т.к. скидка может превышать стоимость одного или нескольких промонаборов и в заказе они добавляются товарами,
			//то мы должны просчитать скидки для всех товаров, входящих во все промики
			var promoSaleItem = _applicablePromotionFactory.CreateApplicablePromotion(onlineOrderPromoSet);
			var allPromoSetItemsDiscounts = _discountsController.CalculatePromoSetTotalDiscountBySaleItems(
				uow,
				promoSaleItem,
				hasPermissionsForAlternativePrice);

			for(var i = 0; i < onlineOrderPromoSet.Count; i++)
			{
				var newSaleItems = new List<NewOrderSaleItem>();
				newSaleItemsList.Add(newSaleItems);
				
				var promoSetItemsDiscounts = allPromoSetItemsDiscounts[i];

				foreach(var promoItem in onlineOrderPromoSet.PromoSet.PromotionalSetItems)
				{
					var newSaleItem = NewOrderSaleItem.Create(
						promoItem.Nomenclature,
						promoItem.Count,
						promoSet: onlineOrderPromoSet.PromoSet);

					newSaleItems.Add(newSaleItem);

					if(promoSetItemsDiscounts.TryGetValue(promoItem.Id, out var totalDiscountFromItem))
					{
						newSaleItem.UpdateDiscount(
							totalDiscountFromItem.TotalDiscountValue.IsDiscountMoney,
							totalDiscountFromItem.TotalDiscountValue.GetDiscount,
							totalDiscountFromItem.DiscountReasons);
					}
				}
			}
			
			return newSaleItemsList;
		}
	}
}
