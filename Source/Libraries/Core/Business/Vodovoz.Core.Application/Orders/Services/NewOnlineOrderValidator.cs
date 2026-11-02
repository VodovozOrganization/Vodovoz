using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Orders.Delivery;
using Vodovoz.Core.Domain.Interfaces.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class NewOnlineOrderValidator : OrderFromOnlineOrderValidator
	{
		private readonly ISaleDiscountController _discountController;
		private readonly IGenericRepository<OnlineOrderTemplate> _templateRepository;

		public NewOnlineOrderValidator(
			IGoodsPriceCalculator goodsPriceCalculator,
			IDeliveryPriceGetter<OnlineOrderDeliveryPriceContext> deliveryPriceGetter,
			INomenclatureSettings nomenclatureSettings,
			IClientDeliveryPointsChecker clientDeliveryPointsChecker,
			ISaleDiscountController discountController,
			IFreeLoaderChecker freeLoaderChecker,
			IOrderOrganizationManager orderOrganizationManager,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IGenericRepository<OnlineOrderTemplate> templateRepository)
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
			_templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
		}
		
		private new OnlineOrderV2 OnlineOrder => base.OnlineOrder as OnlineOrderV2;

		public override Result Validate(IUnitOfWork uow, bool checkPerformedOrders = false)
		{
			base.Validate(uow, checkPerformedOrders);
			
			if(OnlineOrder.Template != null)
			{
				if(OnlineOrder.Template.Status == OnlineOrderTemplateStatus.PaymentProblem)
				{
					
				}
			}
			
			return ValidationResults.Any()
				? Result.Failure(ValidationResults)
				: Result.Success();
		}
		
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

				var checkOnlineOrderSum = CheckOnlineOrderSum.Create(onlinePromoSet.Count, 0, 0);
				
				CheckPromoSetForNewClientsCount(1, (int)onlinePromoSet.Count, promoSet);
				ValidatePrice(onlinePromoSet, checkOnlineOrderSum);
				ValidateDiscounts(onlinePromoSet, checkOnlineOrderSum);
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

		private void ValidateDiscounts(OnlineOrderPromoSet onlinePromoSet, CheckOnlineOrderSum checkSum)
		{
			var applicablePromotion = ApplicablePromotion.Create(onlinePromoSet);
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
				else
				{
					checkSum.DiscountMoney += discountReason.ValueType == DiscountUnits.money
						? discountReason.Value
						: checkSum.CalculateDiscountMoney(discountReason.Value);
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

		private void ValidatePrice(OnlineOrderPromoSet onlinePromoSet, CheckOnlineOrderSum checkSum)
		{
			var price = onlinePromoSet.PromoSet.Sum();
			checkSum.Price = price;
			
			if(price != onlinePromoSet.Price)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectPricePromoSetInOnlineOrder(
					onlinePromoSet.PromoSet.Title, price, onlinePromoSet.Price));
			}
		}
	}
}
