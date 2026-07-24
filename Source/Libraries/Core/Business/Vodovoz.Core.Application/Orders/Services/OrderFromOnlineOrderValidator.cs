using System;
using System.Collections.Generic;
using System.Linq;
using Core.Infrastructure;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Extensions;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using Vodovoz.Validation;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public abstract class OrderFromOnlineOrderValidator : IOnlineOrderValidator
	{
		protected OrderFromOnlineOrderValidator(
			IGoodsPriceCalculator goodsPriceCalculator,
			IOnlineOrderDeliveryPriceGetter deliveryPriceGetter,
			INomenclatureSettings nomenclatureSettings,
			IClientDeliveryPointsChecker clientDeliveryPointsChecker,
			IDiscountController discountController,
			IFreeLoaderChecker freeLoaderChecker,
			IOrderOrganizationManager orderOrganizationManager,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository
			)
		{
			PriceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			DeliveryPriceGetter = deliveryPriceGetter ?? throw new ArgumentNullException(nameof(deliveryPriceGetter));
			NomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			ClientDeliveryPointsChecker = clientDeliveryPointsChecker ?? throw new ArgumentNullException(nameof(clientDeliveryPointsChecker));
			DiscountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
			FreeLoaderChecker = freeLoaderChecker ?? throw new ArgumentNullException(nameof(freeLoaderChecker));
			OrderOrganizationManager = orderOrganizationManager ?? throw new ArgumentNullException(nameof(orderOrganizationManager));
			OrderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			OrderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		}

		protected IGoodsPriceCalculator PriceCalculator { get; }
		protected IOnlineOrderDeliveryPriceGetter DeliveryPriceGetter { get; }
		protected INomenclatureSettings NomenclatureSettings { get; }
		protected IClientDeliveryPointsChecker ClientDeliveryPointsChecker { get; }
		protected IDiscountController DiscountController { get; }
		protected IFreeLoaderChecker FreeLoaderChecker { get; }
		protected IOrderOrganizationManager OrderOrganizationManager { get; }
		protected IOrderSettings OrderSettings { get; }
		protected IOrderRepository OrderRepository { get; }
		
		protected virtual OnlineOrder OnlineOrder { get; set; }
		protected List<Error> ValidationResults { get; set; }
		protected List<ICheckOnlineOrderSum> CalculatedOrderItemPrices { get; set; }

		public void SetOnlineOrder(OnlineOrder onlineOrder)
		{
			OnlineOrder = onlineOrder;
		}

		public virtual Result Validate(IUnitOfWork uow, bool checkPerformedOrders = false)
		{
			ThrowIfOnlineOrderIsNull();
			ValidationResults = new List<Error>();
			CalculatedOrderItemPrices = new List<ICheckOnlineOrderSum>();

			if(OnlineOrder.IsNeedConfirmationByCall)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsNeedConfirmationByCall);
			}

			if(OnlineOrder.IsSelfDelivery)
			{
				if(OnlineOrder.SelfDeliveryGeoGroup is null)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsEmptySelfDeliveryGeoGroup);
				}
			}
			else
			{
				if(OnlineOrder.Counterparty is null)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsEmptyCounterparty);
				}
				else
				{
					if(OnlineOrder.DeliveryPoint != null)
					{
						var result =
							ClientDeliveryPointsChecker.ClientDeliveryPointExists(
								OnlineOrder.Counterparty.Id, OnlineOrder.DeliveryPoint.Id);
						
						if(!result)
						{
							ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.DeliveryPointNotBelongCounterparty);
						}
						
						OnlineOrder.SetDeliveryPointNotBelongCounterparty(!result);
					}
				}
				
				if(OnlineOrder.DeliveryPoint is null)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsEmptyDeliveryPoint);
				}
				else
				{
					if(OnlineOrder.DeliveryPoint.District is null)
					{
						ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsEmptyDistrictFromDeliveryPoint);
					}
				}

				if(OnlineOrder.DeliverySchedule is null)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsEmptyDeliverySchedule);
				}
			}
			
			if(OnlineOrder.DeliveryDate < DateTime.Today && OnlineOrder.OnlineOrderStatus == OnlineOrderStatus.New)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDeliveryDate);
			}

			if(!string.IsNullOrEmpty(OnlineOrder.ContactPhone))
			{
				var phone = new PhoneEntity { Number = OnlineOrder.ContactPhone };
				if(!phone.IsValidPhoneNumber)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.InvalidPhone(OnlineOrder.ContactPhone));
				}
			}

			if(OrderOrganizationManager.SplitOrderByOrganizations(
				uow, DateTime.Now.TimeOfDay, OrderOrganizationChoice.Create(uow, OrderSettings, OnlineOrder)).Count() > 1)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.OnlineOrderContainsGoodsSoldFromSeveralOrganizations());
			}

			if(checkPerformedOrders)
			{
				var ordersIds =
					OrderRepository.GetClientOrdersIdsForDate(
						uow,
						OnlineOrder.DeliveryDate,
						OnlineOrder.CounterpartyId,
						OnlineOrder.DeliveryPointId);

				if(ordersIds.Any())
				{
					ValidationResults.Add(
						Vodovoz.Errors.Orders.OnlineOrderErrors.ClientHasOrdersForThisDate(ordersIds.ToStringValue(',')));
				}
			}

			if(OnlineOrder.OnlineOrderPaymentType == OnlineOrderPaymentType.PaidOnline
				&& OnlineOrder.OnlineOrderPaymentStatus == OnlineOrderPaymentStatus.UnPaid
				&& OnlineOrder.OnlineOrderStatus == OnlineOrderStatus.New)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.ClientDontPayOrder());
			}

			ValidateOnlineOrderItems(uow);
			ValidateTotalSum();
			
			return !ValidationResults.Any() ? Result.Success() : Result.Failure(ValidationResults);
		}

		protected virtual void ValidateTotalSum()
		{
			var producedOrderSum = OnlineOrder.OnlineOrderSum;
			var currentOrderSum = CalculatedOrderItemPrices.Sum(x => x.Sum);

			if(producedOrderSum != currentOrderSum)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectOrderSum(producedOrderSum, currentOrderSum));
			}
		}

		protected virtual void ValidateOnlineOrderItems(IUnitOfWork uow)
		{
			var archivedNomenclatures = new Dictionary<int, bool>();
			ValidatePromoSet(uow, archivedNomenclatures);
			ValidateOtherItemsWithoutDeliveries(archivedNomenclatures);
			ValidatePaidDelivery();
			ValidateFastDelivery();
			ValidateOnlineRentPackages();
		}

		protected virtual void ValidatePromoSet(IUnitOfWork uow, IDictionary<int, bool> archivedNomenclatures)
		{
			var onlineOrderPromoSets = OnlineOrder.OnlineOrderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSetId);

			CheckFreeLoader(uow);
			
			foreach(var onlineOrderItemGroup in onlineOrderPromoSets)
			{
				var promoSet = onlineOrderItemGroup.First().PromoSet;
				
				if(promoSet.IsArchive)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsArchivedOnlineOrderPromoSet(promoSet.Title));
				}

				var promoSetItemsCount = promoSet.PromotionalSetItems.Count;
				var onlinePromoItemsCount = onlineOrderItemGroup.Count();
				
				CheckCorrectPromoSetItemsCount(promoSetItemsCount, onlinePromoItemsCount, promoSet.Title);
				CheckPromoSetForNewClientsCount(promoSetItemsCount, onlinePromoItemsCount, promoSet);
					
				var i = 0;
				foreach(var onlineOrderItem in onlineOrderItemGroup)
				{
					var checkOnlineOrderSum = CheckOnlineOrderSum.Create(0, 0, 0);
					
					ValidateNomenclatureByArchive(archivedNomenclatures, onlineOrderItem);
					ValidateCountFromPromoSet(onlineOrderItem, i, checkOnlineOrderSum);
					ValidatePrice(onlineOrderItem, checkOnlineOrderSum);
					ValidateDiscountFromPromoSet(onlineOrderItem, i, checkOnlineOrderSum);
					i++;

					if(i >= promoSetItemsCount)
					{
						i = 0;
					}
					
					CalculatedOrderItemPrices.Add(checkOnlineOrderSum);
				}
			}
		}

		protected virtual void CheckFreeLoader(IUnitOfWork uow)
		{
			var hasPromoSetForNewClients =
				OnlineOrder.OnlineOrderItems
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

		private void CheckCorrectPromoSetItemsCount(
			int promoSetItemsCount,
			int onlinePromoItemsCount,
			string promoSetTitle)
		{
			if(promoSetItemsCount < onlinePromoItemsCount)
			{
				if(onlinePromoItemsCount % promoSetItemsCount != 0)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsIncorrectOnlineOrderPromoSetItemsCount(promoSetTitle));
				}
			}
		}
		
		protected virtual void CheckPromoSetForNewClientsCount(
			int promoSetItemsCount,
			int onlinePromoItemsCount,
			PromotionalSet promoSet)
		{
			if(promoSetItemsCount < onlinePromoItemsCount && promoSet.PromotionalSetForNewClients)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsIncorrectOnlineOrderPromoSetForNewClientsCount());
			}
		}

		protected void ValidateNomenclatureByArchive(IDictionary<int, bool> archivedNomenclatures, OnlineOrderItem onlineOrderItem)
		{
			var nomenclature = onlineOrderItem.Nomenclature;
			
			if(nomenclature == null)
			{
				return;
			}

			if(!nomenclature.IsArchive)
			{
				return;
			}

			if(archivedNomenclatures.TryGetValue(nomenclature.Id, out var hasArchivedNomenclature))
			{
				return;
			}

			archivedNomenclatures.Add(nomenclature.Id, true);
			ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsArchivedNomenclatureInOnlineOrder(nomenclature.ToString()));
		}

		private void ValidateCountFromPromoSet(OnlineOrderItem onlineOrderItem, int index, CheckOnlineOrderSum checkOnlineOrderSum)
		{
			var promoSetItem = onlineOrderItem.PromoSet.PromotionalSetItems[index];
			var countFromPromoSetItem = promoSetItem.Count;

			onlineOrderItem.CountFromPromoSet = countFromPromoSetItem;
			checkOnlineOrderSum.Count = countFromPromoSetItem;

			if(countFromPromoSetItem != onlineOrderItem.Count)
			{
				ValidationResults.Add(
					Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectCountNomenclatureInOnlineOrderPromoSet(
						onlineOrderItem.PromoSet.Title,
						++index,
						onlineOrderItem.Nomenclature.ToString(),
						countFromPromoSetItem,
						(int)onlineOrderItem.Count));
			}
		}

		protected virtual void ValidatePrice(OnlineOrderItem onlineOrderItem, CheckOnlineOrderSum checkOnlineOrderSum)
		{
			var price = PriceCalculator.CalculateItemPrice(
				OnlineOrder.OnlineOrderItems,
				OnlineOrder.DeliveryPoint,
				OnlineOrder.Counterparty,
				onlineOrderItem,
				false);

			onlineOrderItem.NomenclaturePrice = price;
			checkOnlineOrderSum.Price = price;

			if(price != onlineOrderItem.Price)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectPriceNomenclatureInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(), price, onlineOrderItem.Price));
			}
		}
		
		protected virtual void ValidateDiscountProperties(OnlineOrderItem onlineOrderItem, CheckOnlineOrderSum checkOnlineOrderSum)
		{
			ValidateDiscountParametersFromNotPromoSet(onlineOrderItem, checkOnlineOrderSum);
		}

		protected void ValidateDiscountParametersFromNotPromoSet(OnlineOrderItem onlineOrderItem, CheckOnlineOrderSum checkOnlineOrderSum)
		{
			var isAllDiscountReasonsApplicable = onlineOrderItem.DiscountReasons
				.All(reason => DiscountController.IsApplicableDiscount(reason, onlineOrderItem.Nomenclature));

			if(isAllDiscountReasonsApplicable)
			{
				ValidateApplicableDiscountFromNotPromoSet(onlineOrderItem, checkOnlineOrderSum);
			}
			else if(onlineOrderItem.DiscountReasons.Any())
			{
				ValidateNotApplicableDiscountFromNotPromoSet(onlineOrderItem);
			}
			else if(onlineOrderItem.GetDiscount > 0)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountNomenclatureInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(), 0, onlineOrderItem.GetDiscount));
				onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
			}
		}

		protected void ValidateApplicableDiscountFromNotPromoSet(OnlineOrderItem onlineOrderItem, CheckOnlineOrderSum checkOnlineOrderSum)
		{
			checkOnlineOrderSum.DiscountMoney =
				onlineOrderItem.IsDiscountInMoneyFromDiscountReasons
					? onlineOrderItem.DiscountMoneyFromDiscountReasons
					: checkOnlineOrderSum.CalculateDiscountMoney(onlineOrderItem.DiscountPercentFromDiscountReasons);
			
			if(onlineOrderItem.GetDiscount != onlineOrderItem.GetDiscountFromDiscountReasons)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountNomenclatureInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(),
					onlineOrderItem.GetDiscountFromDiscountReasons,
					onlineOrderItem.GetDiscount));
				onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
			}

			if(onlineOrderItem.IsDiscountInMoneyFromDiscountReasons && !onlineOrderItem.IsDiscountInMoney)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountTypeInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(), true, onlineOrderItem.IsDiscountInMoney));
				onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
			}
			else if(!onlineOrderItem.IsDiscountInMoneyFromDiscountReasons && onlineOrderItem.IsDiscountInMoney)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountTypeInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(), false, onlineOrderItem.IsDiscountInMoney));
				onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
			}
		}

		protected void ValidateNotApplicableDiscountFromNotPromoSet(OnlineOrderItem onlineOrderItem)
		{
			if(onlineOrderItem.GetDiscount > 0)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.NotApplicableDiscountToNomenclatureOnlineOrder(
					onlineOrderItem.Nomenclature.ToString()));
			}
							
			if(onlineOrderItem.IsDiscountInMoney)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountTypeInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(), false, onlineOrderItem.IsDiscountInMoney));
			}

			onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
		}

		private void ValidateDiscountFromPromoSet(OnlineOrderItem onlineOrderItem, int index, CheckOnlineOrderSum checkOnlineOrderSum)
		{
			var promoSetItem = onlineOrderItem.PromoSet.PromotionalSetItems[index];
			var discountInMoneyFromPromoSet = promoSetItem.IsDiscountInMoney;
			var discountItemFromPromoSet = discountInMoneyFromPromoSet ? promoSetItem.DiscountMoney : promoSetItem.Discount;
			var onlineOrderItemDiscount = onlineOrderItem.GetDiscount;
			
			onlineOrderItem.DiscountFromPromoSet = discountItemFromPromoSet;
			onlineOrderItem.IsDiscountInMoneyFromPromoSet = discountInMoneyFromPromoSet;
			checkOnlineOrderSum.DiscountMoney = promoSetItem.IsDiscountInMoney
				? promoSetItem.DiscountMoney
				: checkOnlineOrderSum.CalculateDiscountMoney(promoSetItem.Discount);

			var position = ++index;

			if(discountInMoneyFromPromoSet != onlineOrderItem.IsDiscountInMoney)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountTypeInOnlineOrderPromoSet(
					onlineOrderItem.PromoSet.Title,
					position,
					onlineOrderItem.Nomenclature.ToString(),
					discountInMoneyFromPromoSet,
					onlineOrderItem.IsDiscountInMoney));
			}
			
			if(discountItemFromPromoSet != onlineOrderItemDiscount)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountInOnlineOrderPromoSet(
					onlineOrderItem.PromoSet.Title,
					position,
					onlineOrderItem.Nomenclature.ToString(),
					discountItemFromPromoSet,
					onlineOrderItemDiscount));
			}
		}

		protected virtual void ValidateOtherItemsWithoutDeliveries(IDictionary<int, bool> archivedNomenclatures)
		{
			var onlineOrderItemsNotPromoSet =
				OnlineOrder.OnlineOrderItems
					.Where(x => x.PromoSet is null);
			
			foreach(var onlineOrderItem in onlineOrderItemsNotPromoSet)
			{
				if(onlineOrderItem.Nomenclature is null)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsIncorrectNomenclatureInOnlineOrder(onlineOrderItem.NomenclatureId));
					continue;
				}
				
				if(onlineOrderItem.NomenclatureId == NomenclatureSettings.PaidDeliveryNomenclatureId
					|| onlineOrderItem.NomenclatureId == NomenclatureSettings.FastDeliveryNomenclatureId)
				{
					continue;
				}

				if(onlineOrderItem.Nomenclature.Category is NomenclatureCategory.master or NomenclatureCategory.spare_parts)
				{
					ValidationResults.Add(
						Vodovoz.Errors.Orders.OnlineOrderErrors.IsServiceNomenclatureInOnlineOrder(
							onlineOrderItem.NomenclatureId, onlineOrderItem.Nomenclature.Category.GetEnumDisplayName()));
				}
				
				var checkOnlineOrderSum = CheckOnlineOrderSum.Create(onlineOrderItem.Count, 0, 0);
				
				ValidateNomenclatureByArchive(archivedNomenclatures, onlineOrderItem);
				ValidateCount(onlineOrderItem);
				ValidatePrice(onlineOrderItem, checkOnlineOrderSum);
				ValidateDiscountProperties(onlineOrderItem, checkOnlineOrderSum);
				
				CalculatedOrderItemPrices.Add(checkOnlineOrderSum);
			}
		}

		protected virtual void ValidateCount(OnlineOrderItem onlineOrderItem)
		{
			if(onlineOrderItem.Count <= 0)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectCountNomenclatureInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(), onlineOrderItem.Count));
			}
		}

		protected virtual void ValidatePaidDelivery()
		{
			if(OnlineOrder.IsSelfDelivery || OnlineOrder.DeliveryPoint?.District is null)
			{
				return;
			}
			
			var paidDelivery =
				OnlineOrder.OnlineOrderItems
					.SingleOrDefault(x => x.PromoSet is null && x.NomenclatureId == NomenclatureSettings.PaidDeliveryNomenclatureId);

			var deliveryPrice = DeliveryPriceGetter.GetDeliveryPrice(OnlineOrder);
			var needPaidDelivery = deliveryPrice > 0;
			var checkOnlineOrderSum = CheckOnlineOrderSum.Create(1, deliveryPrice, 0);

			if(paidDelivery != null)
			{
				paidDelivery.NomenclaturePrice = deliveryPrice;
			}

			if(needPaidDelivery && paidDelivery != null)
			{
				if(paidDelivery.Price != deliveryPrice)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectPricePaidDelivery(deliveryPrice, paidDelivery.Price));
				}
			}
			else if(needPaidDelivery && paidDelivery is null)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.NeedPaidDelivery);
			}
			else if(!needPaidDelivery && paidDelivery != null)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.NotNeedPaidDelivery);
				checkOnlineOrderSum.Count = 0;
			}
			
			CalculatedOrderItemPrices.Add(checkOnlineOrderSum);
		}
		
		protected virtual void ValidateFastDelivery()
		{
			var fastDelivery =
				OnlineOrder.OnlineOrderItems
					.SingleOrDefault(x => x.PromoSet is null && x.NomenclatureId == NomenclatureSettings.FastDeliveryNomenclatureId);
			
			if(!OnlineOrder.IsFastDelivery)
			{
				if(fastDelivery != null)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.FastDeliveryErrors.NotNeedFastDelivery);
				}
				return;
			}
			
			var checkOnlineOrderSum = CheckOnlineOrderSum.Create(1, 0, 0);

			if(fastDelivery is null)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.FastDeliveryErrors.FastDeliveryIsMissing);
				return;
			}

			if(OnlineOrder.DeliveryDate != DateTime.Today)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.FastDeliveryErrors.InvalidDate);
			}
			
			ValidatePrice(fastDelivery, checkOnlineOrderSum);
			CalculatedOrderItemPrices.Add(checkOnlineOrderSum);
		}

		protected virtual void ValidateOnlineRentPackages()
		{
			foreach(var onlineRentPackage in OnlineOrder.OnlineRentPackages)
			{
				var freeRentPackage = onlineRentPackage.FreeRentPackage;
				if(freeRentPackage is null)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectRentPackageIdInOnlineOrder(onlineRentPackage.FreeRentPackageId));
					continue;
				}

				if(freeRentPackage.IsArchive)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsArchivedRentPackageIdInOnlineOrder(onlineRentPackage.FreeRentPackageId));
				}

				var depositFromRentPackage = freeRentPackage.Deposit;

				onlineRentPackage.FreeRentPackagePriceFromProgram = depositFromRentPackage;
				var checkOnlineOrderSum = CheckOnlineOrderSum.Create(onlineRentPackage.Count, depositFromRentPackage, 0);
				CalculatedOrderItemPrices.Add(checkOnlineOrderSum);

				if(depositFromRentPackage != onlineRentPackage.Price)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectRentPackagePriceInOnlineOrder(
						onlineRentPackage.FreeRentPackageId, onlineRentPackage.Price, depositFromRentPackage));
				}
			}
		}
		
		private void ThrowIfOnlineOrderIsNull()
		{
			if(OnlineOrder is null)
			{
				throw new ArgumentNullException(nameof(OnlineOrder), "Онлайн заказ не должен быть null!!!");
			}
		}
	}
}
