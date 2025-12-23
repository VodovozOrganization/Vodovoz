using DocumentFormat.OpenXml.Office2010.Excel;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Orders;
using Vodovoz.Settings.Employee;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Domain.Client.Specifications;
using VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters;
using VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters.Specifications;
using VodovozBusiness.Domain.Goods.Specifications;
using VodovozBusiness.Domain.Logistic.Specifications;
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
		private readonly IOrderFromOnlineOrderCreator _orderFromOnlineOrderCreator;
		private readonly IEmployeeSettings _employeeSettings;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IGenericRepository<DiscountReason> _discountReasonRepository;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrderDiscountsController _orderDiscountsController;
		private readonly IOrderDeliveryPriceGetter _orderDeliveryPriceGetter;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IGenericRepository<RobotMiaParameters> _robotMiaParametersRepository;
		private readonly IGenericRepository<DeliveryPoint> _deliveryPointRepository;
		private readonly IGenericRepository<Counterparty> _counterpartyRepository;
		private readonly IGenericRepository<DeliverySchedule> _deliveryScheduleRepository;
		private readonly IGenericRepository<Nomenclature> _nomenclatureGenericRepository;
		private readonly IOrderContractUpdater _orderContractUpdater;
		private readonly IOrderConfirmationService _orderConfirmationService;
		private readonly IPaymentItemsRepository _paymentItemsRepository;

		public OrderService(
			ILogger<OrderService> logger,
			INomenclatureSettings nomenclatureSettings,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeRepository employeeRepository,
			IOrderDailyNumberController orderDailyNumberController,
			IPaymentFromBankClientController paymentFromBankClientController,
			IOrderFromOnlineOrderCreator orderFromOnlineOrderCreator,
			IEmployeeSettings employeeSettings,
			INomenclatureRepository nomenclatureRepository,
			IGenericRepository<DiscountReason> discountReasonRepository,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IOrderDiscountsController orderDiscountsController,
			IOrderDeliveryPriceGetter orderDeliveryPriceGetter,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			ISubdivisionRepository subdivisionRepository,
			IGenericRepository<RobotMiaParameters> robotMiaParametersRepository,
			IGenericRepository<DeliveryPoint> deliveryPointRepository,
			IGenericRepository<Counterparty> counterpartyRepository,
			IGenericRepository<DeliverySchedule> deliveryScheduleRepository,
			IGenericRepository<Nomenclature> nomenclatureGenericRepository,
			IOrderContractUpdater orderContractUpdater,
			IOrderConfirmationService orderConfirmationService,
			IPaymentItemsRepository paymentItemsRepository
			)
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
			_orderFromOnlineOrderCreator = orderFromOnlineOrderCreator ?? throw new ArgumentNullException(nameof(orderFromOnlineOrderCreator));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_orderDiscountsController = orderDiscountsController ?? throw new ArgumentNullException(nameof(orderDiscountsController));
			_orderDeliveryPriceGetter = orderDeliveryPriceGetter ?? throw new ArgumentNullException(nameof(orderDeliveryPriceGetter));
			_undeliveredOrdersRepository = undeliveredOrdersRepository;
			_subdivisionRepository = subdivisionRepository;
			_robotMiaParametersRepository = robotMiaParametersRepository ?? throw new ArgumentNullException(nameof(robotMiaParametersRepository));
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_deliveryScheduleRepository = deliveryScheduleRepository ?? throw new ArgumentNullException(nameof(deliveryScheduleRepository));
			_nomenclatureGenericRepository = nomenclatureGenericRepository ?? throw new ArgumentNullException(nameof(nomenclatureGenericRepository));
			_orderContractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));
			_orderConfirmationService = orderConfirmationService ?? throw new ArgumentNullException(nameof(orderConfirmationService));
			_paymentItemsRepository = paymentItemsRepository ?? throw new ArgumentNullException(nameof(paymentItemsRepository));
			PaidDeliveryNomenclatureId = nomenclatureSettings.PaidDeliveryNomenclatureId;
			ForfeitNomenclatureId = nomenclatureSettings.ForfeitId;
		}

		public int PaidDeliveryNomenclatureId { get; }
		public int ForfeitNomenclatureId { get; }

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
		public (decimal OrderPrice, decimal DeliveryPrice, decimal ForfeitPrice) GetOrderAndDeliveryPrices(CreateOrderRequest createOrderRequest)
		{
			if(createOrderRequest is null)
			{
				throw new ArgumentNullException(nameof(createOrderRequest));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>("Сервис заказов: подсчет стоимости заказа"))
			{
				var robotMiaEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork)
					?? throw new InvalidOperationException(_employeeRequiredForServiceError);

				var counterparty = unitOfWork.GetById<Counterparty>(createOrderRequest.CounterpartyId)
					?? throw new InvalidOperationException($"Не найден контрагент #{createOrderRequest.CounterpartyId}");
				var deliveryPoint = unitOfWork.GetById<DeliveryPoint>(createOrderRequest.DeliveryPointId)
					?? throw new InvalidOperationException($"Не найдена точка доставки #{createOrderRequest.DeliveryPointId}");

				Order order = unitOfWork.Root;
				order.Author = robotMiaEmployee;
				order.UpdateClient(counterparty, _orderContractUpdater, out var updateClientMessage);
				order.UpdateDeliveryPoint(deliveryPoint, _orderContractUpdater);
				order.UpdatePaymentType(PaymentType.Cash, _orderContractUpdater);
				order.Author = robotMiaEmployee;

				foreach(var saleItem in createOrderRequest.SaleItems)
				{
					var nomenclature = unitOfWork.GetById<Nomenclature>(saleItem.NomenclatureId)
						?? throw new InvalidOperationException($"Не найдена номенклатура #{saleItem.NomenclatureId}");

					if(nomenclature.Id == ForfeitNomenclatureId)
					{
						order.AddNomenclature(unitOfWork, _orderContractUpdater, nomenclature, saleItem.BottlesCount);
						continue;
					}

					var nomenclatureParameters = _robotMiaParametersRepository
						.Get(unitOfWork, x => x.NomenclatureId == nomenclature.Id, 1)
						.FirstOrDefault();

					if(nomenclature.Category == NomenclatureCategory.water)
					{
						order.AddWaterForSale(unitOfWork, _orderContractUpdater, nomenclature, saleItem.BottlesCount);
					}
					else if(nomenclatureParameters is null
						|| nomenclatureParameters.GoodsOnlineAvailability != GoodsOnlineAvailability.ShowAndSale)
					{
						throw new InvalidOperationException(
							$"Номенклатура [{nomenclature.Id}] {nomenclature.Name} не может быть добавлена. В заказ может быть добавлена либо номенклатура, одобренная для продажи, либо неустойка");
					}
					else
					{
						order.AddNomenclature(unitOfWork, _orderContractUpdater, nomenclature, saleItem.BottlesCount);
					}
				}

				order.RecalculateItemsPrice();
				UpdateDeliveryCost(unitOfWork, order);

				return
				(
					order.OrderSum,
					order.OrderItems.Where(oi => oi.Nomenclature.Id == PaidDeliveryNomenclatureId).FirstOrDefault()?.ActualSum ?? 0m,
					order.OrderItems.Where(oi => oi.Nomenclature.Id == ForfeitNomenclatureId).Sum(x => x.ActualSum)
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
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork)
					?? throw new InvalidOperationException(_employeeRequiredForServiceError);

				var order = CreateOrder(unitOfWork, roboatsEmployee, createOrderRequest);
				_orderConfirmationService.AcceptOrder(unitOfWork, roboatsEmployee, order);
				return order.Id;
			}
		}

		/// <inheritdoc/>
		public async Task<Result<int>> CreateAndAcceptOrderAsync(CreateOrderRequest createOrderRequest)
		{
			if(createOrderRequest is null)
			{
				throw new ArgumentNullException(nameof(createOrderRequest));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork)
					?? throw new InvalidOperationException(_employeeRequiredForServiceError);

				var orderResult = await CreateOrderAsync(unitOfWork, roboatsEmployee, createOrderRequest);

				if(orderResult.IsFailure)
				{
					return Result.Failure<int>(orderResult.Errors);
				}

				var order = orderResult.Value;
				_orderConfirmationService.AcceptOrder(unitOfWork, roboatsEmployee, order);
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

		/// <inheritdoc/>
		public async Task<Result<(int OrderId, int AuthorId, OrderStatus OrderStatus)>> CreateIncompleteOrderAsync(CreateOrderRequest createOrderRequest)
		{
			if(createOrderRequest is null)
			{
				throw new ArgumentNullException(nameof(createOrderRequest));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				return await CreateIncompleteOrderAsync(unitOfWork, createOrderRequest);
			}
		}

		private async Task<Result<(int OrderId, int AuthorId, OrderStatus OrderStatus)>> CreateIncompleteOrderAsync(
			IUnitOfWorkGeneric<Order> unitOfWork, CreateOrderRequest createOrderRequest)
		{
			var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);

			if(roboatsEmployee is null)
			{
				return await Task.FromResult(Errors.ServiceEmployee.MissingServiceUser);
			}

			var order = CreateOrder(unitOfWork, roboatsEmployee, createOrderRequest);
			order.SaveEntity(
				unitOfWork,
				_orderContractUpdater,
				roboatsEmployee,
				_orderDailyNumberController,
				_paymentFromBankClientController);
			return await Task.FromResult((order.Id, order.Author.Id, order.OrderStatus));
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
				
				_orderConfirmationService.AcceptOrder(unitOfWork, employee, order);
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
				case PaymentType.Terminal:
					if(createOrderRequest.PaymentByTerminalSource == PaymentByTerminalSource.ByCard)
					{
						order.PaymentByTerminalSource = PaymentByTerminalSource.ByCard;
						break;
					}

					if(createOrderRequest.PaymentByTerminalSource == PaymentByTerminalSource.ByQR)
					{
						order.PaymentByTerminalSource = PaymentByTerminalSource.ByQR;
					}
					break;
			}

			order.DeliverySchedule = deliverySchedule;
			order.UpdateDeliveryDate(createOrderRequest.Date, _orderContractUpdater, out var updateDeliveryDateMessage);

			_orderContractUpdater.UpdateOrCreateContract(unitOfWork, order);

			foreach(var waterInfo in createOrderRequest.SaleItems)
			{
				var nomenclature = unitOfWork.GetById<Nomenclature>(waterInfo.NomenclatureId);

				if(nomenclature != null)
				{
					order.AddWaterForSale(unitOfWork, _orderContractUpdater, nomenclature, waterInfo.BottlesCount);
				}
				else
				{
					_logger.LogError("Попытка добавить отсутствующую номенклатуру {NomenclatureId}", waterInfo.NomenclatureId);
				}
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
			
			if(createOrderRequest.TareNonReturnReasonId != null)
			{
				var tareNonReturnReason = unitOfWork.GetById<NonReturnReason>(createOrderRequest.TareNonReturnReasonId.Value);
				order.TareNonReturnReason = tareNonReturnReason;
				order.OPComment = $"Робот Мия: {tareNonReturnReason.Name}.";
			}

			return order;
		}

		private async Task<Result<Order>> CreateOrderAsync(IUnitOfWorkGeneric<Order> unitOfWork, Employee author, CreateOrderRequest createOrderRequest)
		{
			var counterparty = _counterpartyRepository
				.Get(
					unitOfWork,
					CounterpartySpecifications.CreateForId(createOrderRequest.CounterpartyId),
					1)
				.FirstOrDefault();

			var deliveryPoint = _deliveryPointRepository
				.Get(
					unitOfWork,
					DeliveryPointSpecifications.CreateForAvailableToDelivery(createOrderRequest.CounterpartyId, createOrderRequest.DeliveryPointId),
					1)
				.FirstOrDefault();

			var deliverySchedule = _deliveryScheduleRepository
				.Get(
					unitOfWork,
					DeliveryScheduleSpecifications.CreateForId(createOrderRequest.DeliveryScheduleId),
					1)
				.FirstOrDefault();

			if(counterparty is null)
			{
				return Vodovoz.Errors.Clients.CounterpartyErrors.NotFound;
			}

			if(deliveryPoint is null)
			{
				return Vodovoz.Errors.Clients.DeliveryPointErrors.NotFound;
			}

			if(deliverySchedule is null)
			{
				return Vodovoz.Errors.Logistics.DeliveryScheduleErrors.NotFound;
			}

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
				case PaymentType.Terminal:
					if(createOrderRequest.PaymentByTerminalSource is null)
					{
						throw new InvalidOperationException("Должен быть указан источник оплаты для типа оплаты терминал");
					}

					if(createOrderRequest.PaymentByTerminalSource == PaymentByTerminalSource.ByCard)
					{
						order.PaymentByTerminalSource = PaymentByTerminalSource.ByCard;
						break;
					}

					if(createOrderRequest.PaymentByTerminalSource == PaymentByTerminalSource.ByQR)
					{
						order.PaymentByTerminalSource = PaymentByTerminalSource.ByQR;
						break;
					}

					throw new InvalidOperationException("Обработчик не смог обработать источник оплаты, не было предусмотрено");
			}

			order.DeliverySchedule = deliverySchedule;
			order.UpdateDeliveryDate(createOrderRequest.Date, _orderContractUpdater, out var updateDeliveryDate);

			_orderContractUpdater.UpdateOrCreateContract(unitOfWork, order);

			var nomenclatureIds = createOrderRequest.SaleItems
				.Select(si => si.NomenclatureId)
				.ToArray();

			var nomenclaturesToAdd = _nomenclatureGenericRepository
				.Get(
					unitOfWork,
					NomenclatureSpecifications.CreateForIds(nomenclatureIds))
				.ToArray();

			var nomenclaturesParameters = _robotMiaParametersRepository
				.Get(unitOfWork, RobotMiaParametersSpecifications.CreateForHasNomenclatureIds(nomenclatureIds))
				.ToArray();

			foreach(var waterInfo in createOrderRequest.SaleItems)
			{
				var nomenclature = nomenclaturesToAdd
					.FirstOrDefault(x => x.Id == waterInfo.NomenclatureId);

				if(nomenclature is null)
				{
					_logger.LogError("Попытка добавить отсутствующую номенклатуру {NomenclatureId}", waterInfo.NomenclatureId);
				}
				else if(nomenclature.Category == NomenclatureCategory.water)
				{
					order.AddWaterForSale(unitOfWork, _orderContractUpdater, nomenclature, waterInfo.BottlesCount);
				}
				else
				{
					var nomenclatureParameters = nomenclaturesParameters
						.FirstOrDefault(x => x.NomenclatureId == nomenclature.Id);

					if(nomenclature.Id != ForfeitNomenclatureId
						&& (nomenclatureParameters is null
							|| nomenclatureParameters.GoodsOnlineAvailability != GoodsOnlineAvailability.ShowAndSale))
					{
						throw new InvalidOperationException(
							$"Номенклатура [{nomenclature.Id}] {nomenclature.Name} не может быть добавлена. В заказ может быть добавлена либо номенклатура, одобренная для продажи, либо неустойка");
					}
					order.AddNomenclature(unitOfWork, _orderContractUpdater, nomenclature, waterInfo.BottlesCount);
				}
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

			if(createOrderRequest.TareNonReturnReasonId != null)
			{
				var tareNonReturnReason = unitOfWork.GetById<NonReturnReason>(createOrderRequest.TareNonReturnReasonId.Value);
				order.TareNonReturnReason = tareNonReturnReason;
				order.OPComment = $"Робот Мия: {tareNonReturnReason.Name}.";
			}

			return order;
		}

		public void AddLogisticsRequirements(Order order)
		{
			order.LogisticsRequirements = GetLogisticsRequirements(order);
		}

		public async Task<int> TryCreateOrderFromOnlineOrderAndAcceptAsync(
			IUnitOfWork uow, 
			OnlineOrder onlineOrder,
			IRouteListService routeListService,
			CancellationToken cancellationToken
		)
		{
			if(onlineOrder.IsNeedConfirmationByCall)
			{
				return 0;
			}

			Employee employee = null;
			switch(onlineOrder.Source)
			{
				case Source.MobileApp:
					employee = await uow.Session.GetAsync<Employee>(_employeeSettings.MobileAppEmployee, cancellationToken);
					break;
				case Source.VodovozWebSite:
					employee = await uow.Session.GetAsync<Employee>(_employeeSettings.VodovozWebSiteEmployee, cancellationToken);
					break;
				case Source.KulerSaleWebSite:
					employee = await uow.Session.GetAsync<Employee>(_employeeSettings.KulerSaleWebSiteEmployee, cancellationToken);
					break;
			}

			// Необходимо сделать асинхронным
			var order = _orderFromOnlineOrderCreator.CreateOrderFromOnlineOrder(uow, employee, onlineOrder);

			// Необходимо сделать асинхронным
			UpdateDeliveryCost(uow, order);

			// Необходимо сделать асинхронным
			AddLogisticsRequirements(order);

			order.AddDeliveryPointCommentToOrder();

			// Необходимо сделать асинхронным
			order.AddFastDeliveryNomenclatureIfNeeded(uow, _orderContractUpdater);

			await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);

			var acceptResult = await _orderConfirmationService.TryAcceptOrderCreatedByOnlineOrderAsync(
				uow,
				employee,
				order,
				routeListService,
				cancellationToken
			);

			if(acceptResult.IsFailure)
			{
				return 0;
			}

			onlineOrder.SetOrderPerformed(new []{ order }, employee);
			var notification = OnlineOrderStatusUpdatedNotification.Create(onlineOrder);

			await uow.SaveAsync(notification, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);

			return order.Id;
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
				.Count(x => x.Type == Core.Domain.Documents.DocumentContainerType.Bill) > 0;
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

		public void UpdatePaymentStatus(IUnitOfWork uow, Order order)
		{
			if(order.PaymentType != PaymentType.Cashless)
			{
				order.OrderPaymentStatus = OrderPaymentStatus.None;
				return;
			}

			if(order.Id == 0)
			{
				order.OrderPaymentStatus = OrderPaymentStatus.UnPaid;
				return;
			}

			var allocatedSum = _paymentItemsRepository.GetAllocatedSumForOrder(uow, order.Id);
			UpdatePaymentStatusByAllocatedSum(order, allocatedSum);
		}

		public async Task UpdatePaymentStatusAsync(IUnitOfWork uow, Order order, CancellationToken cancellationToken)
		{
			if(order.PaymentType != PaymentType.Cashless)
			{
				order.OrderPaymentStatus = OrderPaymentStatus.None;
				return;
			}

			if(order.Id == 0)
			{
				order.OrderPaymentStatus = OrderPaymentStatus.UnPaid;
				return;
			}

			var allocatedSum = await _paymentItemsRepository.GetAllocatedSumForOrderAsync(uow, order.Id, cancellationToken);
			UpdatePaymentStatusByAllocatedSum(order, allocatedSum);
		}

		private void UpdatePaymentStatusByAllocatedSum(Order order, decimal allocatedSum)
		{
			var isUnpaid = allocatedSum == default;
			var isPaid = allocatedSum >= order.OrderSum;

			//т.к. имеем скриптовое распределение на заказы до старта выписки без операций,
			//то не трогаем статус оплаты заказов ранее 12.08.2020
			if(order.DeliveryDate < new DateTime(2020, 8, 12)
				&& order.PaymentType == PaymentType.Cashless
				&& isUnpaid)
			{
				return;
			}

			if(isUnpaid)
			{
				order.OrderPaymentStatus = OrderPaymentStatus.UnPaid;
			}
			else if(isPaid)
			{
				order.OrderPaymentStatus = OrderPaymentStatus.Paid;
			}
			else
			{
				order.OrderPaymentStatus = OrderPaymentStatus.PartiallyPaid;
			}
		}

		public void RejectOrderTrueMarkCodes(IUnitOfWork uow, int orderId)
		{
			var requests = uow.Session.QueryOver<FormalEdoRequest>()
				.Where(x => x.Order.Id == orderId)
				.List();

			var requestWithCodes = requests.Where(x => x.ProductCodes.Any()).FirstOrDefault();
			if(requestWithCodes == null)
			{
				return;
			}

			if(requestWithCodes.Task == null || requestWithCodes.Task.Status != EdoTaskStatus.Cancelled)
			{
				throw new InvalidOperationException("Отклонить коды можно только у отмененной ЭДО задачи");
			}

			foreach(var productCode in requestWithCodes.ProductCodes)
			{
				productCode.SourceCodeStatus = SourceProductCodeStatus.Rejected;
				productCode.ResultCode = null;
				uow.Save(productCode);
			}
		}
	}
}
