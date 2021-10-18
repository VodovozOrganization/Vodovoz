using Bitrix;
using Bitrix.DTO;
using NLog;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Factories;
using Vodovoz.Services;
using Vodovoz.Tools.CallTasks;
using BitrixPhone = Bitrix.DTO.Phone;

namespace BitrixIntegration.Processors
{
	//Класс перегружен, его необходимо разделить на зависимости отвечающие за свои действия
	public class DealProcessor
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IBitrixClient _bitrixClient;
		private readonly ICounterpartyContractRepository _counterpartyContractRepository;
		private readonly CounterpartyContractFactory _counterpartyContractFactory;
		private readonly DealRegistrator _dealRegistrator;
		private readonly IBitrixServiceSettings _bitrixServiceSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository;
		private readonly IDeliveryPointProcessor _deliveryPointProcessor;
		private readonly IProductProcessor _productProcessor;
		private readonly ICounterpartyProcessor _counterpartyProcessor;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IFlyerRepository _flyerRepository;
		private readonly IRouteListParametersProvider _routeListParametersProvider;

		private readonly Employee _bitrixAccount;

		public DealProcessor(
			IUnitOfWorkFactory uowFactory,
			IBitrixClient bitrixClient,
			ICounterpartyContractRepository counterpartyContractRepository,
			CounterpartyContractFactory counterpartyContractFactory,
			DealRegistrator dealRegistrator,
			IBitrixServiceSettings bitrixServiceSettings,
			IOrderRepository orderRepository,
			IDeliveryScheduleRepository deliveryScheduleRepository,
			IDeliveryPointProcessor deliveryPointProcessor,
			IProductProcessor productProcessor,
			ICounterpartyProcessor counterpartyProcessor,
			ICallTaskWorker callTaskWorker,
			IFlyerRepository flyerRepository,
			IRouteListParametersProvider routeListParametersProvider
		)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_bitrixClient = bitrixClient ?? throw new ArgumentNullException(nameof(bitrixClient));
			_bitrixServiceSettings = bitrixServiceSettings ?? throw new ArgumentNullException(nameof(bitrixServiceSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_deliveryScheduleRepository = deliveryScheduleRepository ?? throw new ArgumentNullException(nameof(deliveryScheduleRepository));
			_deliveryPointProcessor = deliveryPointProcessor ?? throw new ArgumentNullException(nameof(deliveryPointProcessor));
			_productProcessor = productProcessor ?? throw new ArgumentNullException(nameof(productProcessor));
			_counterpartyContractRepository =
				counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
			_counterpartyContractFactory =
				counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
			_dealRegistrator = dealRegistrator ?? throw new ArgumentNullException(nameof(dealRegistrator));
			_counterpartyProcessor = counterpartyProcessor ?? throw new ArgumentNullException(nameof(counterpartyProcessor));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
			_routeListParametersProvider =
				routeListParametersProvider ?? throw new ArgumentNullException(nameof(routeListParametersProvider));

			using var uow = _uowFactory.CreateWithoutRoot("Получение сотрудника для создания заказов из сделок");
			_bitrixAccount = uow.GetById<Employee>(_bitrixServiceSettings.EmployeeForOrderCreate);
		}

		public void ProcessDeals(DateTime date)
		{
			var startDay = date.Date;
			var endDay = date.Date.AddDays(1).AddMilliseconds(-1);

			var deals = _bitrixClient.GetDeals(startDay, endDay).GetAwaiter().GetResult();
			foreach(var deal in deals)
			{
				_dealRegistrator.RegisterDealAsInProgress(deal.Id);
				try
				{
					var order = ProcessDeal(deal);
					_dealRegistrator.BindOrderToRegistration(deal.Id, order);
				}
				catch(Exception e)
				{
					_dealRegistrator.RegisterDealAsError(deal.Id, e.Message);
				}
			}
		}

		private Order ProcessDeal(Deal deal)
		{
			_logger.Info($"Обработка сделки: {deal.Id}");

			//ВНИМАНИЕ! Тут реализована обработка сделки на основании создания нового заказа
			//необходимо сделать обработку заказа универсально как для нового так и для существующего
			//чтобы без опаски можно было прогнать сделку на существующем заказе и актуализировать информацию в нем

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var order = _orderRepository.GetOrderByBitrixId(uow, deal.Id);
				if(order != null)
				{
					_logger.Info($"Обработка сделки пропущена. Для сделки №{deal.Id} уже есть существующий заказ №{order.Id}.");
					return order;
				}

				order = _orderRepository.GetOrderByOnlineOrderId(uow, deal.OrderNumber ?? -1);
				if(order != null)
				{
					_logger.Info(
						$"Обработка сделки пропущена. Для заказа с сайта №{deal.OrderNumber} уже есть существующий заказ №{order.Id}.");
					return order;
				}

				_logger.Info("Обработка контрагента");
				var counterparty = _counterpartyProcessor.ProcessCounterparty(uow, deal);
				//для возможности создать контракт в заказе если создан новый клиент
				uow.Save(counterparty);

				DeliveryPoint deliveryPoint = null;
				if(!deal.IsSelfDelivery)
				{
					_logger.Info("Обработка точки доставки");
					deliveryPoint = _deliveryPointProcessor.ProcessDeliveryPoint(uow, deal, counterparty);
					uow.Save(deliveryPoint);
				}

				_logger.Info("Сборка заказа");

				order = CreateOrder(uow, deal, counterparty, deliveryPoint);

				_logger.Info("Обработка номенклатур");
				_productProcessor.ProcessProducts(uow, deal, order);

				FindExistingOrder(uow, order);

				foreach(var orderItem in order.OrderItems)
				{
					uow.Save(orderItem.Nomenclature);
				}

				order.UpdateOrCreateContract(uow, _counterpartyContractRepository, _counterpartyContractFactory);
				order.CalculateDeliveryPrice();
				order.AcceptOrder(_bitrixAccount, _callTaskWorker);

				uow.Save(order);
				uow.Commit();
				return order;
			}
		}

		private void FindExistingOrder(IUnitOfWork uow, Order order)
		{
			if(order.SelfDelivery)
			{
				return;
			}

			var duplicate = _orderRepository
					.GetSameOrderForDateAndDeliveryPoint(_uowFactory, order.DeliveryDate.Value.Date, order.DeliveryPoint)
					.Where(o => o.Id != order.Id
					            && !_orderRepository.GetGrantedStatusesToCreateSeveralOrders().Contains(o.OrderStatus)
					            && o.OrderAddressType != OrderAddressType.Service)
					.FirstOrDefault();

			var hasMaster = order.OrderItems.Any(oi => oi.Nomenclature.Category == NomenclatureCategory.master);

			if(!hasMaster && duplicate != null)
			{
				throw new InvalidOperationException(
					$"Обработка сделки пропущена. Для данной точки и даты доставки уже создан заказ {duplicate.Id}");
			}
		}

		private Order CreateOrder(IUnitOfWork uow, Deal deal, Counterparty counterparty, DeliveryPoint deliveryPoint)
		{
			DeliverySchedule deliverySchedule = null;

			if(!deal.IsSelfDelivery)
			{
				deliverySchedule = _deliveryScheduleRepository.GetByBitrixId(uow, deal.DeliverySchedule);
				if(deliverySchedule == null)
				{
					throw new InvalidOperationException($"Не найдено время доставки DeliverySchedule ({deal.DeliverySchedule}) по bitrixId");
				}
			}

			var order = new Order()
			{
				UoW = uow,
				PaymentType = deal.GetPaymentMethod(),
				CreateDate = deal.CreateDate,
				DeliveryDate = deal.DeliveryDate,
				DeliverySchedule = deliverySchedule,
				Client = counterparty,
				DeliveryPoint = deliveryPoint,
				OrderStatus = OrderStatus.NewOrder,
				Author = _bitrixAccount,
				LastEditor = _bitrixAccount,
				LastEditedTime = DateTime.Now,
				PaymentBySms = deal.IsSmsPayment,
				OrderPaymentStatus = deal.GetOrderPaymentStatus(),
				SelfDelivery = deal.IsSelfDelivery,
				Comment = deal.Comment,
				Trifle = deal.Trifle ?? 0,
				BottlesReturn = deal.BottlesToReturn ?? 0,
				EShopOrder = (int)deal.Id,
				OnlineOrder = deal.OrderNumber ?? null
			};

			if(order.PaymentType == PaymentType.ByCard)
			{
				order.PaymentByCardFrom = uow.GetById<PaymentFrom>(7);
			}

			order.TryAddFlyers(uow, _routeListParametersProvider);

			return order;
		}
	}
}
