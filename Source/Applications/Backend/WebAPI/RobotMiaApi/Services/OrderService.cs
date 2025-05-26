using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RobotMiaApi.Contracts.Requests.V1;
using RobotMiaApi.Contracts.Responses.V1;
using RobotMiaApi.Extensions.Mapping;
using RobotMiaApi.Specifications;
using Sms.Internal;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Factories;
using Vodovoz.Models;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Roboats;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters;
using VodovozBusiness.Specifications.Orders;
using static RobotMiaApi.Errors.RobotMiaErrors;
using IVodovozOrderService = VodovozBusiness.Services.Orders.IOrderService;
using PaymentType = RobotMiaApi.Contracts.Requests.V1.PaymentType;
using VodovozCreateOrderRequest = VodovozBusiness.Services.Orders.CreateOrderRequest;
using VodovozPaymentType = Vodovoz.Domain.Client.PaymentType;

namespace RobotMiaApi.Services
{
	/// <inheritdoc cref="IOrderService"/>
	public class OrderService : IOrderService
	{
		private const int _smsSendIntervalInSeconds = 60;
		private const int _smsSendMaxCount = 3;

		private static readonly OrderStatus[] _lastOrderCompletedStatuses = new OrderStatus[]
		{
			OrderStatus.Shipped,
			OrderStatus.Closed,
			OrderStatus.UnloadingOnStock
		};

		private readonly int _forfeitNomenclatureId;

		private readonly ILogger<OrderService> _logger;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IRoboatsSettings _roboatsSettings;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly IVodovozOrderService _vodovozOrderService;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IFastPaymentSender _fastPaymentSender;
		private readonly IGenericRepository<RobotMiaParameters> _robotMiaParametersRepository;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IOrderDailyNumberController _orderDailyNumberController;
		private readonly ICounterpartyContractRepository _counterpartyContractRepository;
		private readonly ICounterpartyContractFactory _counterpartyContractFactory;
		private readonly IPaymentFromBankClientController _paymentFromBankClientController;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="nomenclatureSettings"></param>
		/// <param name="roboatsSettings"></param>
		/// <param name="orderRepository"></param>
		/// <param name="vodovozOrderService"></param>
		/// <param name="employeeRepository"></param>
		/// <param name="unitOfWork"></param>
		/// <param name="unitOfWorkFactory"></param>
		/// <param name="fastPaymentSender"></param>
		/// <param name="robotMiaParametersRepository"></param>
		/// <param name="callTaskWorker"></param>
		/// <param name="orderDailyNumberController"></param>
		/// <param name="counterpartyContractRepository"></param>
		/// <param name="counterpartyContractFactory"></param>
		/// <param name="paymentFromBankClientController"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public OrderService(
			ILogger<OrderService> logger,
			INomenclatureSettings nomenclatureSettings,
			IRoboatsSettings roboatsSettings,
			IGenericRepository<Order> orderRepository,
			IVodovozOrderService vodovozOrderService,
			IEmployeeRepository employeeRepository,
			IUnitOfWork unitOfWork,
			IUnitOfWorkFactory unitOfWorkFactory,
			IFastPaymentSender fastPaymentSender,
			IGenericRepository<RobotMiaParameters> robotMiaParametersRepository,
			ICallTaskWorker callTaskWorker,
			IOrderDailyNumberController orderDailyNumberController,
			ICounterpartyContractRepository counterpartyContractRepository,
			ICounterpartyContractFactory counterpartyContractFactory,
			IPaymentFromBankClientController paymentFromBankClientController)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_nomenclatureSettings = nomenclatureSettings
				?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_roboatsSettings = roboatsSettings
				?? throw new ArgumentNullException(nameof(roboatsSettings));
			_orderRepository = orderRepository
				?? throw new ArgumentNullException(nameof(orderRepository));
			_vodovozOrderService = vodovozOrderService
				?? throw new ArgumentNullException(nameof(vodovozOrderService));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_fastPaymentSender = fastPaymentSender
				?? throw new ArgumentNullException(nameof(fastPaymentSender));
			_robotMiaParametersRepository = robotMiaParametersRepository
				?? throw new ArgumentNullException(nameof(robotMiaParametersRepository));
			_callTaskWorker = callTaskWorker
				?? throw new ArgumentNullException(nameof(callTaskWorker));
			_orderDailyNumberController = orderDailyNumberController
				?? throw new ArgumentNullException(nameof(orderDailyNumberController));
			_counterpartyContractRepository = counterpartyContractRepository
				?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
			_counterpartyContractFactory = counterpartyContractFactory
				?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
			_paymentFromBankClientController = paymentFromBankClientController
				?? throw new ArgumentNullException(nameof(paymentFromBankClientController));

			_forfeitNomenclatureId = nomenclatureSettings.ForfeitId;
		}

		/// <inheritdoc/>
		public LastOrderResponse GetLastOrderByCounterpartyId(int counterpartyId)
		{
			var order = _orderRepository
				.Get(
					_unitOfWork,
					OrderSpecification
						.CreateForCounterpartyId(counterpartyId)
						.And(LastOrderSpecification.CreateForValidRobotMiaLastOrders(
							_lastOrderCompletedStatuses,
							DateTime.Today.AddMonths(-_roboatsSettings.OrdersInMonths),
							_nomenclatureSettings.PaidDeliveryNomenclatureId,
							_nomenclatureSettings.FastDeliveryNomenclatureId)))
				.FirstOrDefault();

			if(order is null)
			{
				return null;
			}

			var deliveryItem = order.OrderItems
				.FirstOrDefault(x => x.Nomenclature.Id == _nomenclatureSettings.PaidDeliveryNomenclatureId);

			if(deliveryItem != null)
			{
				order.OrderItems.Remove(deliveryItem);
			}

			return order.MapToLastOrderResponseV1();
		}

		/// <inheritdoc/>
		public LastOrderResponse GetLastOrderByDeliveryPointId(int deliveryPointId)
		{
			var order = _orderRepository
				.Get(
					_unitOfWork,
					OrderSpecification
						.CreateForDeliveryPointId(deliveryPointId)
						.And(LastOrderSpecification.CreateForValidRobotMiaLastOrders(
							_lastOrderCompletedStatuses,
							DateTime.Today.AddMonths(-_roboatsSettings.OrdersInMonths),
							_nomenclatureSettings.PaidDeliveryNomenclatureId,
							_nomenclatureSettings.FastDeliveryNomenclatureId)))
				.FirstOrDefault();

			if(order is null)
			{
				return null;
			}

			var deliveryItem = order.OrderItems
				.FirstOrDefault(x => x.Nomenclature.Id == _nomenclatureSettings.PaidDeliveryNomenclatureId);

			if(deliveryItem != null)
			{
				order.OrderItems.Remove(deliveryItem);
			}

			return order.MapToLastOrderResponseV1();
		}

		/// <inheritdoc/>
		public async Task<Result> CreateIncompleteOrderAsync(CreateOrderRequest createOrderRequest)
		{
			if(createOrderRequest is null)
			{
				throw new ArgumentNullException(nameof(createOrderRequest));
			}

			using var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>();

			var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);

			if(roboatsEmployee is null)
			{
				return await Task.FromResult(Vodovoz.Application.Errors.ServiceEmployee.MissingServiceUser);
			}

			var order = CreateOrder(unitOfWork, roboatsEmployee, createOrderRequest);
			order.SaveEntity(unitOfWork, roboatsEmployee, _orderDailyNumberController, _paymentFromBankClientController);
			return Result.Success();
		}

		/// <inheritdoc/>
		public async Task<Result<int>> CreateAndAcceptOrderAsync(CreateOrderRequest createOrderRequest)
		{
			var orderArgs = createOrderRequest.MapToCreateOrderRequest();

			if(createOrderRequest.PaymentType == PaymentType.SmsQR)
			{
				var orderData = _vodovozOrderService.CreateIncompleteOrder(orderArgs);

				var paymentSent = await TryingSendPayment(createOrderRequest.ContactPhone, orderData.OrderId);

				if(paymentSent)
				{
					orderData = _vodovozOrderService.AcceptOrder(orderData.OrderId, orderData.AuthorId);
				}

				return orderData.OrderId;
			}

			return CreateAndAcceptOrder(createOrderRequest);
		}

		/// <inheritdoc/>
		public Result<(decimal orderPrice, decimal deliveryPrice, decimal forfeitPrice)> GetOrderAndDeliveryPrices(CalculatePriceRequest calculatePriceRequest)
		{
			try
			{
				return _vodovozOrderService.GetOrderAndDeliveryPrices(calculatePriceRequest.MapToCreateOrderRequest());
			}
			// TODO: Заменить этот код и вызываемый код на код использующий результаты вместо Exception,
			// либо поменять на новую архитектуру работы с ошибками, использующую Exception вместо Error
			// В текущем исполнении - может работать не стабильно
			catch(InvalidOperationException invalidOperationException)
			{
				return OrderErrors.AddNomenclatureError(invalidOperationException.Message);
			}
		}

		private async Task<bool> TryingSendPayment(string phone, int orderId)
		{
			FastPaymentResult result;
			var attemptsCount = 0;

			do
			{
				if(attemptsCount > 0)
				{
					await Task.Delay(_smsSendIntervalInSeconds * 1000);
				}

				result = await _fastPaymentSender.SendFastPaymentUrlAsync(orderId, phone, true);

				if(result.Status == ResultStatus.Error && result.OrderAlreadyPaied)
				{
					return true;
				}

				attemptsCount++;

			} while(result.Status == ResultStatus.Error && attemptsCount < _smsSendMaxCount);

			return result.Status == ResultStatus.Ok;
		}

		private int CreateAndAcceptOrder(CreateOrderRequest createOrderRequest)
		{
			if(createOrderRequest is null)
			{
				throw new ArgumentNullException(nameof(createOrderRequest));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork)
					?? throw new InvalidOperationException("Требуется сотрудник. Еслм сообщение получено в сервисе - убедитесь, что настроили сервис корректно и в ДВ есть соответствующий сотрудник");

				var order = CreateOrder(unitOfWork, roboatsEmployee, createOrderRequest);
				order.AcceptOrder(roboatsEmployee, _callTaskWorker);
				order.SaveEntity(unitOfWork, roboatsEmployee, _orderDailyNumberController, _paymentFromBankClientController);
				return order.Id;
			}
		}

		private Order CreateOrder(IUnitOfWorkGeneric<Order> unitOfWork, Employee author, CreateOrderRequest createOrderRequest)
		{
			var counterparty = unitOfWork.GetById<Counterparty>(createOrderRequest.CounterpartyId);
			var deliveryPoint = unitOfWork.GetById<DeliveryPoint>(createOrderRequest.DeliveryPointId);
			var deliverySchedule = unitOfWork.GetById<DeliverySchedule>(createOrderRequest.DeliveryIntervalId.Value);
			Order order = unitOfWork.Root;
			order.Author = author;
			order.Client = counterparty;
			order.DeliveryPoint = deliveryPoint;
			order.DriverMobileAppComment = createOrderRequest.DriverAppComment;
			order.DriverMobileAppCommentTime = DateTime.Now;
			order.CallBeforeArrivalMinutes = createOrderRequest.CallBeforeArrivalMinutes ?? 15;

			if(createOrderRequest.PaymentType == PaymentType.Cash)
			{
				order.Trifle = createOrderRequest.Trifle;
			}
			else
			{
				order.Trifle = 0;
			}

			switch(createOrderRequest.PaymentType)
			{
				case PaymentType.Cash:
					order.Trifle = createOrderRequest.Trifle;
					break;
				case PaymentType.TerminalQR:
					order.PaymentType = VodovozPaymentType.Terminal;
					order.PaymentByTerminalSource = PaymentByTerminalSource.ByQR;
					break;
				case PaymentType.TerminalCard:
					order.PaymentType = VodovozPaymentType.Terminal;
					order.PaymentByTerminalSource = PaymentByTerminalSource.ByCard;
					break;
				case PaymentType.SmsQR:
					order.PaymentType = VodovozPaymentType.SmsQR;
					break;
			}

			order.DeliverySchedule = deliverySchedule;
			order.DeliveryDate = createOrderRequest.DeliveryDate;

			order.UpdateOrCreateContract(unitOfWork, _counterpartyContractRepository, _counterpartyContractFactory);

			foreach(var saleItem in createOrderRequest.SaleItems)
			{
				var nomenclature = unitOfWork.GetById<Nomenclature>(saleItem.NomenclatureId)
					?? throw new InvalidOperationException($"Не найдена номенклатура #{saleItem.NomenclatureId}");

				if(nomenclature.Id == _forfeitNomenclatureId)
				{
					order.AddNomenclature(nomenclature, saleItem.Count);
					continue;
				}

				var nomenclatureParameters = _robotMiaParametersRepository
					.Get(unitOfWork, x => x.NomenclatureId == nomenclature.Id, 1)
					.FirstOrDefault();

				if(nomenclature.Category == NomenclatureCategory.water)
				{
					order.AddWaterForSale(nomenclature, saleItem.Count);
				}
				else if(nomenclatureParameters is null
					|| nomenclatureParameters.GoodsOnlineAvailability != GoodsOnlineAvailability.ShowAndSale)
				{
					throw new InvalidOperationException(
						$"Номенклатура [{nomenclature.Id}] {nomenclature.Name} не может быть добавлена. В заказ может быть добавлена либо номенклатура, одобренная для продажи, либо неустойка");
				}
				else
				{
					order.AddNomenclature(nomenclature, saleItem.Count);
				}
			}
			order.BottlesReturn = createOrderRequest.BottlesReturn;
			order.RecalculateItemsPrice();
			_vodovozOrderService.UpdateDeliveryCost(unitOfWork, order);
			_vodovozOrderService.AddLogisticsRequirements(order);
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
	}
}
