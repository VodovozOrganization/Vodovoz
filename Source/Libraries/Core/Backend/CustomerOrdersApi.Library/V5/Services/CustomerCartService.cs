using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomerOrdersApi.Library.V5.Dto.Carts;
using CustomerOrdersApi.Library.V5.Dto.Orders;
using CustomerOrdersApi.Library.V5.Dto.Orders.OrderItem;
using CustomerOrdersApi.Library.V5.Dto.Orders.PromoSets;
using CustomerOrdersApi.Library.V5.Factories;
using QS.DomainModel.UoW;
using QS.Utilities;
using QS.Utilities.Extensions;
using Vodovoz.Core.Application.Orders.Services;
using Vodovoz.Core.Data.InfoMessages;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Handlers;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Nodes;
using VodovozBusiness.Services.Orders;

namespace CustomerOrdersApi.Library.V5.Services
{
	/// <inheritdoc/>
	public class CustomerCartService : ICustomerCartService
	{
		private readonly IUnitOfWork _uow;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IGeneralSettings _generalSettings;
		private readonly IGoodsPriceCalculator _goodsPriceCalculator;
		private readonly IOnlineOrderFromCartDistrictRulesGetter _deliveryRulesGetter;
		private readonly IInfoMessageFactoryV5 _infoMessageFactory;
		private readonly IWarningMessageFactoryV5 _warningMessageFactory;
		private readonly IOnlineOrderDiscountHandler _discountHandler;
		private readonly IStockRepository _stockRepository;
		private readonly IFreeLoaderChecker _freeLoaderChecker;

		public CustomerCartService(
			IUnitOfWork uow,
			INomenclatureSettings nomenclatureSettings,
			IGeneralSettings generalSettings,
			IGoodsPriceCalculator goodsPriceCalculator,
			IOnlineOrderFromCartDistrictRulesGetter deliveryRulesGetter,
			IInfoMessageFactoryV5 infoMessageFactory,
			IWarningMessageFactoryV5 warningMessageFactory,
			IOnlineOrderDiscountHandler discountHandler,
			IStockRepository stockRepository,
			IFreeLoaderChecker freeLoaderChecker)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_goodsPriceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			_deliveryRulesGetter = deliveryRulesGetter ?? throw new ArgumentNullException(nameof(deliveryRulesGetter));
			_infoMessageFactory = infoMessageFactory ?? throw new ArgumentNullException(nameof(infoMessageFactory));
			_warningMessageFactory = warningMessageFactory ?? throw new ArgumentNullException(nameof(warningMessageFactory));
			_discountHandler = discountHandler ?? throw new ArgumentNullException(nameof(discountHandler));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			_freeLoaderChecker = freeLoaderChecker ?? throw new ArgumentNullException(nameof(freeLoaderChecker));
		}
		
		/// <inheritdoc/>
		public CheckUsersBasketResponse Check(CheckUsersBasketRequest request)
		{
			var infoMessages = new List<InfoMessage>();
			var warnings = new List<WarningMessage>();
			var nomenclatureIdsAndCounts = new Dictionary<int, decimal>();
			var cartItemsCheck = new CartItemsCheck();
			
			var deliveryPoint = request.DeliveryPointId.HasValue ? _uow.GetById<DeliveryPoint>(request.DeliveryPointId.Value) : null;
			var counterparty = request.CounterpartyErpId.HasValue ? _uow.GetById<Counterparty>(request.CounterpartyErpId.Value) : null;

			ProcessOrderItems(request, cartItemsCheck, nomenclatureIdsAndCounts, out var checkedItems);
			var unavailablePromoSetWarning = ProcessPromoSets(request, cartItemsCheck, nomenclatureIdsAndCounts, out var checkedPromoSets);
			ProcessRentPackages(request, out var checkedPackages);
			TryAddWarning(unavailablePromoSetWarning, warnings);

			var stockWarning = CheckStock(nomenclatureIdsAndCounts, checkedItems, checkedPromoSets);
			TryAddWarning(stockWarning, warnings);

			var priceWarning = CheckPrices(request.Source, deliveryPoint, counterparty, cartItemsCheck);
			TryAddWarning(priceWarning, warnings);

			var onlineOrderFromCart = OnlineOrderFromCart.Create(deliveryPoint, cartItemsCheck.Goods, request.IsSelfDelivery, request.DeliveryDate);
			var deliveryWarning = CheckDelivery(onlineOrderFromCart, checkedItems);
			TryAddWarning(deliveryWarning, warnings);
			
			var promoCodeWarning = CheckDiscounts(request, cartItemsCheck);
			TryAddWarning(promoCodeWarning, warnings);

			var orderSum = CalculateOrderSum(checkedItems, checkedPromoSets, checkedPackages);
			var nextStep = SetNextStepAfterChecking(warnings);

			var response = CheckUsersBasketResponse.Create(
				Guid.NewGuid(),
				orderSum,
				nextStep,
				checkedItems,
				checkedPromoSets,
				checkedPackages,
				infoMessages,
				warnings);
			
			return response;
		}

		private OnlineOrderSumDto CalculateOrderSum(
			IEnumerable<CheckedOnlineOrderItemDto> onlineOrderItems,
			IEnumerable<CheckedPromoSetDto> promoSets,
			IEnumerable<CheckedOnlineRentPackageDto> rentPackages
			)
		{
			var orderSum = OnlineOrderSumDto.Create();

			foreach(var item in onlineOrderItems)
			{
				if(item.NomenclatureId == _nomenclatureSettings.PaidDeliveryNomenclatureId)
				{
					orderSum.Delivery = item.Price;
				}
				else
				{
					orderSum.Discount += item.SumWithoutDiscount - item.Sum;
					orderSum.RawSum += item.SumWithoutDiscount;
					orderSum.Total += item.Sum;
				}
			}
			
			foreach(var promoSet in promoSets)
			{
				orderSum.Discount += promoSet.SumWithoutDiscount - promoSet.Sum;
				orderSum.RawSum += promoSet.SumWithoutDiscount;
				orderSum.Total += promoSet.Sum;
			}
			
			foreach(var rentPackage in rentPackages)
			{
				orderSum.Discount = rentPackage.SumWithoutDiscount - rentPackage.Sum;
				orderSum.RawSum += rentPackage.SumWithoutDiscount;
				orderSum.Total += rentPackage.Sum;
			}
			
			return orderSum;
		}

		private WarningMessage CheckStock(
			IDictionary<int, decimal> nomenclatureIdsAndCounts,
			ICollection<CheckedOnlineOrderItemDto> checkedItems,
			ICollection<CheckedPromoSetDto> checkedPromoSets)
		{
			var warehouses = _generalSettings.WarehousesForPricesAndStocksIntegration;
			var stockResult = _stockRepository.NomenclatureInStock(_uow, nomenclatureIdsAndCounts.Keys.ToArray(), warehouses);
			var blockedCount = 0;

			foreach(var checkedItem in checkedItems)
			{
				foreach(var (key, count) in nomenclatureIdsAndCounts)
				{
					if(stockResult.ContainsKey(key) && stockResult[key] < count)
					{
						checkedItem.Status = CartItemStatus.Blocked;
						blockedCount++;
						break;
					}
				}
			}

			foreach(var checkedPromoSet in checkedPromoSets)
			{
				foreach(var (key, count) in nomenclatureIdsAndCounts)
				{
					if(stockResult.ContainsKey(key) && stockResult[key] < count)
					{
						checkedPromoSet.Status = CartItemStatus.Blocked;
						blockedCount++;
						break;
					}
				}
			}
			
			var cartItemsCount = checkedItems.Count + checkedPromoSets.Count;

			var warning = blockedCount switch
			{
				> 0 when blockedCount < cartItemsCount => _warningMessageFactory.CreateOutOfStockMessage(),
				> 0 when blockedCount >= cartItemsCount => _warningMessageFactory.CreateAllOutOfStockMessage(),
				_ => null
			};

			return warning;
		}

		private void ProcessOrderItems(
			CheckUsersBasketRequest request,
			CartItemsCheck cartItemsCheck,
			IDictionary<int, decimal> nomenclatureIdsAndCounts,
			out ICollection<CheckedOnlineOrderItemDto> checkedItems)
		{
			checkedItems = new List<CheckedOnlineOrderItemDto>();
			
			foreach(var item in request.OnlineOrderItems)
			{
				var checkedItem = CheckedOnlineOrderItemDto.Create(item);
				var nomenclature = _uow.GetById<Nomenclature>(item.NomenclatureId);
				var discountReason = item.DiscountReasonId != null ? _uow.GetById<DiscountReason>(item.DiscountReasonId.Value) : null;
				
				if(nomenclature is null || nomenclature.IsArchive)
				{
					checkedItem.Status = CartItemStatus.Blocked;
					continue;
				}
				
				AddNomenclaturesCount(nomenclatureIdsAndCounts, checkedItem.NomenclatureId, checkedItem.Count);
				var goods = Goods.Create(item.Price, item.Count, nomenclature, null, discountReason);
				checkedItems.Add(checkedItem);
				cartItemsCheck.AddItem(checkedItem, goods);
			}
		}

		private WarningMessage ProcessPromoSets(
			CheckUsersBasketRequest request,
			CartItemsCheck cartItemsCheck,
			IDictionary<int, decimal> nomenclatureIdsAndCounts,
			out ICollection<CheckedPromoSetDto> checkedPromoSets)
		{
			WarningMessage warningMessage = null;
			checkedPromoSets = new List<CheckedPromoSetDto>();
			
			foreach(var onlinePromoSet in request.PromoSets)
			{
				var checkedSet = CheckedPromoSetDto.Create(onlinePromoSet);
				var promoSet = _uow.GetById<PromotionalSet>(onlinePromoSet.PromoSetId);

				if(promoSet is null || promoSet.IsArchive)
				{
					checkedSet.Status = CartItemStatus.Blocked;
					continue;
				}

				foreach(var item in promoSet.ObservablePromotionalSetItems)
				{
					AddNomenclaturesCount(nomenclatureIdsAndCounts, item.Nomenclature.Id, item.Count);
					var goods = Goods.Create(item.Nomenclature.GetPrice(item.Count), item.Count, item.Nomenclature, promoSet, null);
					cartItemsCheck.AddPromoSet(checkedSet, goods);
				}
				
				checkedSet.PromotionalSetForNewClients = promoSet.PromotionalSetForNewClients;
				checkedPromoSets.Add(checkedSet);
			}
			
			warningMessage = CheckFreeLoader(request, checkedPromoSets);
			
			return warningMessage;
		}

		private WarningMessage CheckFreeLoader(
			CheckUsersBasketRequest request,
			IEnumerable<CheckedPromoSetDto> promoSets)
		{
			WarningMessage warningMessage = null;
			var hasPromoSetForNewClients = promoSets.Any(x => x.PromotionalSetForNewClients);

			if(!hasPromoSetForNewClients)
			{
				return null;
			}
			
			string contactNumber = null;

			var result = _freeLoaderChecker.CanOrderPromoSetForNewClientsFromOnline(
				_uow,
				request.IsSelfDelivery,
				request.CounterpartyErpId,
				request.DeliveryPointId,
				contactNumber);

			if(result.IsSuccess)
			{
				return null;
			}
			
			foreach(var promoSet in promoSets)
			{
				if(promoSet.PromotionalSetForNewClients)
				{
					promoSet.Status = CartItemStatus.Blocked;
				}
			}
			
			warningMessage = _warningMessageFactory.CreatePromoSetUnavailableMessage();

			return warningMessage;
		}

		private void ProcessRentPackages(
			CheckUsersBasketRequest request,
			out ICollection<CheckedOnlineRentPackageDto> checkedPackages)
		{
			checkedPackages = new List<CheckedOnlineRentPackageDto>();
			
			foreach(var rentPackage in request.OnlineRentPackages)
			{
				var checkedPackage = CheckedOnlineRentPackageDto.Create(rentPackage);
				var package = _uow.GetById<FreeRentPackage>(rentPackage.RentPackageId);

				if(package is null || package.IsArchive)
				{
					checkedPackage.Status = CartItemStatus.Blocked;
					continue;
				}
				
				checkedPackages.Add(checkedPackage);
			}
		}
		
		private void AddNomenclaturesCount(
			IDictionary<int, decimal> nomenclatureIdsAndCounts,
			int nomenclatureId,
			decimal count)
		{
			if(!nomenclatureIdsAndCounts.ContainsKey(nomenclatureId))
			{
				nomenclatureIdsAndCounts.Add(nomenclatureId, count);
			}
			else
			{
				nomenclatureIdsAndCounts[nomenclatureId] += count;
			}
		}

		private WarningMessage CheckDelivery(
			IOnlineOrderFromCart onlineOrder,
			ICollection<CheckedOnlineOrderItemDto> checkedItems)
		{
			WarningMessage warningMessage = null;

			var result = _deliveryRulesGetter.GetDeliveryRules(onlineOrder);

			if(result.IsFailure)
			{
				//TODO нужно обговорить этот сценарий, когда не найден наш логистический район
				//будет отдельное сообщение
				return _warningMessageFactory.CreateDistrictNotFoundMessage();
			}
			
			var districtRules = result.Value;

			if(districtRules is null)
			{
				return null;
			}
			
			var calculatedDeliveryPrice = districtRules.Max(x => x.Price);
			var currentDeliveryPriceItem = checkedItems
				.FirstOrDefault(x => x.NomenclatureId == _nomenclatureSettings.PaidDeliveryNomenclatureId);
			var currentDeliveryPrice = currentDeliveryPriceItem?.Price;

			if(calculatedDeliveryPrice == currentDeliveryPrice)
			{
				return null;
			}
			
			switch(currentDeliveryPrice)
			{
				case null when calculatedDeliveryPrice == 0:
					return null;
				case null:
					checkedItems.Add(new CheckedOnlineOrderItemDto
					{
						Count = 1,
						Discount = 0,
						DiscountReasonId = null,
						IsDiscountInMoney = false,
						IsFixedPrice = false,
						NomenclatureId = _nomenclatureSettings.PaidDeliveryNomenclatureId,
						Price = calculatedDeliveryPrice,
						PriceWithoutDiscount = null,
						PromoSetId = null,
						Status = CartItemStatus.Active
					});
					break;
				default:
				{
					if(calculatedDeliveryPrice == 0)
					{
						checkedItems.Remove(currentDeliveryPriceItem);
					}
					else
					{
						currentDeliveryPriceItem.Price = calculatedDeliveryPrice;
						currentDeliveryPriceItem.PriceWithoutDiscount = null;
					}

					break;
				}
			}

			warningMessage = GetDeliveryChangedWarningMessage(districtRules as IList<DistrictRuleItemBase>);

			return warningMessage;
		}

		private WarningMessage GetDeliveryChangedWarningMessage(IList<DistrictRuleItemBase> districtRuleItems)
		{
			var sb = new StringBuilder();
			
			districtRuleItems.MergeSort((x, y) =>
			{
				if(x.Price == y.Price)
				{
					return 0;
				}

				//Сортируем по убыванию
				if(x.Price < y.Price)
				{
					return 1;
				}

				return -1;
			});

			var total19L = _deliveryRulesGetter.OnlineOrderFromCartStateKey.DisposableWater19LCount +
				_deliveryRulesGetter.OnlineOrderFromCartStateKey.NotDisposableWater19LCount;

			var total1500ml = _deliveryRulesGetter.OnlineOrderFromCartStateKey.DisposableWater1500mlCount;
			
			var bottlesStingBuilder = new StringBuilder();

			for(var i = 0; i < districtRuleItems.Count; i++)
			{
				bottlesStingBuilder.Clear();
				
				bottlesStingBuilder.Append($"{districtRuleItems[i].DeliveryPriceRule.Water19LCount - total19L} 19л");

				if(total1500ml != 0)
				{
					bottlesStingBuilder.Append(" или ");
					bottlesStingBuilder.Append($"{districtRuleItems[i].DeliveryPriceRule.Water1500mlCount - total1500ml} 1.5л");
				}

				bottlesStingBuilder.Append(" бутылок");
				
				string deliveryMessage = null;
				const string message = "добавьте в заказ {0}, чтобы доставка стала {1}";
				
				if(i != districtRuleItems.Count - 1)
				{
					var deliveryPrice = districtRuleItems[i + 1].Price;
					deliveryMessage = $"{deliveryPrice}{CurrencyWorks.CurrencyShortFormat}";
					sb.AppendLine(string.Format(message, bottlesStingBuilder, deliveryMessage));
					sb.AppendLine("или");
				}
				else
				{
					deliveryMessage = "бесплатной";
					sb.AppendLine(string.Format(message, bottlesStingBuilder, deliveryMessage));
				}
			}
			
			return _warningMessageFactory.CreateDeliveryChangedMessage(sb.ToString());
		}

		private WarningMessage CheckDiscounts(
			CheckUsersBasketRequest request,
			CartItemsCheck cartItemsCheck)
		{
			//пока проверяем только промокод
			WarningMessage warningMessage = null;
			
			foreach(var (checkedItem, goods) in cartItemsCheck.CombinedItems)
			{
				var discountCheck = _discountHandler.IsApplicableDiscount(
					_uow,
					request.Source,
					request.CounterpartyErpId,
					request.OrderSum.RawSum,
					DateTime.Now,
					goods);
				
				if(!discountCheck.PromoCodeValid.HasValue || !discountCheck.PromoCodeValid.Value)
				{
					warningMessage = _warningMessageFactory.CreatePromoCodeUnavailableMessage();
					break;
				}
			}

			if(warningMessage != null)
			{
				return warningMessage;
			}
			
			foreach(var (checkedPromoSet, goods) in cartItemsCheck.CombinedPromoSets)
			{
				var discountCheck = _discountHandler.IsApplicableDiscount(
					_uow,
					request.Source,
					request.CounterpartyErpId,
					request.OrderSum.RawSum,
					DateTime.Now,
					goods);
				
				if(!discountCheck.PromoCodeValid.HasValue || !discountCheck.PromoCodeValid.Value)
				{
					warningMessage = _warningMessageFactory.CreatePromoCodeUnavailableMessage();
					break;
				}
			}

			return warningMessage;
		}

		private WarningMessage CheckPrices(
			Source source,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			CartItemsCheck cartItemsCheck)
		{
			WarningMessage warning = null;

			CheckPriceFromItems(source, deliveryPoint, counterparty, cartItemsCheck, warning);
			CheckPriceFromPromoSets(source, deliveryPoint, counterparty, cartItemsCheck, warning);

			return warning;
		}

		private void CheckPriceFromItems(
			Source source,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			CartItemsCheck cartItemsCheck,
			WarningMessage warning)
		{
			foreach(var (checkedItem, product) in cartItemsCheck.CombinedItems)
			{
				var price = _goodsPriceCalculator.CalculateItemPrice(
					cartItemsCheck.Goods,
					deliveryPoint,
					counterparty,
					product,
					false);
				
				var onlineParameters = product.Nomenclature.GetNomenclatureOnlineParameters(source);
				var onlinePrice = onlineParameters?.GetOnlinePrice(product.Count);

				var priceWithoutDiscount = onlinePrice?.PriceWithoutDiscount;
				checkedItem.PriceWithoutDiscount = priceWithoutDiscount;
				checkedItem.Marker = onlineParameters?.NomenclatureOnlineMarker;

				if(price != product.Price)
				{
					checkedItem.Price = price;
					warning ??= _warningMessageFactory.CreatePriceChangedMessage();
				}
			}
		}
		
		private void CheckPriceFromPromoSets(
			Source source,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			CartItemsCheck cartItemsCheck,
			WarningMessage warning)
		{
			var lookupPromoSets = cartItemsCheck.CombinedPromoSets.ToLookup(x => x.CheckedPromoSet);
			
			//TODO проверка на халявщика
			
			foreach(var groupedPromoSets in lookupPromoSets)
			{
				var firstGroup = groupedPromoSets.FirstOrDefault();
				var price = firstGroup.Product.PromoSet.Sum;
				var checkedPromoSet = firstGroup.CheckedPromoSet;

				if(price != checkedPromoSet.Price)
				{
					checkedPromoSet.Price = price;
					warning ??= _warningMessageFactory.CreatePriceChangedMessage();
				}
			}
		}
		
		private NextStepCheckUsersBasket SetNextStepAfterChecking(IEnumerable<WarningMessage> warnings)
		{
			NextStepCheckUsersBasket? nextStep = null;
			
			foreach(var warning in warnings)
			{
				switch(warning.Type)
				{
					case nameof(WarningMessageType.AllItemsOutOfStock):
						nextStep = NextStepCheckUsersBasket.Blocked;
						break;
					case nameof(WarningMessageType.ItemOutOfStock)
						or nameof(WarningMessageType.PriceChanged)
						or nameof(WarningMessageType.DeliveryChanged)
						or nameof(WarningMessageType.PromoSetInvalid)
						or nameof(WarningMessageType.PromoCodeInvalid):
						if(nextStep != NextStepCheckUsersBasket.Blocked)
						{
							nextStep = NextStepCheckUsersBasket.ProceedWithWarnings;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException($"Неизвестный WarningMessageType {warning.Type}");
				}
			}
			
			return nextStep ?? NextStepCheckUsersBasket.Proceed;
		}
		
		private static void TryAddWarning(
			WarningMessage unavailablePromoSetWarning,
			ICollection<WarningMessage> warnings)
		{
			if(unavailablePromoSetWarning != null)
			{
				warnings.Add(unavailablePromoSetWarning);
			}
		}
	}
}
