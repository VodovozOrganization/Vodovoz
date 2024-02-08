using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Errors;
using Vodovoz.Extensions;
using Vodovoz.Factories;
using Vodovoz.Services;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.Domain.Service
{
	public class OrderFromOnlineOrderCreator
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderParametersProvider _orderParametersProvider;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IPromotionalSetRepository _promotionalSetRepository;
		private readonly ICounterpartyContractRepository _counterpartyContractRepository;
		private readonly ICounterpartyContractFactory _counterpartyContractFactory;
		private readonly FastDeliveryHandler _fastDeliveryHandler;
		private readonly OrderFromOnlineOrderValidator _onlineOrderValidator;

		public OrderFromOnlineOrderCreator(
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderParametersProvider orderParametersProvider,
			INomenclatureRepository nomenclatureRepository,
			ICallTaskWorker callTaskWorker,
			IEmployeeRepository employeeRepository,
			IPromotionalSetRepository promotionalSetRepository,
			ICounterpartyContractRepository counterpartyContractRepository,
			ICounterpartyContractFactory counterpartyContractFactory,
			FastDeliveryHandler fastDeliveryHandler,
			OrderFromOnlineOrderValidator onlineOrderValidator)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_counterpartyContractRepository =
				counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
			_counterpartyContractFactory =
				counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
			_fastDeliveryHandler = fastDeliveryHandler ?? throw new ArgumentNullException(nameof(fastDeliveryHandler));
			_onlineOrderValidator = onlineOrderValidator ?? throw new ArgumentNullException(nameof(onlineOrderValidator));
		}

		public int CreateOrderFromOnlineOrderAndTryAccept(OnlineOrder onlineOrder)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var orderCreator = _employeeRepository.GetEmployeeForCurrentUser(uow);
				var order = new Order
				{
					UoW = uow
				};
				
				var paymentFrom = onlineOrder.OnlinePaymentSource.HasValue
					? uow.GetById<PaymentFrom>(
						onlineOrder.OnlinePaymentSource.Value.ConvertToPaymentFromId(_orderParametersProvider))
					: null;

				order.Author = orderCreator;
				order.Client = onlineOrder.Counterparty;
				order.DeliveryPoint = onlineOrder.DeliveryPoint;
				order.DeliveryDate = onlineOrder.DeliveryDate;
				order.DeliverySchedule = onlineOrder.DeliverySchedule;
				order.SelfDelivery = onlineOrder.IsSelfDelivery;
				order.IsFastDelivery = onlineOrder.IsFastDelivery;
				order.PaymentType = onlineOrder.OnlineOrderPaymentType.ConvertToOrderPaymentType();
				order.BottlesReturn = onlineOrder.BottlesReturn;
				order.OnlineOrder = onlineOrder.OnlinePayment;
				order.PaymentByCardFrom = paymentFrom;
				order.Trifle = onlineOrder.Trifle;
				order.Comment = onlineOrder.OnlineOrderComment;
				
				order.UpdateOrCreateContract(uow, _counterpartyContractRepository, _counterpartyContractFactory);

				//TODO проверка доступности быстрой доставки, если заказ с быстрой доставкой
				
				//заполнить строки заказа
				AddOrderItems(order, onlineOrder.OnlineOrderItems);
				
				//добавить аренду
				AddFreeRentPackages(order, onlineOrder.OnlineRentPackages);
				
				//TODO необходимо перепроверить нужность вызова этого метода
				//order.RecalculateItemsPrice();
				
				//TODO необходимо разобраться с доступом к методу расчета платной доставки
				//UpdateDeliveryCost(uowNewOrder, order);
				order.AddDeliveryPointCommentToOrder();
				order.AddFastDeliveryNomenclatureIfNeeded();
				
				//TODO проверка возможности добавления промонаборов
				var validationResult = _onlineOrderValidator.ValidateOnlineOrder(onlineOrder);//валидируем онлайн заказ на правильность заполнения данных
				
				//uowNewOrder.Save();

				if(validationResult.IsFailure)
				{
					//добавлем записи в ошибки
					onlineOrder.OrderWarnings = validationResult.GetErrorsString();
					
					uow.Save(onlineOrder);
					uow.Commit();
					
					return 0;
				}

				var acceptResult = TryAcceptOrder(uow, order);
				if(acceptResult.IsFailure)
				{
					//добавлем записи в ошибки
					onlineOrder.OrderWarnings = acceptResult.GetErrorsString();
					
					uow.Save(onlineOrder);
					uow.Commit();
					
					return 0;
				}
				
				uow.Save(order);
				uow.Commit();
				
				return order.Id;
			}
		}

		private Result TryAcceptOrder(IUnitOfWork uow, Order order)
		{
			var hasPromoSetForNewClients = order.PromotionalSets.Any(x => x.PromotionalSetForNewClients);
			
			if(hasPromoSetForNewClients && order.HasUsedPromoForNewClients(_promotionalSetRepository))
			{
				return Result.Failure(Errors.Orders.Order.UnableToShipPromoSet);
			}
			
			//Проверяем доступность быстрой доставки
			var fastDeliveryResult = _fastDeliveryHandler.CheckFastDelivery(uow, order);
			
			if(fastDeliveryResult.IsFailure)
			{
				return fastDeliveryResult;
			}
			
			order.AcceptOrder(order.Author, _callTaskWorker);
			//order.SaveEntity();
			
			return Result.Success();
		}

		private void AddOrderItems(Order order, IList<OnlineOrderItem> onlineOrderItems)
		{
			foreach(var onlineOrderItem in onlineOrderItems)
			{
				if(onlineOrderItem.PromoSet != null)
				{
					order.AddNomenclature(
						onlineOrderItem.Nomenclature,
						onlineOrderItem.Count,
						onlineOrderItem.Discount,
						onlineOrderItem.IsDiscountInMoney,
						null,
						onlineOrderItem.PromoSet);
				}
				else
				{
					order.AddNomenclature(onlineOrderItem.Nomenclature, onlineOrderItem.Count);
				}
			}
		}
		
		private void AddFreeRentPackages(Order order, IList<OnlineRentPackage> onlineRentPackages)
		{
			foreach(var onlineRentPackage in onlineRentPackages)
			{
				var rentPackage = onlineRentPackage.RentPackage;
				
				var existingItems = order.OrderEquipments
					.Where(x => x.OrderRentDepositItem != null || x.OrderRentServiceItem != null)
					.Select(x => x.Nomenclature.Id)
					.Distinct()
					.ToArray();

				var anyNomenclature = _nomenclatureRepository.GetAvailableNonSerialEquipmentForRent(
					order.UoW,
					rentPackage.EquipmentKind,
					existingItems);
				
				order.AddFreeRent(rentPackage, anyNomenclature);
			}
		}
	}

	public class OrderFromOnlineOrderValidator
	{
		private readonly GoodsPriceCalculator _priceCalculator;
		private OnlineOrder _onlineOrder;

		public OrderFromOnlineOrderValidator(GoodsPriceCalculator priceCalculator)
		{
			_priceCalculator = priceCalculator;
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
				if(_onlineOrder.DeliveryPoint is null)
				{
					validationResults.Add(Errors.Orders.OnlineOrder.IsEmptyDeliveryPoint);
				}

				if(_onlineOrder.DeliverySchedule is null)
				{
					validationResults.Add(Errors.Orders.OnlineOrder.IsEmptyDeliverySchedule);
				}
			}

			ValidateOnlineOrderItems(validationResults);
			
			return validationResults.Any() ? Result.Success() : Result.Failure(validationResults);
		}

		public void ValidateOnlineOrderItems(ICollection<Error> errors)
		{
			ValidatePromoSet(errors);
			ValidateOtherItems(errors);
			ValidateOnlineRentPackages(errors);
		}

		private void ValidatePromoSet(ICollection<Error> errors)
		{
			var onlineOrderPromoSets = _onlineOrder.OnlineOrderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSetId);
			
			foreach(var onlineOrderItemGroup in onlineOrderPromoSets)
			{
				var i = 0;
				foreach(var onlineOrderItem in onlineOrderItemGroup)
				{
					ValidateCount(onlineOrderItem, i, errors);
					ValidatePrice(onlineOrderItem, errors);
					ValidateDiscount(onlineOrderItem, i, errors);
					i++;
				}
			}
		}
		
		private void ValidateCount(OnlineOrderItem onlineOrderItem, int index, ICollection<Error> errors)
		{
			var countFromPromoSet = onlineOrderItem.PromoSet.PromotionalSetItems[index].Count;

			if(countFromPromoSet != onlineOrderItem.Count)
			{
				errors.Add(
					Errors.Orders.OnlineOrder.IncorrectCountNomenclatureInOnlineOrderPromoSet(
						onlineOrderItem.PromoSet.Title,
						++index,
						onlineOrderItem.Nomenclature.ToString(),
						countFromPromoSet,
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

			if(discountInMoneyFromPromoSet != onlineOrderItem.IsDiscountInMoney)
			{
				errors.Add(Errors.Orders.OnlineOrder.IncorrectDiscountTypeInOnlineOrderPromoSet(
					onlineOrderItem.PromoSet.Title,
					++index,
					onlineOrderItem.Nomenclature.ToString(),
					discountInMoneyFromPromoSet,
					onlineOrderItem.IsDiscountInMoney));
			}
			
			if(discountItemFromPromoSet != onlineOrderItem.Discount)
			{
				errors.Add(Errors.Orders.OnlineOrder.IncorrectDiscountInOnlineOrderPromoSet(
					onlineOrderItem.PromoSet.Title,
					++index,
					onlineOrderItem.Nomenclature.ToString(),
					discountItemFromPromoSet,
					onlineOrderItem.Discount));
			}
		}

		private void ValidateOtherItems(ICollection<Error> errors)
		{
			var onlineOrderItemsNotPromoSet =
				_onlineOrder.OnlineOrderItems
				.Where(x => x.PromoSet == null);
			
			foreach(var onlineOrderItem in onlineOrderItemsNotPromoSet)
			{
				ValidatePrice(onlineOrderItem, errors);
			}
		}

		private void ValidateOnlineRentPackages(ICollection<Error> errors)
		{
			foreach(var onlineRentPackage in _onlineOrder.OnlineRentPackages)
			{
				var depositFromRentPackage = onlineRentPackage.RentPackage.Deposit;

				if(depositFromRentPackage != onlineRentPackage.Price)
				{
					errors.Add(Errors.Orders.OnlineOrder.IncorrectRentPackagePriceInOnlineOrder(
						onlineRentPackage.RentPackageId, onlineRentPackage.Price, depositFromRentPackage));
				}
			}
		}
	}
}
