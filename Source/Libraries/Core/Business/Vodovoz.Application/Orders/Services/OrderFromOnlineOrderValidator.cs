using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.Services.Orders;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Models.Orders;
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
		private readonly IOrderOrganizationManager _orderOrganizationManager;
		private readonly IOrderSettings _orderSettings;
		private OnlineOrder _onlineOrder;
		private List<Error> _validationResults;
		private List<ICheckOnlineOrderSum> _calculatedOrderItemPrices;

		public OrderFromOnlineOrderValidator(
			IGoodsPriceCalculator goodsPriceCalculator,
			IOnlineOrderDeliveryPriceGetter deliveryPriceGetter,
			INomenclatureSettings nomenclatureSettings,
			IClientDeliveryPointsChecker clientDeliveryPointsChecker,
			IDiscountController discountController,
			IFreeLoaderChecker freeLoaderChecker,
			IOrderOrganizationManager orderOrganizationManager,
			IOrderSettings orderSettings)
		{
			_priceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			_deliveryPriceGetter = deliveryPriceGetter ?? throw new ArgumentNullException(nameof(deliveryPriceGetter));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_clientDeliveryPointsChecker = clientDeliveryPointsChecker ?? throw new ArgumentNullException(nameof(clientDeliveryPointsChecker));
			_discountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
			_freeLoaderChecker = freeLoaderChecker ?? throw new ArgumentNullException(nameof(freeLoaderChecker));
			_orderOrganizationManager = orderOrganizationManager ?? throw new ArgumentNullException(nameof(orderOrganizationManager));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
		}

		public Result ValidateOnlineOrder(IUnitOfWork uow, OnlineOrder onlineOrder)
		{
			_onlineOrder = onlineOrder;
			_validationResults = new List<Error>();
			_calculatedOrderItemPrices = new List<ICheckOnlineOrderSum>();

			if(_onlineOrder.IsSelfDelivery)
			{
				if(_onlineOrder.SelfDeliveryGeoGroup is null)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsEmptySelfDeliveryGeoGroup);
				}
			}
			else
			{
				if(_onlineOrder.Counterparty is null)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsEmptyCounterparty);
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
							_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.DeliveryPointNotBelongCounterparty);
						}
						
						_onlineOrder.SetDeliveryPointNotBelongCounterparty(!result);
					}
				}
				
				if(_onlineOrder.DeliveryPoint is null)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsEmptyDeliveryPoint);
				}
				else
				{
					if(_onlineOrder.DeliveryPoint.District is null)
					{
						_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsEmptyDistrictFromDeliveryPoint);
					}
				}

				if(_onlineOrder.DeliverySchedule is null)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsEmptyDeliverySchedule);
				}
			}
			
			if(_onlineOrder.DeliveryDate < DateTime.Today && _onlineOrder.OnlineOrderStatus == OnlineOrderStatus.New)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDeliveryDate);
			}

			if(!string.IsNullOrEmpty(onlineOrder.ContactPhone))
			{
				var phone = new PhoneEntity { Number = onlineOrder.ContactPhone };
				if(!phone.IsValidPhoneNumber)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.InvalidPhone(onlineOrder.ContactPhone));
				}
			}

			if(_orderOrganizationManager.SplitOrderByOrganizations(
				uow, DateTime.Now.TimeOfDay, OrderOrganizationChoice.Create(uow, _orderSettings, onlineOrder)).Count() > 1)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.OnlineOrderContainsGoodsSoldFromSeveralOrganizations());
			}

			ValidateOnlineOrderItems(uow);
			ValidateTotalSum();
			
			return !_validationResults.Any() ? Result.Success() : Result.Failure(_validationResults);
		}

		private void ValidateTotalSum()
		{
			var producedOrderSum = _onlineOrder.OnlineOrderSum;
			var currentOrderSum = _calculatedOrderItemPrices.Sum(x => x.Sum);

			if(producedOrderSum != currentOrderSum)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectOrderSum(producedOrderSum, currentOrderSum));
			}
		}

		private void ValidateOnlineOrderItems(IUnitOfWork uow)
		{
			var archivedNomenclatures = new Dictionary<int, bool>();
			ValidatePromoSet(uow, archivedNomenclatures);
			ValidateOtherItemsWithoutDeliveries(archivedNomenclatures);
			ValidatePaidDelivery();
			ValidateFastDelivery();
			ValidateOnlineRentPackages();
		}

		private void ValidatePromoSet(IUnitOfWork uow, IDictionary<int, bool> archivedNomenclatures)
		{
			var onlineOrderPromoSets = _onlineOrder.OnlineOrderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSetId);

			CheckFreeLoader(uow);
			
			foreach(var onlineOrderItemGroup in onlineOrderPromoSets)
			{
				var promoSet = onlineOrderItemGroup.First().PromoSet;
				
				if(promoSet.IsArchive)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsArchivedOnlineOrderPromoSet(promoSet.Title));
				}

				var promoSetItemsCount = promoSet.PromotionalSetItems.Count;
				var onlinePromoItemsCount = onlineOrderItemGroup.Count();
				
				CheckCorrectPromoSetItemsCount(promoSetItemsCount, onlinePromoItemsCount, promoSet.Title);
				CheckPromoSetForNewClientsCount(promoSetItemsCount, onlinePromoItemsCount, promoSet);
					
				var i = 0;
				foreach(var onlineOrderItem in onlineOrderItemGroup)
				{
					var checkOnlineOrderSum = CheckOnlineOrderSum.Create(onlineOrderItem.Nomenclature.Id, 0, 0, 0);
					
					ValidateNomenclatureByArchive(archivedNomenclatures, onlineOrderItem);
					ValidateCountFromPromoSet(onlineOrderItem, i, checkOnlineOrderSum);
					ValidatePrice(onlineOrderItem, checkOnlineOrderSum);
					ValidateDiscountFromPromoSet(onlineOrderItem, i, checkOnlineOrderSum);
					i++;

					if(i >= promoSetItemsCount)
					{
						i = 0;
					}
					
					_calculatedOrderItemPrices.Add(checkOnlineOrderSum);
				}
			}
		}

		private void CheckFreeLoader(IUnitOfWork uow)
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
			
			var contactNumber =
				!string.IsNullOrWhiteSpace(_onlineOrder.ContactPhone) && _onlineOrder.ContactPhone.Length > 2
					? _onlineOrder.ContactPhone.Substring(2)
					: null;

			var result = _freeLoaderChecker.CanOrderPromoSetForNewClientsFromOnline(
				uow,
				_onlineOrder.IsSelfDelivery,
				_onlineOrder.CounterpartyId,
				_onlineOrder.DeliveryPointId,
				contactNumber);

			if(result.IsSuccess)
			{
				return;
			}

			foreach(var error in result.Errors)
			{
				_validationResults.Add(error);
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
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsIncorrectOnlineOrderPromoSetItemsCount(promoSetTitle));
				}
			}
		}
		
		private void CheckPromoSetForNewClientsCount(
			int promoSetItemsCount,
			int onlinePromoItemsCount,
			PromotionalSet promoSet)
		{
			if(promoSetItemsCount < onlinePromoItemsCount && promoSet.PromotionalSetForNewClients)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsIncorrectOnlineOrderPromoSetForNewClientsCount());
			}
		}

		private void ValidateNomenclatureByArchive(IDictionary<int, bool> archivedNomenclatures, OnlineOrderItem onlineOrderItem)
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
			_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsArchivedNomenclatureInOnlineOrder(nomenclature.ToString()));
		}

		private void ValidateCountFromPromoSet(OnlineOrderItem onlineOrderItem, int index, CheckOnlineOrderSum checkOnlineOrderSum)
		{
			var promoSetItem = onlineOrderItem.PromoSet.PromotionalSetItems[index];
			var countFromPromoSetItem = promoSetItem.Count;

			onlineOrderItem.CountFromPromoSet = countFromPromoSetItem;
			checkOnlineOrderSum.Count = countFromPromoSetItem;

			if(countFromPromoSetItem != onlineOrderItem.Count)
			{
				_validationResults.Add(
					Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectCountNomenclatureInOnlineOrderPromoSet(
						onlineOrderItem.PromoSet.Title,
						++index,
						onlineOrderItem.Nomenclature.ToString(),
						countFromPromoSetItem,
						(int)onlineOrderItem.Count));
			}
		}

		private void ValidatePrice(OnlineOrderItem onlineOrderItem, CheckOnlineOrderSum checkOnlineOrderSum)
		{
			var price = _priceCalculator.CalculateItemPrice(
				_onlineOrder.OnlineOrderItems,
				_onlineOrder.DeliveryPoint,
				_onlineOrder.Counterparty,
				onlineOrderItem,
				false);

			onlineOrderItem.NomenclaturePrice = price;
			checkOnlineOrderSum.Price = price;

			if(price != onlineOrderItem.Price)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectPriceNomenclatureInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(), price, onlineOrderItem.Price));
			}
		}
		
		private void ValidateDiscountProperties(OnlineOrderItem onlineOrderItem, CheckOnlineOrderSum checkOnlineOrderSum)
		{
			ValidateDiscountParametersFromNotPromoSet(onlineOrderItem, checkOnlineOrderSum);
		}

		private void ValidateDiscountParametersFromNotPromoSet(OnlineOrderItem onlineOrderItem, CheckOnlineOrderSum checkOnlineOrderSum)
		{
			if(onlineOrderItem.DiscountReason != null)
			{
				var applicableDiscount =
					_discountController.IsApplicableDiscount(onlineOrderItem.DiscountReason, onlineOrderItem.Nomenclature);

				if(applicableDiscount)
				{
					ValidateApplicableDiscountFromNotPromoSet(onlineOrderItem, checkOnlineOrderSum);
				}
				else
				{
					ValidateNotApplicableDiscountFromNotPromoSet(onlineOrderItem);
				}
			}
			else
			{
				if(onlineOrderItem.GetDiscount > 0)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountNomenclatureInOnlineOrder(
						onlineOrderItem.Nomenclature.ToString(), 0, onlineOrderItem.GetDiscount));
				}
						
				onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
			}
		}

		private void ValidateApplicableDiscountFromNotPromoSet(OnlineOrderItem onlineOrderItem, CheckOnlineOrderSum checkOnlineOrderSum)
		{
			checkOnlineOrderSum.DiscountMoney =
				onlineOrderItem.DiscountReason.ValueType == DiscountUnits.money
					? onlineOrderItem.DiscountReason.Value
					: checkOnlineOrderSum.CalculateDiscountMoney(onlineOrderItem.DiscountReason.Value);
			
			if(onlineOrderItem.GetDiscount != onlineOrderItem.DiscountReason.Value)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountNomenclatureInOnlineOrder(
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
						_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountTypeInOnlineOrder(
							onlineOrderItem.Nomenclature.ToString(), true, onlineOrderItem.IsDiscountInMoney));
						onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
					}
					break;
				case DiscountUnits.percent:
					if(onlineOrderItem.IsDiscountInMoney)
					{
						_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountTypeInOnlineOrder(
							onlineOrderItem.Nomenclature.ToString(), false, onlineOrderItem.IsDiscountInMoney));
						onlineOrderItem.OnlineOrderErrorState = OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable;
					}
					break;
			}
		}

		private void ValidateNotApplicableDiscountFromNotPromoSet(OnlineOrderItem onlineOrderItem)
		{
			if(onlineOrderItem.GetDiscount > 0)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.NotApplicableDiscountToNomenclatureOnlineOrder(
					onlineOrderItem.Nomenclature.ToString()));
			}
							
			if(onlineOrderItem.IsDiscountInMoney)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountTypeInOnlineOrder(
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
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountTypeInOnlineOrderPromoSet(
					onlineOrderItem.PromoSet.Title,
					position,
					onlineOrderItem.Nomenclature.ToString(),
					discountInMoneyFromPromoSet,
					onlineOrderItem.IsDiscountInMoney));
			}
			
			if(discountItemFromPromoSet != onlineOrderItemDiscount)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectDiscountInOnlineOrderPromoSet(
					onlineOrderItem.PromoSet.Title,
					position,
					onlineOrderItem.Nomenclature.ToString(),
					discountItemFromPromoSet,
					onlineOrderItemDiscount));
			}
		}

		private void ValidateOtherItemsWithoutDeliveries(IDictionary<int, bool> archivedNomenclatures)
		{
			var onlineOrderItemsNotPromoSet =
				_onlineOrder.OnlineOrderItems
					.Where(x => x.PromoSet is null);
			
			foreach(var onlineOrderItem in onlineOrderItemsNotPromoSet)
			{
				if(onlineOrderItem.Nomenclature is null)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsIncorrectNomenclatureInOnlineOrder(onlineOrderItem.NomenclatureId));
					continue;
				}
				
				if(onlineOrderItem.NomenclatureId == _nomenclatureSettings.PaidDeliveryNomenclatureId
					|| onlineOrderItem.NomenclatureId == _nomenclatureSettings.FastDeliveryNomenclatureId)
				{
					continue;
				}
				
				var checkOnlineOrderSum = CheckOnlineOrderSum.Create(onlineOrderItem.Nomenclature.Id, onlineOrderItem.Count, 0, 0);
				
				ValidateNomenclatureByArchive(archivedNomenclatures, onlineOrderItem);
				ValidateCount(onlineOrderItem);
				ValidatePrice(onlineOrderItem, checkOnlineOrderSum);
				ValidateDiscountProperties(onlineOrderItem, checkOnlineOrderSum);
				
				_calculatedOrderItemPrices.Add(checkOnlineOrderSum);
			}
		}

		private void ValidateCount(OnlineOrderItem onlineOrderItem)
		{
			if(onlineOrderItem.Count <= 0)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectCountNomenclatureInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(), onlineOrderItem.Count));
			}
		}

		private void ValidatePaidDelivery()
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
			var checkOnlineOrderSum = CheckOnlineOrderSum.Create(
				_nomenclatureSettings.PaidDeliveryNomenclatureId, 1, deliveryPrice, 0);

			if(paidDelivery != null)
			{
				paidDelivery.NomenclaturePrice = deliveryPrice;
			}

			if(needPaidDelivery && paidDelivery != null)
			{
				if(paidDelivery.Price != deliveryPrice)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectPricePaidDelivery(deliveryPrice, paidDelivery.Price));
				}
			}
			else if(needPaidDelivery && paidDelivery is null)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.NeedPaidDelivery);
			}
			else if(!needPaidDelivery && paidDelivery != null)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.NotNeedPaidDelivery);
				checkOnlineOrderSum.Count = 0;
			}
			
			_calculatedOrderItemPrices.Add(checkOnlineOrderSum);
		}
		
		private void ValidateFastDelivery()
		{
			var fastDelivery =
				_onlineOrder.OnlineOrderItems
					.SingleOrDefault(x => x.PromoSet is null && x.NomenclatureId == _nomenclatureSettings.FastDeliveryNomenclatureId);
			
			if(!_onlineOrder.IsFastDelivery)
			{
				if(fastDelivery != null)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.FastDeliveryErrors.NotNeedFastDelivery);
				}
				return;
			}
			
			var checkOnlineOrderSum = CheckOnlineOrderSum.Create(_nomenclatureSettings.FastDeliveryNomenclatureId, 1, 0, 0);

			if(fastDelivery is null)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.FastDeliveryErrors.FastDeliveryIsMissing);
				return;
			}

			if(_onlineOrder.DeliveryDate != DateTime.Today)
			{
				_validationResults.Add(Vodovoz.Errors.Orders.FastDeliveryErrors.InvalidDate);
			}
			
			ValidatePrice(fastDelivery, checkOnlineOrderSum);
		}

		private void ValidateOnlineRentPackages()
		{
			foreach(var onlineRentPackage in _onlineOrder.OnlineRentPackages)
			{
				var freeRentPackage = onlineRentPackage.FreeRentPackage;
				if(freeRentPackage is null)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectRentPackageIdInOnlineOrder(onlineRentPackage.FreeRentPackageId));
					continue;
				}

				if(freeRentPackage.IsArchive)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsArchivedRentPackageIdInOnlineOrder(onlineRentPackage.FreeRentPackageId));
				}

				var depositFromRentPackage = freeRentPackage.Deposit;

				onlineRentPackage.FreeRentPackagePriceFromProgram = depositFromRentPackage;
				var checkOnlineOrderSum = CheckOnlineOrderSum.Create(
					freeRentPackage.DepositService.Id, onlineRentPackage.Count, depositFromRentPackage, 0);
				_calculatedOrderItemPrices.Add(checkOnlineOrderSum);

				if(depositFromRentPackage != onlineRentPackage.Price)
				{
					_validationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectRentPackagePriceInOnlineOrder(
						onlineRentPackage.FreeRentPackageId, onlineRentPackage.Price, depositFromRentPackage));
				}
			}
		}
	}
}
