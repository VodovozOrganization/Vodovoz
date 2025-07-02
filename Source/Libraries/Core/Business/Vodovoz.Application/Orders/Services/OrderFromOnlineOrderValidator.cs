﻿using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.Services.Orders;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Controllers;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OrderFromOnlineOrderValidator : IOrderFromOnlineOrderValidator
	{
		private readonly IGoodsPriceCalculator _priceCalculator;
		private readonly IOnlineOrderDeliveryPriceGetter _deliveryPriceGetter;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IClientDeliveryPointsChecker _clientDeliveryPointsChecker;
		private readonly IDiscountController _discountController;
		private readonly IFreeLoaderChecker _freeLoaderChecker;
		private OnlineOrder _onlineOrder;

		public OrderFromOnlineOrderValidator(
			IGoodsPriceCalculator goodsPriceCalculator,
			IOnlineOrderDeliveryPriceGetter deliveryPriceGetter,
			INomenclatureSettings nomenclatureSettings,
			IClientDeliveryPointsChecker clientDeliveryPointsChecker,
			IDiscountController discountController,
			IFreeLoaderChecker freeLoaderChecker)
		{
			_priceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			_deliveryPriceGetter = deliveryPriceGetter ?? throw new ArgumentNullException(nameof(deliveryPriceGetter));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_clientDeliveryPointsChecker = clientDeliveryPointsChecker ?? throw new ArgumentNullException(nameof(clientDeliveryPointsChecker));
			_discountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
			_freeLoaderChecker = freeLoaderChecker ?? throw new ArgumentNullException(nameof(freeLoaderChecker));
		}

		public Result ValidateOnlineOrder(IUnitOfWork uow, OnlineOrder onlineOrder)
		{
			_onlineOrder = onlineOrder;
			var validationResults = new List<Error>();
			
			if(_onlineOrder.IsSelfDelivery)
			{
				if(_onlineOrder.SelfDeliveryGeoGroup is null)
				{
					validationResults.Add(Vodovoz.Errors.Orders.OnlineOrder.IsEmptySelfDeliveryGeoGroup);
				}
			}
			else
			{
				if(_onlineOrder.Counterparty is null)
				{
					validationResults.Add(Vodovoz.Errors.Orders.OnlineOrder.IsEmptyCounterparty);
				}
				else
				{
					if(_onlineOrder.DeliveryPoint != null)
					{
						var result =
							_clientDeliveryPointsChecker.ClientDeliveryPointExists(
								_onlineOrder.Counterparty.Id, _onlineOrder.DeliveryPoint.Id);
						
						if(!result)
						{
							validationResults.Add(Vodovoz.Errors.Orders.OnlineOrder.DeliveryPointNotBelongCounterparty);
						}
						
						_onlineOrder.SetDeliveryPointNotBelongCounterparty(!result);
					}
				}
				
				if(_onlineOrder.DeliveryPoint is null)
				{
					validationResults.Add(Vodovoz.Errors.Orders.OnlineOrder.IsEmptyDeliveryPoint);
				}
				else
				{
					if(_onlineOrder.DeliveryPoint.District is null)
					{
						validationResults.Add(Vodovoz.Errors.Orders.OnlineOrder.IsEmptyDistrictFromDeliveryPoint);
					}
				}

				if(_onlineOrder.DeliverySchedule is null)
				{
					validationResults.Add(Vodovoz.Errors.Orders.OnlineOrder.IsEmptyDeliverySchedule);
				}
			}
			
			if(_onlineOrder.DeliveryDate < DateTime.Today && _onlineOrder.OnlineOrderStatus == OnlineOrderStatus.New)
			{
				validationResults.Add(Vodovoz.Errors.Orders.OnlineOrder.IncorrectDeliveryDate);
			}

			var phone = new PhoneEntity { Number = onlineOrder.ContactPhone };
			if(!phone.IsValidPhoneNumber)
			{
				validationResults.Add(Vodovoz.Errors.Orders.OnlineOrder.InvalidPhone(onlineOrder.ContactPhone));
			}

			ValidateOnlineOrderItems(uow, validationResults);
			
			return !validationResults.Any() ? Result.Success() : Result.Failure(validationResults);
		}

		private void ValidateOnlineOrderItems(IUnitOfWork uow, ICollection<Error> errors)
		{
			var archivedNomenclatures = new Dictionary<int, bool>();
			ValidatePromoSet(uow, archivedNomenclatures, errors);
			ValidateOtherItemsWithoutDeliveries(archivedNomenclatures, errors);
			ValidatePaidDelivery(errors);
			ValidateFastDelivery(errors);
			ValidateOnlineRentPackages(errors);
		}

		private void ValidatePromoSet(IUnitOfWork uow, IDictionary<int, bool> archivedNomenclatures, ICollection<Error> errors)
		{
			var onlineOrderPromoSets = _onlineOrder.OnlineOrderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSetId);

			CheckFreeLoader(uow, errors);
			
			foreach(var onlineOrderItemGroup in onlineOrderPromoSets)
			{
				var promoSet = onlineOrderItemGroup.First().PromoSet;
				
				if(promoSet.IsArchive)
				{
					errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IsArchivedOnlineOrderPromoSet(promoSet.Title));
				}

				var promoSetItemsCount = promoSet.PromotionalSetItems.Count;
				var onlinePromoItemsCount = onlineOrderItemGroup.Count();
				
				CheckCorrectPromoSetItemsCount(errors, promoSetItemsCount, onlinePromoItemsCount, promoSet.Title);
				CheckPromoSetForNewClientsCount(errors, promoSetItemsCount, onlinePromoItemsCount, promoSet);
					
				var i = 0;
				foreach(var onlineOrderItem in onlineOrderItemGroup)
				{
					ValidateNomenclatureByArchive(archivedNomenclatures, onlineOrderItem, errors);
					ValidateCount(onlineOrderItem, i, errors);
					ValidatePrice(onlineOrderItem, errors);
					ValidateDiscountFromPromoSet(onlineOrderItem, i, errors);
					i++;

					if(i >= promoSetItemsCount)
					{
						i = 0;
					}
				}
			}
		}

		private void CheckFreeLoader(IUnitOfWork uow, ICollection<Error> errors)
		{
			var hasPromoSetForNewClients =
				_onlineOrder.OnlineOrderItems
					.Where(x => x.PromoSet != null)
					.Select(x => x.PromoSet)
					.Any(x => x.PromotionalSetForNewClients);

			if(!hasPromoSetForNewClients)
			{
				return;
			}

			var result = _freeLoaderChecker.CanOrderPromoSetForNewClientsFromOnline(
				uow,
				_onlineOrder.IsSelfDelivery,
				_onlineOrder.CounterpartyId,
				_onlineOrder.DeliveryPointId);

			if(result.IsSuccess)
			{
				return;
			}

			foreach(var error in result.Errors)
			{
				errors.Add(error);
			}
		}

		private void CheckCorrectPromoSetItemsCount(
			ICollection<Error> errors,
			int promoSetItemsCount,
			int onlinePromoItemsCount,
			string promoSetTitle)
		{
			if(promoSetItemsCount < onlinePromoItemsCount)
			{
				if(onlinePromoItemsCount % promoSetItemsCount != 0)
				{
					errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IsIncorrectOnlineOrderPromoSetItemsCount(promoSetTitle));
				}
			}
		}
		
		private void CheckPromoSetForNewClientsCount(
			ICollection<Error> errors,
			int promoSetItemsCount,
			int onlinePromoItemsCount,
			PromotionalSet promoSet)
		{
			if(promoSetItemsCount < onlinePromoItemsCount && promoSet.PromotionalSetForNewClients)
			{
				errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IsIncorrectOnlineOrderPromoSetForNewClientsCount());
			}
		}

		private void ValidateNomenclatureByArchive(
			IDictionary<int, bool> archivedNomenclatures,
			OnlineOrderItem onlineOrderItem,
			ICollection<Error> errors)
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
			errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IsArchivedNomenclatureInOnlineOrder(nomenclature.ToString()));
		}

		private void ValidateCount(OnlineOrderItem onlineOrderItem, int index, ICollection<Error> errors)
		{
			var promoSetItem = onlineOrderItem.PromoSet.PromotionalSetItems[index];
			var countFromPromoSetItem = promoSetItem.Count;

			onlineOrderItem.CountFromPromoSet = countFromPromoSetItem;

			if(countFromPromoSetItem != onlineOrderItem.Count)
			{
				errors.Add(
					Vodovoz.Errors.Orders.OnlineOrder.IncorrectCountNomenclatureInOnlineOrderPromoSet(
						onlineOrderItem.PromoSet.Title,
						++index,
						onlineOrderItem.Nomenclature.ToString(),
						countFromPromoSetItem,
						(int)onlineOrderItem.Count));
			}
		}

		private void ValidatePrice(OnlineOrderItem onlineOrderItem, ICollection<Error> errors)
		{
			var price = _priceCalculator.CalculateItemPrice(
				_onlineOrder.OnlineOrderItems,
				_onlineOrder.DeliveryPoint,
				null,
				onlineOrderItem.Nomenclature,
				onlineOrderItem.PromoSet,
				onlineOrderItem.Count,
				false);

			onlineOrderItem.NomenclaturePrice = price;

			if(price != onlineOrderItem.Price)
			{
				errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IncorrectPriceNomenclatureInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(), price, onlineOrderItem.Price));
			}
		}
		
		private void ValidateDiscountProperties(OnlineOrderItem onlineOrderItem, ICollection<Error> errors)
		{
			ValidateDiscountParametersFromNotPromoSet(onlineOrderItem, errors);
		}

		private void ValidateDiscountParametersFromNotPromoSet(OnlineOrderItem onlineOrderItem, ICollection<Error> errors)
		{
			if(onlineOrderItem.DiscountReason != null)
			{
				var applicableDiscount =
					_discountController.IsApplicableDiscount(onlineOrderItem.DiscountReason, onlineOrderItem.Nomenclature);

				if(applicableDiscount)
				{
					ValidateApplicableDiscountFromNotPromoSet(onlineOrderItem, errors);
				}
				else
				{
					ValidateNotApplicableDiscountFromNotPromoSet(onlineOrderItem, errors);
				}
			}
			else
			{
				if(onlineOrderItem.GetDiscount > 0)
				{
					errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IncorrectDiscountNomenclatureInOnlineOrder(
						onlineOrderItem.Nomenclature.ToString(), 0, onlineOrderItem.GetDiscount));
				}
						
				onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
			}
		}

		private void ValidateApplicableDiscountFromNotPromoSet(OnlineOrderItem onlineOrderItem, ICollection<Error> errors)
		{
			if(onlineOrderItem.GetDiscount != onlineOrderItem.DiscountReason.Value)
			{
				errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IncorrectDiscountNomenclatureInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(),
					onlineOrderItem.DiscountReason.Value,
					onlineOrderItem.GetDiscount));
				onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
			}

			switch(onlineOrderItem.DiscountReason.ValueType)
			{
				case DiscountUnits.money:
					if(!onlineOrderItem.IsDiscountInMoney)
					{
						errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IncorrectDiscountTypeInOnlineOrder(
							onlineOrderItem.Nomenclature.ToString(), true, onlineOrderItem.IsDiscountInMoney));
						onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
					}
					break;
				case DiscountUnits.percent:
					if(onlineOrderItem.IsDiscountInMoney)
					{
						errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IncorrectDiscountTypeInOnlineOrder(
							onlineOrderItem.Nomenclature.ToString(), false, onlineOrderItem.IsDiscountInMoney));
						onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
					}
					break;
			}
		}

		private void ValidateNotApplicableDiscountFromNotPromoSet(OnlineOrderItem onlineOrderItem, ICollection<Error> errors)
		{
			if(onlineOrderItem.GetDiscount > 0)
			{
				errors.Add(Vodovoz.Errors.Orders.OnlineOrder.NotApplicableDiscountToNomenclatureOnlineOrder(
					onlineOrderItem.Nomenclature.ToString()));
			}
							
			if(onlineOrderItem.IsDiscountInMoney)
			{
				errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IncorrectDiscountTypeInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(), false, onlineOrderItem.IsDiscountInMoney));
			}

			onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
		}

		private void ValidateDiscountFromPromoSet(OnlineOrderItem onlineOrderItem, int index, ICollection<Error> errors)
		{
			var promoSetItem = onlineOrderItem.PromoSet.PromotionalSetItems[index];
			var discountInMoneyFromPromoSet = promoSetItem.IsDiscountInMoney;
			var discountItemFromPromoSet = discountInMoneyFromPromoSet ? promoSetItem.DiscountMoney : promoSetItem.Discount;
			var onlineOrderItemDiscount = onlineOrderItem.GetDiscount;
			
			onlineOrderItem.DiscountFromPromoSet = discountItemFromPromoSet;
			onlineOrderItem.IsDiscountInMoneyFromPromoSet = discountInMoneyFromPromoSet;

			var position = ++index;

			if(discountInMoneyFromPromoSet != onlineOrderItem.IsDiscountInMoney)
			{
				errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IncorrectDiscountTypeInOnlineOrderPromoSet(
					onlineOrderItem.PromoSet.Title,
					position,
					onlineOrderItem.Nomenclature.ToString(),
					discountInMoneyFromPromoSet,
					onlineOrderItem.IsDiscountInMoney));
			}
			
			if(discountItemFromPromoSet != onlineOrderItemDiscount)
			{
				errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IncorrectDiscountInOnlineOrderPromoSet(
					onlineOrderItem.PromoSet.Title,
					position,
					onlineOrderItem.Nomenclature.ToString(),
					discountItemFromPromoSet,
					onlineOrderItemDiscount));
			}
		}

		private void ValidateOtherItemsWithoutDeliveries(IDictionary<int, bool> archivedNomenclatures, ICollection<Error> errors)
		{
			var onlineOrderItemsNotPromoSet =
				_onlineOrder.OnlineOrderItems
					.Where(x => x.PromoSet is null);
			
			foreach(var onlineOrderItem in onlineOrderItemsNotPromoSet)
			{
				if(onlineOrderItem.Nomenclature is null)
				{
					errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IsIncorrectNomenclatureInOnlineOrder(onlineOrderItem.NomenclatureId));
					continue;
				}
				
				if(onlineOrderItem.NomenclatureId == _nomenclatureSettings.PaidDeliveryNomenclatureId
					|| onlineOrderItem.NomenclatureId == _nomenclatureSettings.FastDeliveryNomenclatureId)
				{
					continue;
				}
				
				ValidateNomenclatureByArchive(archivedNomenclatures, onlineOrderItem, errors);
				ValidatePrice(onlineOrderItem, errors);
				ValidateDiscountProperties(onlineOrderItem, errors);
			}
		}

		private void ValidatePaidDelivery(ICollection<Error> errors)
		{
			if(_onlineOrder.IsSelfDelivery || _onlineOrder.DeliveryPoint?.District is null)
			{
				return;
			}
			
			var paidDelivery =
				_onlineOrder.OnlineOrderItems
					.SingleOrDefault(x => x.PromoSet is null && x.NomenclatureId == _nomenclatureSettings.PaidDeliveryNomenclatureId);

			var deliveryPrice = _deliveryPriceGetter.GetDeliveryPrice(_onlineOrder);
			var needPaidDelivery = deliveryPrice > 0;

			if(paidDelivery != null)
			{
				paidDelivery.NomenclaturePrice = deliveryPrice;
			}

			if(needPaidDelivery && paidDelivery != null)
			{
				if(paidDelivery.Price != deliveryPrice)
				{
					errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IncorrectPricePaidDelivery(deliveryPrice, paidDelivery.Price));
				}
			}
			else if(needPaidDelivery && paidDelivery is null)
			{
				errors.Add(Vodovoz.Errors.Orders.OnlineOrder.NeedPaidDelivery);
			}
			else if(!needPaidDelivery && paidDelivery != null)
			{
				errors.Add(Vodovoz.Errors.Orders.OnlineOrder.NotNeedPaidDelivery);
			}
		}
		
		private void ValidateFastDelivery(ICollection<Error> errors)
		{
			var fastDelivery =
				_onlineOrder.OnlineOrderItems
					.SingleOrDefault(x => x.PromoSet is null && x.NomenclatureId == _nomenclatureSettings.FastDeliveryNomenclatureId);
			
			if(!_onlineOrder.IsFastDelivery)
			{
				if(fastDelivery != null)
				{
					errors.Add(Vodovoz.Errors.Orders.FastDelivery.NotNeedFastDelivery);
				}
				return;
			}

			if(fastDelivery is null)
			{
				errors.Add(Vodovoz.Errors.Orders.FastDelivery.FastDeliveryIsMissing);
				return;
			}

			if(_onlineOrder.DeliveryDate != DateTime.Today)
			{
				errors.Add(Vodovoz.Errors.Orders.FastDelivery.InvalidDate);
			}
			
			ValidatePrice(fastDelivery, errors);
		}

		private void ValidateOnlineRentPackages(ICollection<Error> errors)
		{
			foreach(var onlineRentPackage in _onlineOrder.OnlineRentPackages)
			{
				var freeRentPackage = onlineRentPackage.FreeRentPackage;
				if(freeRentPackage is null)
				{
					errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IncorrectRentPackageIdInOnlineOrder(onlineRentPackage.FreeRentPackageId));
					continue;
				}

				if(freeRentPackage.IsArchive)
				{
					errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IsArchivedRentPackageIdInOnlineOrder(onlineRentPackage.FreeRentPackageId));
				}

				var depositFromRentPackage = freeRentPackage.Deposit;

				onlineRentPackage.FreeRentPackagePriceFromProgram = depositFromRentPackage;

				if(depositFromRentPackage != onlineRentPackage.Price)
				{
					errors.Add(Vodovoz.Errors.Orders.OnlineOrder.IncorrectRentPackagePriceInOnlineOrder(
						onlineRentPackage.FreeRentPackageId, onlineRentPackage.Price, depositFromRentPackage));
				}
			}
		}
	}
}
