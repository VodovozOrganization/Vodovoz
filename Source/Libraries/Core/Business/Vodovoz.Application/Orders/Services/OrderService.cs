using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Errors;
using Vodovoz.Services.Orders;
using Vodovoz.Settings.Employee;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Application.Orders.Services
{
	internal sealed class OrderService : IOrderService
	{
		private const string _employeeRequiredForServiceError =
			"Требуется сотрудник. " +
			"Если сообщение получено в сервисе - убедитесь, что настроили сервис корректно и в ДВ есть соответствующий сотрудник";

		private readonly ILogger<OrderService> _logger;

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IOrderDailyNumberController _orderDailyNumberController;
		private readonly IPaymentFromBankClientController _paymentFromBankClientController;
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
		private readonly IOrderDeliveryPriceGetter _orderDeliveryPriceGetter;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IOrderContractUpdater _orderContractUpdater;
		private readonly IOrderOrganizationManager _orderOrganizationManager;

		public OrderService(
			ILogger<OrderService> logger,
			INomenclatureSettings nomenclatureSettings,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeRepository employeeRepository,
			IOrderDailyNumberController orderDailyNumberController,
			IPaymentFromBankClientController paymentFromBankClientController,
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
			IOrderDeliveryPriceGetter orderDeliveryPriceGetter,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			ISubdivisionRepository subdivisionRepository,
			IOrderContractUpdater orderContractUpdater,
			IOrderOrganizationManager orderOrganizationManager)
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
			_orderDeliveryPriceGetter = orderDeliveryPriceGetter ?? throw new ArgumentNullException(nameof(orderDeliveryPriceGetter));
			_undeliveredOrdersRepository = undeliveredOrdersRepository;
			_subdivisionRepository = subdivisionRepository;
			_orderContractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));
			_orderOrganizationManager = orderOrganizationManager ?? throw new ArgumentNullException(nameof(orderOrganizationManager));
			PaidDeliveryNomenclatureId = nomenclatureSettings.PaidDeliveryNomenclatureId;
		}

		public int PaidDeliveryNomenclatureId { get; }

		public void UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order)
		{
			var deliveryPrice = _orderDeliveryPriceGetter.GetDeliveryPrice(unitOfWork, order);
			order.UpdateDeliveryItem(
				unitOfWork, _orderContractUpdater, unitOfWork.GetById<Nomenclature>(PaidDeliveryNomenclatureId), deliveryPrice);
		}

		/// <summary>
		/// Рассчитывает и возвращает цену заказа по имеющимся данным о заказе
		/// </summary>
		public decimal GetOrderPrice(CreateOrderRequest createOrderRequest)
		{
			if(createOrderRequest is null)
			{
				throw new ArgumentNullException(nameof(createOrderRequest));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>("Сервис заказов: подсчет стоимости заказа"))
			{
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork)
					?? throw new InvalidOperationException(_employeeRequiredForServiceError);

				var counterparty = unitOfWork.GetById<Counterparty>(createOrderRequest.CounterpartyId);
				var deliveryPoint = unitOfWork.GetById<DeliveryPoint>(createOrderRequest.DeliveryPointId);

				Order order = unitOfWork.Root;
				order.Author = roboatsEmployee;
				order.UpdateClient(counterparty, _orderContractUpdater, out var updateClientMessage);
				order.UpdateDeliveryPoint(deliveryPoint, _orderContractUpdater);
				order.UpdatePaymentType(PaymentType.Cash, _orderContractUpdater);

				foreach(var waterInfo in createOrderRequest.SaleItems)
				{
					var nomenclature = unitOfWork.GetById<Nomenclature>(waterInfo.NomenclatureId);
					order.AddWaterForSale(unitOfWork, _orderContractUpdater, nomenclature, waterInfo.BottlesCount);
				}

				order.RecalculateItemsPrice();
				UpdateDeliveryCost(unitOfWork, order);
				return order.OrderSum;
			}
		}

		/// <summary>
		/// Рассчитывает и возвращает цену заказа и цену доставки по имеющимся данным о заказе
		/// </summary>
		public (decimal OrderPrice, decimal DeliveryPrice) GetOrderAndDeliveryPrices(CreateOrderRequest createOrderRequest)
		{
			if(createOrderRequest is null)
			{
				throw new ArgumentNullException(nameof(createOrderRequest));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>("Сервис заказов: подсчет стоимости заказа"))
			{
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork)
					?? throw new InvalidOperationException(_employeeRequiredForServiceError);

				var counterparty = unitOfWork.GetById<Counterparty>(createOrderRequest.CounterpartyId);
				var deliveryPoint = unitOfWork.GetById<DeliveryPoint>(createOrderRequest.DeliveryPointId);

				Order order = unitOfWork.Root;
				order.Author = roboatsEmployee;
				order.UpdateClient(counterparty, _orderContractUpdater, out var updateClientMessage);
				order.UpdateDeliveryPoint(deliveryPoint, _orderContractUpdater);
				order.UpdatePaymentType(PaymentType.Cash, _orderContractUpdater);

				foreach(var waterInfo in createOrderRequest.SaleItems)
				{
					var nomenclature = unitOfWork.GetById<Nomenclature>(waterInfo.NomenclatureId);
					order.AddWaterForSale(unitOfWork, _orderContractUpdater, nomenclature, waterInfo.BottlesCount);
				}

				order.RecalculateItemsPrice();
				UpdateDeliveryCost(unitOfWork, order);
				return
				(
					order.OrderSum,
					order.OrderItems.Where(oi => oi.Nomenclature.Id == PaidDeliveryNomenclatureId).FirstOrDefault()?.ActualSum ?? 0m
				);
			}
		}

		/// <summary>
		/// Создает и подтверждает заказ
		/// Возвращает номер сохраненного заказа
		/// </summary>
		/// <param name="createOrderRequest"></param>
		public int CreateAndAcceptOrder(CreateOrderRequest createOrderRequest)
		{
			if(createOrderRequest is null)
			{
				throw new ArgumentNullException(nameof(createOrderRequest));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);
				if(roboatsEmployee == null)
				{
					throw new InvalidOperationException(_employeeRequiredForServiceError);
				}

				var order = CreateOrder(unitOfWork, roboatsEmployee, createOrderRequest);
				order.AcceptOrder(roboatsEmployee, _callTaskWorker);
				order.SaveEntity(
					unitOfWork, _orderContractUpdater, roboatsEmployee, _orderDailyNumberController, _paymentFromBankClientController);
				return order.Id;
			}
		}

		/// <summary>
		/// Создает заказ с имеющимися данными в статусе Новый, для обработки его оператором вручную.
		/// Возвращает данные по заказу
		/// </summary>
		public (int OrderId, int AuthorId, OrderStatus OrderStatus) CreateIncompleteOrder(CreateOrderRequest createOrderRequest)
		{
			if(createOrderRequest is null)
			{
				throw new ArgumentNullException(nameof(createOrderRequest));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				return CreateIncompleteOrder(unitOfWork, createOrderRequest);
			}
		}

		private (int OrderId, int AuthorId, OrderStatus OrderStatus) CreateIncompleteOrder(
			IUnitOfWorkGeneric<Order> unitOfWork, CreateOrderRequest createOrderRequest)
		{
			var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);
			if(roboatsEmployee == null)
			{
				throw new InvalidOperationException(_employeeRequiredForServiceError);
			}

			var order = CreateOrder(unitOfWork, roboatsEmployee, createOrderRequest);
			order.SaveEntity(
				unitOfWork, _orderContractUpdater, roboatsEmployee, _orderDailyNumberController, _paymentFromBankClientController);
			return (order.Id, order.Author.Id, order.OrderStatus);
		}

		/// <summary>
		/// Подтверждение заказа
		/// </summary>
		/// <param name="orderId">номер заказа</param>
		/// <param name="roboatsEmployeeId">Id сотрудника</param>
		/// <returns>Данные по заказу(номер заказа, номер автора, статус заказа)</returns>
		public (int OrderId, int AuthorId, OrderStatus OrderStatus) AcceptOrder(int orderId, int roboatsEmployeeId)
		{
			using(var unitOfWork = _unitOfWorkFactory.CreateForRoot<Order>(orderId))
			{
				var order = unitOfWork.Root;
				var employee = unitOfWork.GetById<Employee>(roboatsEmployeeId);
				order.AcceptOrder(employee, _callTaskWorker);
				order.SaveEntity(
					unitOfWork, _orderContractUpdater, employee, _orderDailyNumberController, _paymentFromBankClientController);
				return (order.Id, order.Author.Id, order.OrderStatus);
			}
		}

		private Order CreateOrder(IUnitOfWorkGeneric<Order> unitOfWork, Employee author, CreateOrderRequest createOrderRequest)
		{
			var counterparty = unitOfWork.GetById<Counterparty>(createOrderRequest.CounterpartyId);
			var deliveryPoint = unitOfWork.GetById<DeliveryPoint>(createOrderRequest.DeliveryPointId);
			var deliverySchedule = unitOfWork.GetById<DeliverySchedule>(createOrderRequest.DeliveryScheduleId);
			Order order = unitOfWork.Root;
			order.Author = author;
			order.UpdateClient(counterparty, _orderContractUpdater, out var updateClientMessage);
			order.UpdateDeliveryPoint(deliveryPoint, _orderContractUpdater);

			order.UpdatePaymentType(createOrderRequest.PaymentType, _orderContractUpdater);

			switch(createOrderRequest.PaymentType)
			{
				case PaymentType.Cash:
					order.Trifle = createOrderRequest.BanknoteForReturn;
					break;
				case PaymentType.DriverApplicationQR:
					order.Trifle = 0;
					break;
			}

			order.DeliverySchedule = deliverySchedule;
			order.UpdateDeliveryDate(createOrderRequest.Date, _orderContractUpdater, out var updateDeliveryDateMessage);

			_orderContractUpdater.UpdateOrCreateContract(unitOfWork, order);

			foreach(var waterInfo in createOrderRequest.SaleItems)
			{
				var nomenclature = unitOfWork.GetById<Nomenclature>(waterInfo.NomenclatureId);
				order.AddWaterForSale(unitOfWork, _orderContractUpdater, nomenclature, waterInfo.BottlesCount);
			}
			order.BottlesReturn = createOrderRequest.BottlesReturn;
			order.RecalculateItemsPrice();
			UpdateDeliveryCost(unitOfWork, order);
			AddLogisticsRequirements(order);
			order.AddDeliveryPointCommentToOrder();

			if(!order.SelfDelivery)
			{
				order.CallBeforeArrivalMinutes = 15;
				order.IsDoNotMakeCallBeforeArrival = false;
			}

			return order;
		}
		
		private void AddLogisticsRequirements(Order order)
		{
			order.LogisticsRequirements = GetLogisticsRequirements(order);
		}

		public int TryCreateOrderFromOnlineOrderAndAccept(IUnitOfWork uow, OnlineOrder onlineOrder)
		{
			if(onlineOrder.IsNeedConfirmationByCall)
			{
				return 0;
			}

			var validationResult = _onlineOrderValidator.ValidateOnlineOrder(uow, onlineOrder);

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

			if(_orderOrganizationManager.GetOrganizationsWithOrderItems(
				uow, DateTime.Now.TimeOfDay, OrderOrganizationChoice.Create(order)).Count() > 1)
			{
				return 0;
			}

			UpdateDeliveryCost(uow, order);
			AddLogisticsRequirements(order);
			order.AddDeliveryPointCommentToOrder();
			order.AddFastDeliveryNomenclatureIfNeeded(uow, _orderContractUpdater);

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

			return order.Id;
		}

		private Result TryAcceptOrderCreatedByOnlineOrder(IUnitOfWork uow, Employee employee, Order order)
		{
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
			order.SaveEntity(uow, _orderContractUpdater, employee, _orderDailyNumberController, _paymentFromBankClientController);

			return Result.Success();
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
			var alreadyReceived = _orderRepository.GetAlreadyReceivedBottlesCountByReferPromotion(uow, order, _orderSettings.ReferFriendDiscountReasonId);

			var bottlesToAdd = referredCounterparties - alreadyReceived;

			if(bottlesToAdd < 1)
			{
				return;
			}

			var nomenclature = _nomenclatureRepository.GetWaterSemiozerie(uow);

			var referFriendDiscountReason = _discountReasonRepository.Get(uow, x => x.Id == _orderSettings.ReferFriendDiscountReasonId).First();

			var beforeAddItemsCount = order.OrderItems.Count();
			order.AddNomenclature(uow, _orderContractUpdater, nomenclature, bottlesToAdd);
			var afterAddItemsCount = order.OrderItems.Count();

			if(afterAddItemsCount == beforeAddItemsCount)
			{
				return;
			}

			var orderItem = order.OrderItems.Last();

			_orderDiscountsController.SetDiscountFromDiscountReasonForOrderItem(referFriendDiscountReason, orderItem, canChangeDiscountValue, out string message);
		}

		public bool NeedResendByEdo(IUnitOfWork unitOfWork, Order order)
		{
			return _orderRepository
				.GetEdoContainersByOrderId(unitOfWork, order.Id)
				.Count(x => x.Type == Core.Domain.Documents.Type.Bill) > 0;
		}


		/// <summary>
		/// Автоотмена автопереноса - недовоз, созданный из возвращенного в путь заказа , получает комментарий, а также ответственного "Нет (не недовоз)"
		/// Заказ, созданный из недовоза переходит в статус "Отменен".
		/// Недовоз, созданный из автопереноса получает ответственного "Автоотмена автопереноса", а также аналогичный комментарий.
		/// </summary>
		public void AutoCancelAutoTransfer(IUnitOfWork uow, Order order)
		{
			var oldUndeliveries = _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(uow, order);

			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(uow);

			var oldUndeliveryCommentText = "Доставлен в тот же день";

			foreach(var oldUndelivery in oldUndeliveries)
			{
				oldUndelivery.NewOrder?.ChangeStatus(OrderStatus.Canceled);

				var oldUndeliveredOrderResultComment = new UndeliveredOrderResultComment
				{
					Author = currentEmployee,
					Comment = oldUndeliveryCommentText,
					CreationTime = DateTime.Now,
					UndeliveredOrder = oldUndelivery
				};

				oldUndelivery.ResultComments.Add(oldUndeliveredOrderResultComment);

				var oldOrderGuiltyInUndelivery = new GuiltyInUndelivery
				{
					GuiltySide = GuiltyTypes.None,
					UndeliveredOrder = oldUndelivery
				};

				oldUndelivery.GuiltyInUndelivery.Clear();
				oldUndelivery.GuiltyInUndelivery.Add(oldOrderGuiltyInUndelivery);

				oldUndelivery.AddAutoCommentToOkkDiscussion(uow, oldUndeliveryCommentText);

				uow.Save(oldUndelivery);

				if(oldUndelivery.NewOrder == null)
				{
					continue;
				}

				var newUndeliveries = _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(uow, oldUndelivery.NewOrder);

				if(newUndeliveries.Any())
				{
					return;
				}

				var newUndeliveredOrder = new UndeliveredOrder
				{
					Author = currentEmployee,
					OldOrder = oldUndelivery.NewOrder,
					EmployeeRegistrator = currentEmployee,
					TimeOfCreation = DateTime.Now,
					InProcessAtDepartment = _subdivisionRepository.GetQCDepartment(uow)
				};

				var undeliveredOrderResultComment = new UndeliveredOrderResultComment
				{
					Author = currentEmployee,
					Comment = GuiltyTypes.AutoСancelAutoTransfer.GetEnumTitle(),
					CreationTime = DateTime.Now,
					UndeliveredOrder = newUndeliveredOrder
				};

				newUndeliveredOrder.ResultComments.Add(undeliveredOrderResultComment);

				var newOrderGuiltyInUndelivery = new GuiltyInUndelivery
				{
					GuiltySide = GuiltyTypes.AutoСancelAutoTransfer,
					UndeliveredOrder = newUndeliveredOrder
				};

				newUndeliveredOrder.GuiltyInUndelivery = new List<GuiltyInUndelivery> { newOrderGuiltyInUndelivery };

				uow.Save(newUndeliveredOrder);

				newUndeliveredOrder.AddAutoCommentToOkkDiscussion(uow, GuiltyTypes.AutoСancelAutoTransfer.GetEnumTitle());
			}
		}

		public LogisticsRequirements GetLogisticsRequirements(Order order)
		{
			if(order.LogisticsRequirements != null && order.IsCopiedFromUndelivery)
			{
				return order.LogisticsRequirements;
			}

			if(order.Client == null || (!order.SelfDelivery && order.DeliveryPoint == null))
			{
				return new LogisticsRequirements();
			}

			var counterpartyLogisticsRequirements = new LogisticsRequirements();
			var deliveryPointLogisticsRequirements = new LogisticsRequirements();

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot())
			{
				if(order.Client?.LogisticsRequirements?.Id > 0)
				{
					counterpartyLogisticsRequirements = unitOfWork.GetById<LogisticsRequirements>(order.Client.LogisticsRequirements.Id) ?? new LogisticsRequirements();
				}

				if(order.DeliveryPoint?.LogisticsRequirements?.Id > 0)
				{
					deliveryPointLogisticsRequirements = unitOfWork.GetById<LogisticsRequirements>(order.DeliveryPoint.LogisticsRequirements.Id) ?? new LogisticsRequirements();
				}
			}

			var logisticsRequirementsFromCounterpartyAndDeliveryPoint = new LogisticsRequirements
			{
				ForwarderRequired = counterpartyLogisticsRequirements.ForwarderRequired || deliveryPointLogisticsRequirements.ForwarderRequired,
				DocumentsRequired = counterpartyLogisticsRequirements.DocumentsRequired || deliveryPointLogisticsRequirements.DocumentsRequired,
				RussianDriverRequired = counterpartyLogisticsRequirements.RussianDriverRequired || deliveryPointLogisticsRequirements.RussianDriverRequired,
				PassRequired = counterpartyLogisticsRequirements.PassRequired || deliveryPointLogisticsRequirements.PassRequired,
				LargusRequired = counterpartyLogisticsRequirements.LargusRequired || deliveryPointLogisticsRequirements.LargusRequired
			};

			if(order.LogisticsRequirements != null)
			{
				return new LogisticsRequirements
				{
					ForwarderRequired = logisticsRequirementsFromCounterpartyAndDeliveryPoint.ForwarderRequired || order.LogisticsRequirements.ForwarderRequired,
					DocumentsRequired = logisticsRequirementsFromCounterpartyAndDeliveryPoint.DocumentsRequired || order.LogisticsRequirements.DocumentsRequired,
					RussianDriverRequired = logisticsRequirementsFromCounterpartyAndDeliveryPoint.RussianDriverRequired || order.LogisticsRequirements.RussianDriverRequired,
					PassRequired = logisticsRequirementsFromCounterpartyAndDeliveryPoint.PassRequired || order.LogisticsRequirements.PassRequired,
					LargusRequired = logisticsRequirementsFromCounterpartyAndDeliveryPoint.LargusRequired || order.LogisticsRequirements.LargusRequired
				};
			}

			return logisticsRequirementsFromCounterpartyAndDeliveryPoint;
		}
	}
}
