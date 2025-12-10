using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Sms.Internal;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Factories;
using Vodovoz.Models;
using Vodovoz.RobotMia.Api.Exceptions;
using Vodovoz.RobotMia.Api.Extensions.Mapping;
using Vodovoz.RobotMia.Api.Specifications;
using Vodovoz.RobotMia.Contracts.Requests.V1;
using Vodovoz.RobotMia.Contracts.Responses.V1;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Roboats;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Specifications.Orders;
using static RobotMiaApi.Errors.RobotMiaErrors;
using CreateOrderRequest = Vodovoz.RobotMia.Contracts.Requests.V1.CreateOrderRequest;
using IVodovozOrderService = VodovozBusiness.Services.Orders.IOrderService;
using PaymentType = Vodovoz.RobotMia.Contracts.Requests.V1.PaymentType;
using VodovozPaymentType = Vodovoz.Domain.Client.PaymentType;

namespace Vodovoz.RobotMia.Api.Services
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

		private readonly ILogger<OrderService> _logger;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IRoboatsSettings _roboatsSettings;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly IVodovozOrderService _vodovozOrderService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IFastPaymentSender _fastPaymentSender;
		private readonly IGenericRepository<RobotMiaParameters> _robotMiaParametersRepository;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IOrderDailyNumberController _orderDailyNumberController;
		private readonly ICounterpartyContractRepository _counterpartyContractRepository;
		private readonly IPaymentFromBankClientController _paymentFromBankClientController;
		private readonly IOrderContractUpdater _contractUpdater;
		private readonly IOrderConfirmationService _orderConfirmationService;
		private readonly Employee _robotMiaEmployee;

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
		/// <param name="contractUpdater"></param>
		/// <param name="orderConfirmationService"></param>
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
			IPaymentFromBankClientController paymentFromBankClientController,
			IOrderContractUpdater contractUpdater,
			IOrderConfirmationService orderConfirmationService)
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
			_paymentFromBankClientController = paymentFromBankClientController
				?? throw new ArgumentNullException(nameof(paymentFromBankClientController));
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));
			_orderConfirmationService = orderConfirmationService ?? throw new ArgumentNullException(nameof(orderConfirmationService));

			_robotMiaEmployee = employeeRepository.GetEmployeeForCurrentUser(unitOfWork);
		}

		/// <inheritdoc/>
		public LastOrderResponse GetLastOrderByCounterpartyId(int counterpartyId)
		{
			var order = _orderRepository
				.GetLastOrDefault(
					_unitOfWork,
					OrderSpecification
						.CreateForCounterpartyId(counterpartyId)
						.And(LastOrderSpecification.CreateForValidRobotMiaLastOrders(
							_lastOrderCompletedStatuses,
							DateTime.Today.AddMonths(-_roboatsSettings.OrdersInMonths),
							_nomenclatureSettings.PaidDeliveryNomenclatureId,
							_nomenclatureSettings.FastDeliveryNomenclatureId,
							_nomenclatureSettings.ForfeitId)));

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
				.GetLastOrDefault(
					_unitOfWork,
					OrderSpecification
						.CreateForDeliveryPointId(deliveryPointId)
						.And(LastOrderSpecification.CreateForValidRobotMiaLastOrders(
							_lastOrderCompletedStatuses,
							DateTime.Today.AddMonths(-_roboatsSettings.OrdersInMonths),
							_nomenclatureSettings.PaidDeliveryNomenclatureId,
							_nomenclatureSettings.FastDeliveryNomenclatureId,
							_nomenclatureSettings.ForfeitId)));

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
		public async Task<Result<int>> CreateIncompleteOrderAsync(CreateOrderRequest createOrderRequest)
		{
			if(createOrderRequest is null)
			{
				throw new ArgumentNullException(nameof(createOrderRequest));
			}

			using var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>();

			var order = CreateOrder(unitOfWork, _robotMiaEmployee, createOrderRequest);
			order.SaveEntity(
				unitOfWork,
				_contractUpdater,
				_robotMiaEmployee,
				_orderDailyNumberController,
				_paymentFromBankClientController);
			return order.Id;
		}

		/// <inheritdoc/>
		public async Task<Result<int>> CreateAndAcceptOrderAsync(CreateOrderRequest createOrderRequest)
		{
			if(createOrderRequest.PaymentType == PaymentType.SmsQR)
			{
				using var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>();

				var order = CreateOrder(unitOfWork, _robotMiaEmployee, createOrderRequest);
				order.SaveEntity(
					unitOfWork,
					_contractUpdater,
					_robotMiaEmployee,
					_orderDailyNumberController,
					_paymentFromBankClientController);

				var paymentSent = await TryingSendPayment(createOrderRequest.ContactPhone, order.Id);

				if(paymentSent)
				{
					_vodovozOrderService.AcceptOrder(order.Id, order.Author.Id);
				}

				return order.Id;
			}

			return CreateAndAcceptOrder(createOrderRequest);
		}

		/// <inheritdoc/>
		public Result<CalculatePriceResponse> GetOrderAndDeliveryPrices(CalculatePriceRequest calculatePriceRequest)
		{
			try
			{
				if(calculatePriceRequest is null)
				{
					throw new ArgumentNullException(nameof(calculatePriceRequest));
				}

				using var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>("Сервис заказов: подсчет стоимости заказа");

				var counterparty = unitOfWork.GetById<Counterparty>(calculatePriceRequest.CounterpartyId)
					?? throw new InvalidOperationException($"Не найден контрагент #{calculatePriceRequest.CounterpartyId}");
				var deliveryPoint = unitOfWork.GetById<DeliveryPoint>(calculatePriceRequest.DeliveryPointId)
					?? throw new InvalidOperationException($"Не найдена точка доставки #{calculatePriceRequest.DeliveryPointId}");

				var deliverySchedule =  unitOfWork.GetById<DeliverySchedule>(calculatePriceRequest.DeliveryIntervalId);

				Order order = unitOfWork.Root;
				order.Author = _robotMiaEmployee;
				order.UpdateClient(counterparty, _contractUpdater, out var updateClientMessage);
				order.UpdateDeliveryPoint(deliveryPoint, _contractUpdater);
				order.UpdatePaymentType(VodovozPaymentType.Cash, _contractUpdater);

				order.DeliverySchedule = deliverySchedule;
				order.UpdateDeliveryDate(calculatePriceRequest.DeliveryDate, _contractUpdater, out var updateDeliveryDateMessage);

				var nomenclaturesToAddIds = calculatePriceRequest.OrderSaleItems.Select(x => x.NomenclatureId).ToArray();

				var nomenclaturesParameters = _robotMiaParametersRepository
					.Get(unitOfWork, x => nomenclaturesToAddIds.Contains(x.NomenclatureId.Value))
					.ToDictionary(x => x.NomenclatureId, x => x);

				foreach(var saleItem in calculatePriceRequest.OrderSaleItems)
				{
					var nomenclature = unitOfWork.GetById<Nomenclature>(saleItem.NomenclatureId)
						?? throw new InvalidOperationException($"Не найдена номенклатура #{saleItem.NomenclatureId}");

					if(nomenclature.Id == _nomenclatureSettings.ForfeitId)
					{
						order.AddNomenclature(unitOfWork, _contractUpdater, nomenclature, saleItem.Count);
						continue;
					}

					if(nomenclature.Category == NomenclatureCategory.water)
					{
						order.AddWaterForSale(unitOfWork, _contractUpdater, nomenclature, saleItem.Count);
					}
					else if(!nomenclaturesParameters.ContainsKey(nomenclature.Id)
						|| nomenclaturesParameters[nomenclature.Id].GoodsOnlineAvailability != GoodsOnlineAvailability.ShowAndSale)
					{
						throw new InvalidOperationException(
							$"Номенклатура [{nomenclature.Id}] {nomenclature.Name} не может быть добавлена. В заказ может быть добавлена либо номенклатура, одобренная для продажи, либо неустойка");
					}
					else
					{
						order.AddNomenclature(unitOfWork, _contractUpdater, nomenclature, saleItem.Count);
					}
				}

				order.RecalculateItemsPrice();
				_vodovozOrderService.UpdateDeliveryCost(unitOfWork, order);

				return new CalculatePriceResponse
				{
					OrderPrice = order.OrderSum,
					DeliveryPrice = order.OrderItems.Where(oi => oi.Nomenclature.Id == _nomenclatureSettings.PaidDeliveryNomenclatureId).FirstOrDefault()?.ActualSum ?? 0m,
					ForfeitPrice = order.OrderItems.Where(oi => oi.Nomenclature.Id == _nomenclatureSettings.ForfeitId).Sum(x => x.ActualSum)
				};
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

			using var unitOfWork = _unitOfWorkFactory.CreateWithNewRoot<Order>();

			var order = CreateOrder(unitOfWork, _robotMiaEmployee, createOrderRequest);
			_orderConfirmationService.AcceptOrder(unitOfWork, _robotMiaEmployee, order);
			return order.Id;
		}

		private Order CreateOrder(IUnitOfWorkGeneric<Order> unitOfWork, Employee author, CreateOrderRequest createOrderRequest)
		{
			var counterparty = unitOfWork.GetById<Counterparty>(createOrderRequest.CounterpartyId);
			var deliveryPoint = unitOfWork.GetById<DeliveryPoint>(createOrderRequest.DeliveryPointId);
			DeliverySchedule deliverySchedule = null;

			if(createOrderRequest.DeliveryIntervalId.HasValue)
			{
				deliverySchedule = unitOfWork.GetById<DeliverySchedule>(createOrderRequest.DeliveryIntervalId.Value);
			}

			Order order = unitOfWork.Root;
			order.Author = author;
			order.UpdateClient(counterparty, _contractUpdater, out var updateClientMessage);
			order.UpdateDeliveryPoint(deliveryPoint, _contractUpdater);			

			if(!string.IsNullOrWhiteSpace(createOrderRequest.DriverAppComment))
			{
				order.Comment = createOrderRequest.DriverAppComment;
				order.HasCommentForDriver = true;
			}
			
			order.CallBeforeArrivalMinutes = createOrderRequest.CallBeforeArrivalMinutes;

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
					order.UpdatePaymentType(VodovozPaymentType.Cash, _contractUpdater);
					break;
				case PaymentType.TerminalQR:
					order.UpdatePaymentType(VodovozPaymentType.Terminal, _contractUpdater);
					order.PaymentByTerminalSource = PaymentByTerminalSource.ByQR;
					break;
				case PaymentType.TerminalCard:
					order.UpdatePaymentType(VodovozPaymentType.Terminal, _contractUpdater);
					order.PaymentByTerminalSource = PaymentByTerminalSource.ByCard;
					break;
				case PaymentType.SmsQR:
					order.UpdatePaymentType(VodovozPaymentType.SmsQR, _contractUpdater);
					break;
			}

			
			order.DeliverySchedule = deliverySchedule;
			order.UpdateDeliveryDate(createOrderRequest.DeliveryDate, _contractUpdater, out var updateDeliveryDateMessage);

			_contractUpdater.UpdateOrCreateContract(unitOfWork, order);

			order.SignatureType = createOrderRequest.SignatureType.MapToVodovozSignatureType();

			if(!string.IsNullOrWhiteSpace(createOrderRequest.ContactPhone))
			{
				var normalizedContactPhone = createOrderRequest.ContactPhone.NormalizePhone();

				var contactPhone = counterparty.Phones.FirstOrDefault(p => p.DigitsNumber == normalizedContactPhone);

				if(contactPhone == null && deliveryPoint != null)
				{
					contactPhone = deliveryPoint.Phones.FirstOrDefault(p => p.DigitsNumber == normalizedContactPhone);
				}

				if(contactPhone == null)
				{
					_logger.LogWarning(
						"Не найден телефон {ContactPhone} у контрагента {CounterpartyId} и в указанной точке доставки {DeliveryPointId}.",
						normalizedContactPhone,
						createOrderRequest.CounterpartyId,
						createOrderRequest.DeliveryPointId);
				}
				else
				{
					order.ContactPhone = contactPhone;
				}
			}

			var nomenclaturesToAddIds = createOrderRequest.SaleItems.Select(x => x.NomenclatureId).ToArray();

			var nomenclaturesParameters = _robotMiaParametersRepository
				.Get(unitOfWork, x => nomenclaturesToAddIds.Contains(x.NomenclatureId.Value))
				.ToDictionary(x => x.NomenclatureId, x => x);

			foreach(var saleItem in createOrderRequest.SaleItems)
			{
				var nomenclature = unitOfWork.GetById<Nomenclature>(saleItem.NomenclatureId)
					?? throw new NomenclatureNotFoundException(saleItem.NomenclatureId);

				if(nomenclature.Id == _nomenclatureSettings.ForfeitId)
				{
					order.AddNomenclature(unitOfWork, _contractUpdater, nomenclature, saleItem.Count);
					continue;
				}

				else if(!nomenclaturesParameters.ContainsKey(nomenclature.Id)
					|| nomenclaturesParameters[nomenclature.Id].GoodsOnlineAvailability != GoodsOnlineAvailability.ShowAndSale)
				{
					throw new NomenclatureSaleUnavailableException(nomenclature.Id, nomenclature.Name);
				}

				if(nomenclature.Category == NomenclatureCategory.water)
				{
					order.AddWaterForSale(unitOfWork, _contractUpdater, nomenclature, saleItem.Count);
				}
				else
				{
					order.AddNomenclature(unitOfWork, _contractUpdater, nomenclature, saleItem.Count);
				}
			}

			order.BottlesReturn = createOrderRequest.BottlesReturn;
			order.RecalculateItemsPrice();
			_vodovozOrderService.UpdateDeliveryCost(unitOfWork, order);
			_vodovozOrderService.AddLogisticsRequirements(order);
			order.AddDeliveryPointCommentToOrder();

			if(!order.SelfDelivery && order.CallBeforeArrivalMinutes is null)
			{
				order.CallBeforeArrivalMinutes = 15;
				order.IsDoNotMakeCallBeforeArrival = false;
			}

			if(createOrderRequest.TareNonReturnReasonId != null)
			{
				var tareNonReturnReason = unitOfWork.GetById<NonReturnReason>(createOrderRequest.TareNonReturnReasonId.Value);
				order.TareNonReturnReason = tareNonReturnReason;
				if(tareNonReturnReason != null)
				{
					order.OPComment = $"Робот Мия: {tareNonReturnReason.Name}.";
				}
			}

			return order;
		}
	}
}
