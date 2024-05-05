using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.Errors;
using Vodovoz.Services.Orders;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Application.Orders.Services
{
	public class OrderFromOnlineOrderValidator : IOrderFromOnlineOrderValidator
	{
		private readonly IGoodsPriceCalculator _priceCalculator;
		private readonly IDeliveryPriceCalculator _deliveryPriceCalculator;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private OnlineOrder _onlineOrder;

		public OrderFromOnlineOrderValidator(
			IGoodsPriceCalculator goodsPriceCalculator,
			IDeliveryPriceCalculator deliveryPriceCalculator,
			INomenclatureSettings nomenclatureSettings)
		{
			_priceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			_deliveryPriceCalculator = deliveryPriceCalculator ?? throw new ArgumentNullException(nameof(deliveryPriceCalculator));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
		}
		
		public Result ValidateOnlineOrder(OnlineOrder onlineOrder)
		{
			_onlineOrder = onlineOrder;
			var validationResults = new List<Error>();
			
			if(_onlineOrder.IsSelfDelivery)
			{
				if(_onlineOrder.SelfDeliveryGeoGroup is null)
				{
					validationResults.Add(Errors.Orders.OnlineOrder.IsEmptySelfDeliveryGeoGroup);
				}
			}
			else
			{
				if(_onlineOrder.Counterparty is null)
				{
					validationResults.Add(Errors.Orders.OnlineOrder.IsEmptyCounterparty);
				}
				
				if(_onlineOrder.DeliveryPoint is null)
				{
					validationResults.Add(Errors.Orders.OnlineOrder.IsEmptyDeliveryPoint);
				}
				else
				{
					if(_onlineOrder.DeliveryPoint.District is null)
					{
						validationResults.Add(Errors.Orders.OnlineOrder.IsEmptyDistrictFromDeliveryPoint);
					}
				}

				if(_onlineOrder.DeliverySchedule is null)
				{
					validationResults.Add(Errors.Orders.OnlineOrder.IsEmptyDeliverySchedule);
				}
			}
			
			if(_onlineOrder.DeliveryDate < DateTime.Today && _onlineOrder.OnlineOrderStatus == OnlineOrderStatus.New)
			{
				validationResults.Add(Errors.Orders.OnlineOrder.IncorrectDeliveryDate);
			}

			ValidateOnlineOrderItems(validationResults);
			//CanShipPromoSets(validationResults);
			//CanFastDelivery(validationResults);
			
			return !validationResults.Any() ? Result.Success() : Result.Failure(validationResults);
		}

		private void CanFastDelivery(List<Error> validationResults)
		{
			throw new NotImplementedException();
		}

		private void CanShipPromoSets(List<Error> validationResults)
		{
			throw new NotImplementedException();
		}

		private void ValidateOnlineOrderItems(ICollection<Error> errors)
		{
			var archivedNomenclatures = new Dictionary<int, bool>();
			ValidatePromoSet(archivedNomenclatures, errors);
			ValidateOtherItemsWithoutPaidDelivery(archivedNomenclatures, errors);
			ValidatePaidDelivery(errors);
			ValidateOnlineRentPackages(errors);
		}

		private void ValidatePromoSet(IDictionary<int, bool> archivedNomenclatures, ICollection<Error> errors)
		{
			var onlineOrderPromoSets = _onlineOrder.OnlineOrderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSetId);
			
			foreach(var onlineOrderItemGroup in onlineOrderPromoSets)
			{
				var promoSet = onlineOrderItemGroup.First().PromoSet;
				
				if(promoSet.IsArchive)
				{
					errors.Add(Errors.Orders.OnlineOrder.IsArchivedOnlineOrderPromoSet(promoSet.Title));
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
					ValidateDiscount(onlineOrderItem, i, errors);
					i++;

					if(i >= promoSetItemsCount)
					{
						i = 0;
					}
				}
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
					errors.Add(Errors.Orders.OnlineOrder.IsIncorrectOnlineOrderPromoSetItemsCount(promoSetTitle));
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
				errors.Add(Errors.Orders.OnlineOrder.IsIncorrectOnlineOrderPromoSetForNewClientsCount());
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
			errors.Add(Errors.Orders.OnlineOrder.IsArchivedNomenclatureInOnlineOrder(nomenclature.ToString()));
		}

		private void ValidateCount(OnlineOrderItem onlineOrderItem, int index, ICollection<Error> errors)
		{
			var promoSetItem = onlineOrderItem.PromoSet.PromotionalSetItems[index];
			var countFromPromoSetItem = promoSetItem.Count;

			onlineOrderItem.CountFromPromoSet = countFromPromoSetItem;

			if(countFromPromoSetItem != onlineOrderItem.Count)
			{
				errors.Add(
					Errors.Orders.OnlineOrder.IncorrectCountNomenclatureInOnlineOrderPromoSet(
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
				errors.Add(Errors.Orders.OnlineOrder.IncorrectPriceNomenclatureInOnlineOrder(
					onlineOrderItem.Nomenclature.ToString(), price, onlineOrderItem.Price));
			}
		}
		
		private void ValidateDiscount(OnlineOrderItem onlineOrderItem, int index, ICollection<Error> errors)
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
				errors.Add(Errors.Orders.OnlineOrder.IncorrectDiscountTypeInOnlineOrderPromoSet(
					onlineOrderItem.PromoSet.Title,
					position,
					onlineOrderItem.Nomenclature.ToString(),
					discountInMoneyFromPromoSet,
					onlineOrderItem.IsDiscountInMoney));
			}
			
			if(discountItemFromPromoSet != onlineOrderItemDiscount)
			{
				errors.Add(Errors.Orders.OnlineOrder.IncorrectDiscountInOnlineOrderPromoSet(
					onlineOrderItem.PromoSet.Title,
					position,
					onlineOrderItem.Nomenclature.ToString(),
					discountItemFromPromoSet,
					onlineOrderItemDiscount));
			}
		}

		private void ValidateOtherItemsWithoutPaidDelivery(IDictionary<int, bool> archivedNomenclatures, ICollection<Error> errors)
		{
			var onlineOrderItemsNotPromoSet =
				_onlineOrder.OnlineOrderItems
					.Where(x => x.PromoSet is null);
			
			foreach(var onlineOrderItem in onlineOrderItemsNotPromoSet)
			{
				if(onlineOrderItem.Nomenclature is null)
				{
					errors.Add(Errors.Orders.OnlineOrder.IsIncorrectNomenclatureInOnlineOrder(onlineOrderItem.NomenclatureId));
					continue;
				}
				
				if(onlineOrderItem.NomenclatureId == _nomenclatureSettings.PaidDeliveryNomenclatureId)
				{
					continue;
				}
				
				ValidateNomenclatureByArchive(archivedNomenclatures, onlineOrderItem, errors);
				ValidatePrice(onlineOrderItem, errors);
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

			var deliveryPrice = _deliveryPriceCalculator.CalculateDeliveryPrice(_onlineOrder);
			var needPaidDelivery = deliveryPrice > 0;

			if(needPaidDelivery && paidDelivery != null)
			{
				if(paidDelivery.Price != deliveryPrice)
				{
					errors.Add(Errors.Orders.OnlineOrder.IncorrectPricePaidDelivery(deliveryPrice, paidDelivery.Price));
				}
			}
			else if(needPaidDelivery && paidDelivery is null)
			{
				errors.Add(Errors.Orders.OnlineOrder.NeedPaidDelivery);
			}
			else if(!needPaidDelivery && paidDelivery != null)
			{
				errors.Add(Errors.Orders.OnlineOrder.NotNeedPaidDelivery);
			}
		}

		private void ValidateOnlineRentPackages(ICollection<Error> errors)
		{
			foreach(var onlineRentPackage in _onlineOrder.OnlineRentPackages)
			{
				var freeRentPackage = onlineRentPackage.FreeRentPackage;
				if(freeRentPackage is null)
				{
					errors.Add(Errors.Orders.OnlineOrder.IncorrectRentPackageIdInOnlineOrder(onlineRentPackage.FreeRentPackageId));
					continue;
				}

				if(freeRentPackage.IsArchive)
				{
					errors.Add(Errors.Orders.OnlineOrder.IsArchivedRentPackageIdInOnlineOrder(onlineRentPackage.FreeRentPackageId));
				}

				var depositFromRentPackage = freeRentPackage.Deposit;

				onlineRentPackage.FreeRentPackagePriceFromProgram = depositFromRentPackage;

				if(depositFromRentPackage != onlineRentPackage.Price)
				{
					errors.Add(Errors.Orders.OnlineOrder.IncorrectRentPackagePriceInOnlineOrder(
						onlineRentPackage.FreeRentPackageId, onlineRentPackage.Price, depositFromRentPackage));
				}
			}
		}
	}
}
