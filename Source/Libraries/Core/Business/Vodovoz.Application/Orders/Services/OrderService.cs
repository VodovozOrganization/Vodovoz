using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Data;
using System.Linq;
using QS.DomainModel.Tracking;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Factories;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Models.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Errors;
using Vodovoz.Services.Orders;
using Vodovoz.Settings.Employee;
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
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IOrderFromOnlineOrderCreator _orderFromOnlineOrderCreator;
		private readonly IOrderFromOnlineOrderValidator _onlineOrderValidator;
		private readonly FastDeliveryHandler _fastDeliveryHandler;
		private readonly IPromotionalSetRepository _promotionalSetRepository;
		private readonly IEmployeeSettings _employeeSettings;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IGenericRepository<DiscountReason> _discountReasonRepository;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrderDiscountsController _orderDiscountsController;
		private readonly IDeliveryPriceCalculator _deliveryPriceCalculator;

		public OrderService(
			ILogger<OrderService> logger,
			INomenclatureSettings nomenclatureSettings,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeRepository employeeRepository,
			IOrderDailyNumberController orderDailyNumberController,
			IPaymentFromBankClientController paymentFromBankClientController,
			ICounterpartyContractRepository counterpartyContractRepository,
			ICounterpartyContractFactory counterpartyContractFactory,
			ICallTaskWorker callTaskWorker,
			IOrderFromOnlineOrderCreator orderFromOnlineOrderCreator,
			IOrderFromOnlineOrderValidator onlineOrderValidator,
			FastDeliveryHandler fastDeliveryHandler,
			IPromotionalSetRepository promotionalSetRepository,
			IEmployeeSettings employeeSettings,
			INomenclatureRepository nomenclatureRepository,
			IGenericRepository<DiscountReason> discountReasonRepository,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IOrderDiscountsController orderDiscountsController,
			IDeliveryPriceCalculator deliveryPriceCalculator)
		{
			if(nomenclatureSettings is null)
			{
				throw new ArgumentNullException(nameof(nomenclatureSettings));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_orderDailyNumberController = orderDailyNumberController ?? throw new ArgumentNullException(nameof(orderDailyNumberController));
			_paymentFromBankClientController = paymentFromBankClientController ?? throw new ArgumentNullException(nameof(paymentFromBankClientController));
			_counterpartyContractRepository = counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
			_counterpartyContractFactory = counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_orderFromOnlineOrderCreator = orderFromOnlineOrderCreator ?? throw new ArgumentNullException(nameof(orderFromOnlineOrderCreator));
			_onlineOrderValidator = onlineOrderValidator ?? throw new ArgumentNullException(nameof(onlineOrderValidator));
			_fastDeliveryHandler = fastDeliveryHandler ?? throw new ArgumentNullException(nameof(fastDeliveryHandler));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_orderDiscountsController = orderDiscountsController ?? throw new ArgumentNullException(nameof(orderDiscountsController));
			_deliveryPriceCalculator = deliveryPriceCalculator ?? throw new ArgumentNullException(nameof(deliveryPriceCalculator));

			PaidDeliveryNomenclatureId = nomenclatureSettings.PaidDeliveryNomenclatureId;
		}

		public int PaidDeliveryNomenclatureId { get; }

		public void UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order)
		{
			var deliveryPrice = _deliveryPriceCalculator.CalculateDeliveryPrice(unitOfWork, order);
			order.UpdateDeliveryItem(unitOfWork.GetById<Nomenclature>(PaidDeliveryNomenclatureId), deliveryPrice);
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
		/// Возвращает сохраненный заказ
		/// </summary>
		public Order CreateIncompleteOrder(RoboatsOrderArgs roboatsOrderArgs)
		{
			if(roboatsOrderArgs is null)
			{
				throw new ArgumentNullException(nameof(roboatsOrderArgs));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				return CreateIncompleteOrder(unitOfWork, roboatsOrderArgs);
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
		/// Подтверждение заказа
		/// </summary>
		/// <param name="orderId">номер заказа</param>
		/// <param name="roboatsEmployee">Id сотрудника</param>
		/// <returns>подтвержденный заказ</returns>
		public Order AcceptOrder(int orderId, int roboatsEmployee)
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

		public int TryCreateOrderFromOnlineOrderAndAccept(IUnitOfWork uow, OnlineOrder onlineOrder)
		{
			if(onlineOrder.IsNeedConfirmationByCall)
			{
				return 0;
			}
			
			var validationResult = _onlineOrderValidator.ValidateOnlineOrder(onlineOrder);

			if(validationResult.IsFailure)
			{
				return 0;
			}
			
			Employee employee = null;
			switch(onlineOrder.Source)
			{
				case Source.MobileApp:
					employee = uow.GetById<Employee>(_employeeSettings.MobileAppEmployee);
					break;
				case Source.VodovozWebSite:
					employee = uow.GetById<Employee>(_employeeSettings.VodovozWebSiteEmployee);
					break;
				case Source.KulerSaleWebSite:
					employee = uow.GetById<Employee>(_employeeSettings.KulerSaleWebSiteEmployee);
					break;
			}
			
			var order = _orderFromOnlineOrderCreator.CreateOrderFromOnlineOrder(uow, employee, onlineOrder);
			
			UpdateDeliveryCost(uow, order);
			order.AddDeliveryPointCommentToOrder();
			order.AddFastDeliveryNomenclatureIfNeeded();
			
			//TODO проверка возможности добавления промонаборов
			
			//TODO проверить работу сохранения заказов
			
			//для открытия внутренней транзакции
			uow.Save(onlineOrder);
			var acceptResult = TryAcceptOrderCreatedByOnlineOrder(uow, employee, order);

			if(acceptResult.IsFailure)
			{
				return 0;
			}

			onlineOrder.SetOrderPerformed(order, employee);
			var notification = OnlineOrderStatusUpdatedNotification.CreateOnlineOrderStatusUpdatedNotification(onlineOrder);
			uow.Save(notification);
			uow.Commit();
			
			/*using(var transaction = uow.Session.BeginTransaction())
			{
				try
				{
					var acceptResult = TryAcceptOrderCreatedByOnlineOrder(uow, employee, order);
					
					if(acceptResult.IsFailure)
					{
						return 0;
					}

					onlineOrder.SetOrderPerformed(order, employee);
					uow.Save(onlineOrder);
					var notification = OnlineOrderStatusUpdatedNotification.CreateOnlineOrderStatusUpdatedNotification(onlineOrder);
					uow.Save(notification);
					transaction.Commit();
					GlobalUowEventsTracker.OnPostCommit((IUnitOfWorkTracked)uow);
				}
				catch(Exception e)
				{
					if(!transaction.WasCommitted
						&& !transaction.WasRolledBack
						&& transaction.IsActive
						&& uow.Session.Connection.State == ConnectionState.Open)
					{
						try
						{
							transaction.Rollback();
						}
						catch { }
					}

					return 0;
				}
			}*/
			
			return order.Id;
		}
		
		private Result TryAcceptOrderCreatedByOnlineOrder(IUnitOfWork uow, Employee employee, Order order)
		{
			var hasPromoSetForNewClients = order.PromotionalSets.Any(x => x.PromotionalSetForNewClients);
			
			if(hasPromoSetForNewClients && order.HasUsedPromoForNewClients(_promotionalSetRepository))
			{
				return Result.Failure(Errors.Orders.Order.UnableToShipPromoSet);
			}

			if(!order.SelfDelivery)
			{
				var fastDeliveryResult = _fastDeliveryHandler.CheckFastDelivery(uow, order);
			
				if(fastDeliveryResult.IsFailure)
				{
					return fastDeliveryResult;
				}
				
				_fastDeliveryHandler.TryAddOrderToRouteListAndNotifyDriver(uow, order, _callTaskWorker);
			}
			
			order.AcceptOrder(order.Author, _callTaskWorker);
			order.SaveEntity(uow, employee, _orderDailyNumberController, _paymentFromBankClientController);
			
			return Result.Success();
		}
	}
}
