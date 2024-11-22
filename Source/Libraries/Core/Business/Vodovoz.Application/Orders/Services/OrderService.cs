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
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Errors;
using Vodovoz.Factories;
using Vodovoz.Models.Orders;
using Vodovoz.Services.Orders;
using Vodovoz.Settings.Employee;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Services.Orders;
using Order = Vodovoz.Domain.Orders.Order;

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
		private readonly IOrderDeliveryPriceGetter _orderDeliveryPriceGetter;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;

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
			IOrderDeliveryPriceGetter orderDeliveryPriceGetter,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			ISubdivisionRepository subdivisionRepository)
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
			_orderDeliveryPriceGetter = orderDeliveryPriceGetter ?? throw new ArgumentNullException(nameof(orderDeliveryPriceGetter));
			_undeliveredOrdersRepository = undeliveredOrdersRepository;
			_subdivisionRepository = subdivisionRepository;
			PaidDeliveryNomenclatureId = nomenclatureSettings.PaidDeliveryNomenclatureId;
		}

		public int PaidDeliveryNomenclatureId { get; }

		public void UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order)
		{
			var deliveryPrice = _orderDeliveryPriceGetter.GetDeliveryPrice(unitOfWork, order);
			order.UpdateDeliveryItem(unitOfWork.GetById<Nomenclature>(PaidDeliveryNomenclatureId), deliveryPrice);
		}

		/// <summary>
		/// Рассчитывает и возвращает цену заказа по имеющимся данным о заказе
		/// </summary>
		public decimal GetOrderPrice(CreateOrderRequest roboatsOrderArgs)
		{
			if(roboatsOrderArgs is null)
			{
				throw new ArgumentNullException(nameof(roboatsOrderArgs));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Сервис заказов: подсчет стоимости заказа"))
			{
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork)
					?? throw new InvalidOperationException("Специальный сотрудник для работы с Roboats должен быть создан и заполнен в параметрах");

				var counterparty = unitOfWork.GetById<Counterparty>(roboatsOrderArgs.CounterpartyId);
				var deliveryPoint = unitOfWork.GetById<DeliveryPoint>(roboatsOrderArgs.DeliveryPointId);

				var order = new Order();
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
		public int CreateAndAcceptOrder(CreateOrderRequest roboatsOrderArgs)
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
		/// Возвращает данные по заказу
		/// </summary>
		public (int OrderId, int AuthorId, OrderStatus OrderStatus) CreateIncompleteOrder(CreateOrderRequest roboatsOrderArgs)
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

		private (int OrderId, int AuthorId, OrderStatus OrderStatus) CreateIncompleteOrder(
			IUnitOfWorkGeneric<Order> unitOfWork, CreateOrderRequest roboatsOrderArgs)
		{
			var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);
			if(roboatsEmployee == null)
			{
				throw new InvalidOperationException("Специальный сотрудник для работы с Roboats должен быть создан и заполнен в параметрах");
			}

			var order = CreateOrder(unitOfWork, roboatsEmployee, roboatsOrderArgs);
			order.SaveEntity(unitOfWork, roboatsEmployee, _orderDailyNumberController, _paymentFromBankClientController);
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
				order.SaveEntity(unitOfWork, employee, _orderDailyNumberController, _paymentFromBankClientController);
				return (order.Id, order.Author.Id, order.OrderStatus);
			}
		}

		private Order CreateOrder(IUnitOfWorkGeneric<Order> unitOfWork, Employee author, CreateOrderRequest roboatsOrderArgs)
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

			UpdateDeliveryCost(uow, order);
			order.AddDeliveryPointCommentToOrder();
			order.AddFastDeliveryNomenclatureIfNeeded();

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
			order.SaveEntity(uow, employee, _orderDailyNumberController, _paymentFromBankClientController);

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
			order.AddNomenclature(nomenclature, bottlesToAdd);
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
				.Count(x => x.Type == Domain.Orders.Documents.Type.Bill) > 0;
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
	}
}

