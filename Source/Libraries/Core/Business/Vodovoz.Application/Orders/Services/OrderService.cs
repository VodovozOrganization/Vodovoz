﻿using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Sms.Internal;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Controllers;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Factories;
using Vodovoz.Models;
using Vodovoz.Models.Orders;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Application.Orders.Services
{
	internal sealed class OrderService : IOrderService
	{
		private readonly ILogger<OrderService> _logger;

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IOrderDailyNumberController _orderDailyNumberController;
		private readonly IPaymentFromBankClientController _paymentFromBankClientController;
		private readonly ICounterpartyContractRepository _counterpartyContractRepository;
		private readonly ICounterpartyContractFactory _counterpartyContractFactory;
		private readonly IFastPaymentSender _fastPaymentSender;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IGenericRepository<DiscountReason> _discountReasonRepository;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrderDiscountsController _orderDiscountsController;

		public OrderService(
			ILogger<OrderService> logger,
			INomenclatureSettings nomenclatureSettings,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeRepository employeeRepository,
			IOrderDailyNumberController orderDailyNumberController,
			IPaymentFromBankClientController paymentFromBankClientController,
			ICounterpartyContractRepository counterpartyContractRepository,
			ICounterpartyContractFactory counterpartyContractFactory,
			IFastPaymentSender fastPaymentSender,
			ICallTaskWorker callTaskWorker,
			INomenclatureRepository nomenclatureRepository,
			IGenericRepository<DiscountReason> discountReasonRepository,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IOrderDiscountsController orderDiscountsController)
		{
			if(nomenclatureSettings is null)
			{
				throw new ArgumentNullException(nameof(nomenclatureSettings));
			}

			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_orderDailyNumberController = orderDailyNumberController ?? throw new ArgumentNullException(nameof(orderDailyNumberController));
			_paymentFromBankClientController = paymentFromBankClientController ?? throw new ArgumentNullException(nameof(paymentFromBankClientController));
			_counterpartyContractRepository = counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
			_counterpartyContractFactory = counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
			_fastPaymentSender = fastPaymentSender ?? throw new ArgumentNullException(nameof(fastPaymentSender));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_orderDiscountsController = orderDiscountsController ?? throw new ArgumentNullException(nameof(orderDiscountsController));
			PaidDeliveryNomenclatureId = nomenclatureSettings.PaidDeliveryNomenclatureId;
		}

		public int PaidDeliveryNomenclatureId { get; }

		public void UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order)
		{
			OrderItem deliveryPriceItem = order.OrderItems
				.FirstOrDefault(x => x.Nomenclature.Id == PaidDeliveryNomenclatureId);

			#region перенести всё это в OrderStateKey

			bool IsDeliveryForFree = order.SelfDelivery
				|| order.OrderAddressType == OrderAddressType.Service
				|| order.DeliveryPoint.AlwaysFreeDelivery
				|| order.ObservableOrderItems
					.Any(n => n.Nomenclature.Category == NomenclatureCategory.spare_parts)
				|| !order.ObservableOrderItems
					.Any(n => n.Nomenclature.Id != PaidDeliveryNomenclatureId)
				&& (order.BottlesReturn > 0
					|| order.ObservableOrderEquipments.Any()
					|| order.ObservableOrderDepositItems.Any());

			if(IsDeliveryForFree)
			{
				if(deliveryPriceItem != null)
				{
					order.RemoveOrderItem(deliveryPriceItem);
				}
				return;
			}

			#endregion

			var district = order.DeliveryPoint != null
				? unitOfWork.GetById<District>(order.DeliveryPoint.District.Id)
				: null;

			var orderKey = new OrderStateKey(order);

			var price =
				district?.GetDeliveryPrice(orderKey, order.ObservableOrderItems
					.Sum(x => x.Nomenclature?.OnlineStoreExternalId != null ? x.ActualSum : 0m))
				?? 0m;

			if(price != 0)
			{
				order.AddOrUpdateDeliveryItem(unitOfWork.GetById<Nomenclature>(PaidDeliveryNomenclatureId), price);

				return;
			}

			if(deliveryPriceItem != null)
			{
				order.RemoveOrderItem(deliveryPriceItem);
			}
		}

		/// <summary>
		/// Рассчитывает и возвращает цену заказа по имеющимся данным о заказе
		/// </summary>
		public decimal GetOrderPrice(RoboatsOrderArgs roboatsOrderArgs)
		{
			if(roboatsOrderArgs is null)
			{
				throw new ArgumentNullException(nameof(roboatsOrderArgs));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);
				if(roboatsEmployee == null)
				{
					throw new InvalidOperationException("Специальный сотрудник для работы с Roboats должен быть создан и заполнен в параметрах");
				}

				var counterparty = unitOfWork.GetById<Counterparty>(roboatsOrderArgs.CounterpartyId);
				var deliveryPoint = unitOfWork.GetById<DeliveryPoint>(roboatsOrderArgs.DeliveryPointId);

				Order order = unitOfWork.Root;
				order.Author = roboatsEmployee;
				order.Client = counterparty;
				order.DeliveryPoint = deliveryPoint;
				order.PaymentType = PaymentType.Cash;
				foreach(var waterInfo in roboatsOrderArgs.WatersInfo)
				{
					var nomenclature = unitOfWork.GetById<Nomenclature>(waterInfo.NomenclatureId);
					order.AddWaterForSale(nomenclature, waterInfo.BottlesCount);
				}
				order.RecalculateItemsPrice();
				UpdateDeliveryCost(unitOfWork, order);
				return order.OrderSum;
			}
		}

		/// <summary>
		/// Создает и подтверждает заказ
		/// Возвращает номер сохраненного заказа
		/// </summary>
		/// <param name="roboatsOrderArgs"></param>
		public int CreateAndAcceptOrder(RoboatsOrderArgs roboatsOrderArgs)
		{
			if(roboatsOrderArgs is null)
			{
				throw new ArgumentNullException(nameof(roboatsOrderArgs));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);
				if(roboatsEmployee == null)
				{
					throw new InvalidOperationException("Специальный сотрудник для работы с Roboats должен быть создан и заполнен в параметрах");
				}

				var order = CreateOrder(unitOfWork, roboatsEmployee, roboatsOrderArgs);
				order.AcceptOrder(roboatsEmployee, _callTaskWorker);
				order.SaveEntity(unitOfWork, roboatsEmployee, _orderDailyNumberController, _paymentFromBankClientController);
				return order.Id;
			}
		}

		/// <summary>
		/// Создает заказ с имеющимися данными в статусе Новый, для обработки его оператором вручную.
		/// Возвращает номер сохраненного заказа
		/// </summary>
		public int CreateIncompleteOrder(RoboatsOrderArgs roboatsOrderArgs)
		{
			if(roboatsOrderArgs is null)
			{
				throw new ArgumentNullException(nameof(roboatsOrderArgs));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				var order = CreateIncompleteOrder(unitOfWork, roboatsOrderArgs);
				return order.Id;
			}
		}

		private Order CreateIncompleteOrder(IUnitOfWorkGeneric<Order> unitOfWork, RoboatsOrderArgs roboatsOrderArgs)
		{
			var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);
			if(roboatsEmployee == null)
			{
				throw new InvalidOperationException("Специальный сотрудник для работы с Roboats должен быть создан и заполнен в параметрах");
			}

			var order = CreateOrder(unitOfWork, roboatsEmployee, roboatsOrderArgs);
			order.SaveEntity(unitOfWork, roboatsEmployee, _orderDailyNumberController, _paymentFromBankClientController);
			return order;
		}

		/// <summary>
		/// Создает заказ с имеющимися данными в статусе Новый.
		/// Запускает процесс формирования оплаты и отправки QR кода по смс.
		/// Если после 3-х попыток не получилось сформировать оплату, то заказ остается в статусе новый.
		/// Если оплата сформирована то заказ переходит в статус Принят
		/// </summary>
		public async Task<Order> CreateOrderWithPaymentByQrCode(string phone, RoboatsOrderArgs roboatsOrderArgs, bool needAcceptOrder)
		{
			if(roboatsOrderArgs is null)
			{
				throw new ArgumentNullException(nameof(roboatsOrderArgs));
			}

			using(var uowNewOrder = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(uowNewOrder);
				if(roboatsEmployee == null)
				{
					throw new InvalidOperationException("Специальный сотрудник для работы с Roboats должен быть создан и заполнен в параметрах");
				}

				var order = CreateIncompleteOrder(uowNewOrder, roboatsOrderArgs);

				if(needAcceptOrder)
				{
					var paymentSended = await TryingSendPayment(phone, order);
					if(paymentSended)
					{
						var acceptedOrder = AcceptOrder(order.Id, roboatsEmployee.Id);
						return acceptedOrder;
					}
				}

				return order;
			}
		}

		private Order AcceptOrder(int orderId, int roboatsEmployee)
		{
			using(var unitOfWork = _unitOfWorkFactory.CreateForRoot<Order>(orderId))
			{
				var order = unitOfWork.Root;
				var employee = unitOfWork.GetById<Employee>(roboatsEmployee);
				order.AcceptOrder(employee, _callTaskWorker);
				order.SaveEntity(unitOfWork, employee, _orderDailyNumberController, _paymentFromBankClientController);
				return order;
			}
		}

		private async Task<bool> TryingSendPayment(string phone, Order order)
		{
			FastPaymentResult result;
			int attemptsCount = 0;

			do
			{
				if(attemptsCount > 0)
				{
					await Task.Delay(60000);
				}
				result = await _fastPaymentSender.SendFastPaymentUrlAsync(order, phone, true);

				if(result.Status == ResultStatus.Error && result.OrderAlreadyPaied)
				{
					return true;
				}

				attemptsCount++;

			} while(result.Status == ResultStatus.Error && attemptsCount < 3);

			return result.Status == ResultStatus.Ok;
		}

		private Order CreateOrder(IUnitOfWorkGeneric<Order> unitOfWork, Employee author, RoboatsOrderArgs roboatsOrderArgs)
		{
			var counterparty = unitOfWork.GetById<Counterparty>(roboatsOrderArgs.CounterpartyId);
			var deliveryPoint = unitOfWork.GetById<DeliveryPoint>(roboatsOrderArgs.DeliveryPointId);
			var deliverySchedule = unitOfWork.GetById<DeliverySchedule>(roboatsOrderArgs.DeliveryScheduleId);
			Order order = unitOfWork.Root;
			order.Author = author;
			order.Client = counterparty;
			order.DeliveryPoint = deliveryPoint;
			switch(roboatsOrderArgs.PaymentType)
			{
				case RoboAtsOrderPayment.Cash:
					order.PaymentType = PaymentType.Cash;
					order.Trifle = roboatsOrderArgs.BanknoteForReturn;
					break;
				case RoboAtsOrderPayment.Terminal:
					order.PaymentType = PaymentType.Terminal;
					break;
				case RoboAtsOrderPayment.QrCode:
					order.PaymentType = PaymentType.DriverApplicationQR;
					order.Trifle = 0;
					break;
				default:
					throw new NotSupportedException("Неподдерживаемый тип оплаты через Roboats");
			}
			order.DeliverySchedule = deliverySchedule;
			order.DeliveryDate = roboatsOrderArgs.Date;

			order.UpdateOrCreateContract(unitOfWork, _counterpartyContractRepository, _counterpartyContractFactory);

			foreach(var waterInfo in roboatsOrderArgs.WatersInfo)
			{
				var nomenclature = unitOfWork.GetById<Nomenclature>(waterInfo.NomenclatureId);
				order.AddWaterForSale(nomenclature, waterInfo.BottlesCount);
			}
			order.BottlesReturn = roboatsOrderArgs.BottlesReturn;
			order.RecalculateItemsPrice();
			UpdateDeliveryCost(unitOfWork, order);
			order.AddDeliveryPointCommentToOrder();

			if(!order.SelfDelivery)
			{
				order.CallBeforeArrivalMinutes = 15;
				order.IsDoNotMakeCallBeforeArrival = false;
			}

			return order;
		}

		public void CheckAndAddBottlesToReferrerByReferFriendPromo(
			IUnitOfWork uow,
			Order order,
			bool canChangeDiscountValue)
		{
			if(order.OrderItems.Any(o => o.DiscountReason?.Id == _orderSettings.ReferFriendDiscountReasonId))
			{
				return;
			}

			var referredCounterparties = _orderRepository.GetReferredCounterpartiesCountByReferPromotion(uow, order.Client.Id);
			var alreadyReceived = _orderRepository.GetAlreadyReceivedBottlesCountByReferPromotion(uow, order.Client.Id, _orderSettings.ReferFriendDiscountReasonId);

			var bottlesToAdd = referredCounterparties - alreadyReceived;

			if(bottlesToAdd < 1)
			{
				return;
			}

			var nomenclature = _nomenclatureRepository.GetWaterSemiozerie(uow);

			var referFriendDiscountReason = _discountReasonRepository.Get(uow, x => x.Id == _orderSettings.ReferFriendDiscountReasonId).First();

			var beforeAddItemsCount = order.OrderItems.Count();
			order.AddWaterForSale(nomenclature, bottlesToAdd);		
			var afterAddItemsCount = order.OrderItems.Count();

			if(afterAddItemsCount == beforeAddItemsCount)
			{
				return;
			}

			var orderItem = order.OrderItems.Last();

			_orderDiscountsController.SetDiscountFromDiscountReasonForOrderItem(referFriendDiscountReason, orderItem, canChangeDiscountValue, out string message);
		}
	}
}
