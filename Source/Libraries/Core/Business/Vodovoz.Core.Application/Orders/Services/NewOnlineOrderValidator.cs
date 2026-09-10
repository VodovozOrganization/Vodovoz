using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Factories;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class NewOnlineOrderValidator : OrderFromOnlineOrderValidator
	{
		private readonly ISaleDiscountController _discountController;
		private readonly IApplicablePromotionFactory _applicablePromotionFactory;

		public NewOnlineOrderValidator(
			IGoodsPriceCalculator goodsPriceCalculator,
			IOnlineOrderDeliveryPriceGetter deliveryPriceGetter,
			INomenclatureSettings nomenclatureSettings,
			IClientDeliveryPointsChecker clientDeliveryPointsChecker,
			ISaleDiscountController discountController,
			IFreeLoaderChecker freeLoaderChecker,
			IOrderOrganizationManager orderOrganizationManager,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IApplicablePromotionFactory applicablePromotionFactory
			)
			: base(
				goodsPriceCalculator,
				deliveryPriceGetter,
				nomenclatureSettings,
				clientDeliveryPointsChecker,
				discountController,
				freeLoaderChecker,
				orderOrganizationManager,
				orderSettings,
				orderRepository)
		{
			_discountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
			_applicablePromotionFactory = applicablePromotionFactory ?? throw new ArgumentNullException(nameof(applicablePromotionFactory));
		}
		
		private new OnlineOrderV2 OnlineOrder => base.OnlineOrder as OnlineOrderV2;
		
		protected override void ValidatePromoSet(IUnitOfWork uow, IDictionary<int, bool> archivedNomenclatures)
		{
			CheckFreeLoader(uow);
			
			foreach(var onlinePromoSet in OnlineOrder.PromoSets)
			{
				var promoSet = onlinePromoSet.PromoSet;
				
				if(promoSet.IsArchive)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsArchivedOnlineOrderPromoSet(promoSet.Title));
				}

				var checkOnlineOrderSum = CheckOnlineOrderSum.Create(onlinePromoSet.Count, onlinePromoSet.Price, 0);
				
				CheckPromoSetForNewClientsCount(1, (int)onlinePromoSet.Count, promoSet);
				ValidatePrice(onlinePromoSet);
				ValidateDiscounts(onlinePromoSet);
				CalculatedOrderItemPrices.Add(checkOnlineOrderSum);
			}
		}

		protected override void CheckFreeLoader(IUnitOfWork uow)
		{
			var hasPromoSetForNewClients = OnlineOrder
				.PromoSets
				.Where(x => x.PromoSet != null)
				.Select(x => x.PromoSet)
				.Any(x => x.PromotionalSetForNewClients);

			if(!hasPromoSetForNewClients)
			{
				return;
			}
			
			var contactNumber =
				!string.IsNullOrWhiteSpace(OnlineOrder.ContactPhone) && OnlineOrder.ContactPhone.Length > 2
					? OnlineOrder.ContactPhone.Substring(2)
					: null;

			var result = FreeLoaderChecker.CanOrderPromoSetForNewClientsFromOnline(
				uow,
				OnlineOrder.IsSelfDelivery,
				OnlineOrder.CounterpartyId,
				OnlineOrder.DeliveryPointId,
				contactNumber);

			if(result.IsSuccess)
			{
				return;
			}

			foreach(var error in result.Errors)
			{
				ValidationResults.Add(error);
			}
		}

		private void ValidateDiscounts(OnlineOrderPromoSet onlinePromoSet)
		{
			var applicablePromotion = _applicablePromotionFactory.CreateApplicablePromotion(onlinePromoSet);
			var notApplicableDiscountReasonsBuilder = new StringBuilder();

			foreach(var discountReason in onlinePromoSet.DiscountReasons)
			{
				if(DiscountController.IsApplicableDiscount(discountReason, applicablePromotion).IsFailure)
				{
					notApplicableDiscountReasonsBuilder
						.Append(discountReason)
						.Append(',')
						.Append(' ');
				}
			}

			if(notApplicableDiscountReasonsBuilder.Length > 0)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.NotApplicableDiscountsToPromoSet(
					onlinePromoSet.PromoSetName,
					notApplicableDiscountReasonsBuilder
						.ToString()
						.TrimEnd(',', ' '))
				);
				onlinePromoSet.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
			}
		}

		private void ValidatePrice(OnlineOrderPromoSet onlinePromoSet)
		{
			var price = onlinePromoSet.PromoSet.Sum();
			
			var applicablePromotion = _applicablePromotionFactory.CreateApplicablePromotion(onlinePromoSet);
			var applicableDiscountReasons = onlinePromoSet.DiscountReasons
				.Where(x => _discountController.IsApplicableDiscount(x, applicablePromotion).IsSuccess)
				.ToList();

			var discounts = applicableDiscountReasons
				.Select(discountReason => _discountController.CalculateMoneyDiscount(price, discountReason))
				.Sum();
			
			price -= discounts;

			if(price != onlinePromoSet.Price)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectPricePromoSetInOnlineOrder(
					onlinePromoSet.PromoSet.Title, price, onlinePromoSet.Price));
			}
		}
	}
}
