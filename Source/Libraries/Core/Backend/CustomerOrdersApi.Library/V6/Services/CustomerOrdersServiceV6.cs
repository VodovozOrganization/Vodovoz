using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.V6.Dto.Orders;
using CustomerOrdersApi.Library.V6.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Orders.V6;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Services.Orders;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V6.Services
{
	public class CustomerOrdersServiceV6 : ICustomerOrdersServiceV6
	{
		private readonly ILogger<CustomerOrdersServiceV6> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISignatureManager _signatureManager;
		private readonly ICustomerOrderFactoryV6 _customerOrderFactory;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IGenericRepository<OrderRating> _genericRatingRepository;
		private readonly IUnPaidOnlineOrderHandler _unPaidOnlineOrderHandler;
		private readonly ICourierTrackingService _courierTrackingService;
		private readonly IOptionsMonitor<CourierCoordinatesOptions> _courierCoordinatesOptions;
		private readonly IConfigurationSection _signaturesSection;

		public CustomerOrdersServiceV6(
			ILogger<CustomerOrdersServiceV6> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISignatureManager signatureManager,
			ICustomerOrderFactoryV6 customerOrderFactory,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IOnlineOrderRepository onlineOrderRepository,
			IRouteListRepository routeListRepository,
			IGenericRepository<OrderRating> genericRatingRepository,
			IUnPaidOnlineOrderHandler unPaidOnlineOrderHandler,
			ICourierTrackingService courierTrackingService,
			IConfiguration configuration,
			IOptionsMonitor<CourierCoordinatesOptions> courierCoordinatesOptions)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_customerOrderFactory = customerOrderFactory ?? throw new ArgumentNullException(nameof(customerOrderFactory));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_genericRatingRepository = genericRatingRepository ?? throw new ArgumentNullException(nameof(genericRatingRepository));
			_unPaidOnlineOrderHandler = unPaidOnlineOrderHandler ?? throw new ArgumentNullException(nameof(unPaidOnlineOrderHandler));
			_courierTrackingService = courierTrackingService ?? throw new ArgumentNullException(nameof(courierTrackingService));
			_courierCoordinatesOptions = courierCoordinatesOptions ?? throw new ArgumentNullException(nameof(courierCoordinatesOptions));

			_signaturesSection = configuration.GetSection("Signatures");
		}

		#region ValidateSignature

		public bool ValidateOrderSignature(ICreatingOnlineOrder creatingOnlineOrder, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(creatingOnlineOrder.Source);

			return _signatureManager.Validate(
				creatingOnlineOrder.Signature,
				new OrderSignatureParams
				{
					OrderId = creatingOnlineOrder.ExternalOrderId.ToString(),
					OrderSumInKopecks = (int)(creatingOnlineOrder.OrderSum * 100),
					ShopId = (int)creatingOnlineOrder.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}

		public bool ValidateOrderRatingSignature(OrderRatingInfoForCreateDto orderRatingInfo, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(orderRatingInfo.Source);

			return _signatureManager.Validate(
				orderRatingInfo.Signature,
				new OrderRatingSignatureParams
				{
					OrderNumber = orderRatingInfo.OrderId.HasValue
						? orderRatingInfo.OrderId.ToString()
						: orderRatingInfo.OnlineOrderId.ToString(),
					Rating = orderRatingInfo.Rating,
					ShopId = (int)orderRatingInfo.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}

		public bool ValidateOrderInfoSignature(GetDetailedOrderInfoDto getDetailedOrderInfoDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(getDetailedOrderInfoDto.Source);

			return _signatureManager.Validate(
				getDetailedOrderInfoDto.Signature,
				new OrderInfoSignatureParams
				{
					OrderId = getDetailedOrderInfoDto.OrderId.HasValue
						? getDetailedOrderInfoDto.OrderId.ToString()
						: getDetailedOrderInfoDto.OnlineOrderId.ToString(),
					ShopId = (int)getDetailedOrderInfoDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}

		public bool ValidateCounterpartyOrdersSignature(GetOrdersDto getOrdersDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(getOrdersDto.Source);

			return _signatureManager.Validate(
				getOrdersDto.Signature,
				new CounterpartyOrdersSignatureParams
				{
					CounterpartyId = getOrdersDto.CounterpartyErpId.ToString(),
					Page = getOrdersDto.Page,
					ShopId = (int)getOrdersDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}

		public bool ValidateOnlineOrderPaymentStatusUpdatedSignature(
			OnlineOrderPaymentStatusUpdatedDto paymentStatusUpdatedDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(paymentStatusUpdatedDto.Source);

			return _signatureManager.Validate(
				paymentStatusUpdatedDto.Signature,
				new OnlineOrderPaymentStatusUpdatedSignatureParams
				{
					OnlineOrderId = paymentStatusUpdatedDto.ExternalOrderId.ToString(),
					OnlinePayment = paymentStatusUpdatedDto.OnlinePayment,
					ShopId = (int)paymentStatusUpdatedDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}

		public bool ValidateRequestForCallSignature(CreatingRequestForCallDto creatingInfoDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(creatingInfoDto.Source);

			return _signatureManager.Validate(
				creatingInfoDto.Signature,
				new RequestForCallSignatureParams
				{
					PhoneNumber = creatingInfoDto.PhoneNumber,
					ShopId = (int)creatingInfoDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}

		#endregion

		public async Task<DetailedOrderInfoDto> GetDetailedOrderInfo(
			GetDetailedOrderInfoDto getDetailedOrderInfoDto,
			CancellationToken cancellationToken
			)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();

			var ratingAvailableFrom = _orderSettings.GetDateAvailabilityRatingOrder;
			OrderRating orderRating = null;
			OnlineOrder onlineOrder = null;

			var timers = uow.GetAll<OnlineOrderTimers>().FirstOrDefault();

			if(getDetailedOrderInfoDto.OnlineOrderId.HasValue)
			{
				onlineOrder = uow.GetById<OnlineOrder>(getDetailedOrderInfoDto.OnlineOrderId.Value);
			}

			if(getDetailedOrderInfoDto.OrderId.HasValue)
			{
				var order = uow.GetById<Order>(getDetailedOrderInfoDto.OrderId.Value);

				orderRating = _genericRatingRepository.Get(
						uow,
						x => x.Order.Id == order.Id)
					.FirstOrDefault();

				var driverPositionData = await _courierTrackingService.GetDriverPositionData(uow, order, cancellationToken);
				var isOrderWasSelectedAsNext =
					driverPositionData.EstablishedRoute || await _routeListRepository.IsOrderEverWasSelectedAsNext(uow, order.Id, cancellationToken);

				var driversMangoExtensionNumber =
					await _orderRepository.GetDriversMangoExtensionNumberByOrderId(uow, order.Id, cancellationToken);

				return await _customerOrderFactory.CreateDetailedOrderInfo(
					uow,
					order,
					orderRating,
					timers,
					onlineOrder,
					ratingAvailableFrom,
					driversMangoExtensionNumber,
					driverPositionData.EstablishedRoute,
					isOrderWasSelectedAsNext,
					driverPositionData.CoordinatesLastUpdateTime,
					cancellationToken
					);
			}

			orderRating = _genericRatingRepository.Get(
					uow,
					x => x.OnlineOrder.Id == onlineOrder.Id)
				.FirstOrDefault();

			return await _customerOrderFactory.CreateDetailedOrderInfo(
				uow,
				onlineOrder,
				orderRating,
				timers,
				getDetailedOrderInfoDto.OrderId,
				ratingAvailableFrom,
				cancellationToken
			);
		}

		public async Task<OrdersDto> GetOrders(GetOrdersDto getOrdersDto, CancellationToken cancellationToken)
		{
			var skipElements = (getOrdersDto.Page - 1) * getOrdersDto.OrdersCountOnPage;
			var dateAvailabilityRating = _orderSettings.GetDateAvailabilityRatingOrder;

			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			var allOrders = GetAllCounterpartyOrders(uow, getOrdersDto, dateAvailabilityRating);

			var orderDtos = allOrders
				.OrderByDescending(x => x.DeliveryDate)
				.ThenByDescending(x => x.CreatedDateTimeUtc)
				.Skip(skipElements)
				.Take(getOrdersDto.OrdersCountOnPage)
				.ToArray();

			foreach(var orderDto in orderDtos)
			{
				var driverPositionData = await _courierTrackingService.GetDriverPositionData(uow, orderDto, cancellationToken);
				orderDto.UpdateTrackingAvailability(driverPositionData.EstablishedRoute, driverPositionData.CoordinatesLastUpdateTime, _courierCoordinatesOptions.CurrentValue.TrackingLostTimeout);
			}

			return new OrdersDto
			{
				Orders = orderDtos,
				OrdersCount = allOrders.Length
			};
		}

		public async Task<ActiveOrdersDto> GetCurrentClientOrders(
			GetCounterpartyOrdersDto getCounterpartyOrdersDto,
			CancellationToken cancellationToken = default)
		{
			var skipElements = (getCounterpartyOrdersDto.Page - 1) * getCounterpartyOrdersDto.OrdersCountOnPage;
			var dateAvailabilityRating = _orderSettings.GetDateAvailabilityRatingOrder;

			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис онлайн заказов.Получение текущих активных заказов клиента");

			var activeStatuses = new[]
			{
				ExternalOrderStatus.OrderPerformed,
				ExternalOrderStatus.OrderDelivering
			};

			var activeOrders =
				GetAllCounterpartyOrders(uow, getCounterpartyOrdersDto, dateAvailabilityRating, activeStatuses)
				.OrderByDescending(x => x.DeliveryDate)
				.ThenByDescending(x => x.CreatedDateTimeUtc)
				.Skip(skipElements)
				.Take(getCounterpartyOrdersDto.OrdersCountOnPage)
				.ToArray();

			var activeOrderDtos = new List<ActiveOrderDto>();

			foreach(var orderDto in activeOrders)
			{
				bool establishedRoute = false;
				DateTime? driversCoordinatesLastUpdateTime = null;
				bool isOrderWasSelectedAsNext = false;

				if(orderDto.OrderId.HasValue)
				{
					var order = uow.GetById<Order>(orderDto.OrderId.Value);

					if(order != null)
					{
						var driverPositionData = await _courierTrackingService.GetDriverPositionData(uow, order, cancellationToken);
						establishedRoute = driverPositionData.EstablishedRoute;
						driversCoordinatesLastUpdateTime = driverPositionData.CoordinatesLastUpdateTime;
						isOrderWasSelectedAsNext =
							establishedRoute || await _routeListRepository.IsOrderEverWasSelectedAsNext(uow, order.Id, cancellationToken);
					}
				}

				var activeOrderDto =
					_customerOrderFactory.CreateActiveOrderInfo(
						orderDto,
						establishedRoute,
						isOrderWasSelectedAsNext,
						driversCoordinatesLastUpdateTime);

				activeOrderDtos.Add(activeOrderDto);
			}

			return new ActiveOrdersDto
			{
				Orders = activeOrderDtos.ToArray(),
				OrdersCount = activeOrders.Length
			};
		}

		private OrderDto[] GetAllCounterpartyOrders(
			IUnitOfWork uow,
			GetCounterpartyOrdersDto getCounterpartyOrdersDto,
			DateTime dateAvailabilityRating,
			IEnumerable<ExternalOrderStatus> orderStatuses = null)
		{
			var ordersWithoutOnlineOrders =
				_orderRepository.GetCounterpartyOrdersWithoutOnlineOrdersV6(
					uow, getCounterpartyOrdersDto.CounterpartyErpId, dateAvailabilityRating, orderStatuses);
			var onlineOrdersWithOrders =
				GetOnlineOrdersWithOrdersInfo(getCounterpartyOrdersDto, uow, dateAvailabilityRating, orderStatuses);
			var onlineOrdersWithoutOrders =
				_onlineOrderRepository.GetCounterpartyOnlineOrdersWithoutOrderV6(
					uow, getCounterpartyOrdersDto.CounterpartyErpId, dateAvailabilityRating, orderStatuses);

			return ordersWithoutOnlineOrders
				.Concat(onlineOrdersWithOrders)
				.Concat(onlineOrdersWithoutOrders)
				.ToArray();
		}

		private IEnumerable<OrderDto> GetOnlineOrdersWithOrdersInfo(
			GetCounterpartyOrdersDto getCounterpartyOrdersDto,
			IUnitOfWork uow,
			DateTime dateAvailabilityRating,
			IEnumerable<ExternalOrderStatus> orderStatuses = null)
		{
			var onlineOrdersInfo = new List<OrderDto>();
			var ordersFromOnlineOrders =
				_orderRepository.GetCounterpartyOrdersFromOnlineOrdersV6(uow, getCounterpartyOrdersDto.CounterpartyErpId, dateAvailabilityRating, orderStatuses)
					.ToLookup(x => x.OnlineOrderId);

			foreach(var ordersFromOnlineOrdersGroup in ordersFromOnlineOrders)
			{
				OrderDto onlineOrderInfo;
				if(ordersFromOnlineOrdersGroup.Count() == 1)
				{
					var orderDto = ordersFromOnlineOrdersGroup.First();
					onlineOrderInfo = new OrderDto
					{
						OrderId = orderDto.OrderId,
						OnlineOrderId = orderDto.OnlineOrderId,
						DeliveryDate = orderDto.DeliveryDate,
						CreatedDateTimeUtc = orderDto.CreatedDateTimeUtc,
						DeliveryAddress = orderDto.DeliveryAddress,
						DeliverySchedule = orderDto.DeliverySchedule,
						RatingValue = orderDto.RatingValue,
						IsRatingAvailable = orderDto.IsRatingAvailable,
						IsNeedPay = false,
						DeliveryPointId = orderDto.DeliveryPointId,
						OrderSum = orderDto.OrderSum,
						OrderStatus = orderDto.OrderStatus
					};

					onlineOrdersInfo.Add(onlineOrderInfo);
				}
				else
				{
					foreach(var orderDto in ordersFromOnlineOrdersGroup)
					{
						onlineOrderInfo = new OrderDto
						{
							OrderId = orderDto.OrderId,
							DeliveryDate = orderDto.DeliveryDate,
							CreatedDateTimeUtc = orderDto.CreatedDateTimeUtc,
							DeliveryAddress = orderDto.DeliveryAddress,
							DeliverySchedule = orderDto.DeliverySchedule,
							RatingValue = orderDto.RatingValue,
							IsRatingAvailable = orderDto.IsRatingAvailable,
							IsNeedPay = false,
							DeliveryPointId = orderDto.DeliveryPointId,
							OrderSum = orderDto.OrderSum,
							OrderStatus = orderDto.OrderStatus
						};
						onlineOrdersInfo.Add(onlineOrderInfo);
					}
				}
			}

			return onlineOrdersInfo;
		}

		public IEnumerable<OrderRatingReasonDto> GetOrderRatingReasons()
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			var reasons = uow.GetAll<OrderRatingReason>().ToArray();

			return _customerOrderFactory.GetOrderRatingReasonDtos(reasons);
		}

		public void CreateOrderRating(OrderRatingInfoForCreateDto orderRatingInfo)
		{
			var negativeRating = _orderSettings.GetOrderRatingForMandatoryProcessing;
			var orderRating = OrderRating.Create(
				orderRatingInfo.Source,
				orderRatingInfo.Rating,
				orderRatingInfo.Comment,
				orderRatingInfo.OnlineOrderId,
				orderRatingInfo.OrderId,
				orderRatingInfo.OrderRatingReasonsIds,
				negativeRating);

			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			uow.Save(orderRating);
			uow.Commit();
		}

		public bool TryUpdateOnlineOrderPaymentStatus(OnlineOrderPaymentStatusUpdatedDto paymentStatusUpdatedDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			var onlineOrder = _onlineOrderRepository.GetOnlineOrderByExternalId(uow, paymentStatusUpdatedDto.ExternalOrderId);

			if(onlineOrder is null)
			{
				return false;
			}

			onlineOrder.OnlinePayment = paymentStatusUpdatedDto.OnlinePayment;
			onlineOrder.OnlinePaymentSource = paymentStatusUpdatedDto.OnlinePaymentSource;
			onlineOrder.OnlineOrderPaymentStatus = paymentStatusUpdatedDto.OnlineOrderPaymentStatus;
			uow.Save(onlineOrder);
			uow.Commit();
			return true;
		}

		public void CreateRequestForCall(CreatingRequestForCallDto creatingInfoDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();

			Nomenclature nomenclature = null;
			Counterparty counterparty = null;

			if(creatingInfoDto.NomenclatureErpId.HasValue)
			{
				nomenclature = uow.GetById<Nomenclature>(creatingInfoDto.NomenclatureErpId.Value);
			}

			if(creatingInfoDto.CounterpartyErpId.HasValue)
			{
				counterparty = uow.GetById<Counterparty>(creatingInfoDto.CounterpartyErpId.Value);
			}

			var requestForCall = RequestForCall.Create(
				creatingInfoDto.Source,
				creatingInfoDto.ContactName,
				creatingInfoDto.PhoneNumber,
				nomenclature,
				counterparty
				);

			uow.Save(requestForCall);
			uow.Commit();
		}

		public (int HttpCode, string Message, AvailablePaymentMethods AvailablePayments) GetAvailablePaymentMethods(
			GetAvailablePaymentMethodsDto getAvailablePaymentMethods)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис онлайн заказов. Получение доступных способов оплат");
			var onlineOrder = _onlineOrderRepository.GetOnlineOrderById(uow, getAvailablePaymentMethods.OnlineOrderId);

			var result = _unPaidOnlineOrderHandler.CanChangePaymentType(uow, onlineOrder);

			if(result.IsFailure)
			{
				var firstError = result.Errors.First();
				return (int.Parse(firstError.Code), firstError.Message, null);
			}

			var availablePayments = AvailablePaymentMethods.Create(getAvailablePaymentMethods.Source, onlineOrder.OnlineOrderPaymentType);

			return (200, null, availablePayments);
		}

		public async Task<Result<ChangedOrderDto>> UpdateOrderAsync(ChangingOrderDto changingOrderDto, CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис онлайн заказов. Изменение заказа");

			var orders = _orderRepository.GetOrdersFromOnlineOrder(uow, changingOrderDto.OnlineOrderId ?? 0);
			var onlineOrder = uow.GetAll<OnlineOrder>().FirstOrDefault(x => x.Id == changingOrderDto.OnlineOrderId);
			var deliverySchedule = uow.GetAll<Vodovoz.Domain.Logistic.DeliverySchedule>()
				.FirstOrDefault(x => x.Id == changingOrderDto.DeliveryScheduleId);

			var result = await _unPaidOnlineOrderHandler.TryUpdateOrderAsync(
				uow,
				orders,
				onlineOrder,
				deliverySchedule,
				changingOrderDto.ToUpdateOnlineOrderFromChangeRequest(),
				cancellationToken);

			if(result.IsFailure)
			{
				return Result.Failure<ChangedOrderDto>(result.Errors);
			}

			var changedOrderDto = ChangedOrderDto.Create(changingOrderDto.OnlineOrderId);

			return Result.Success(changedOrderDto);
		}

		private string GetSourceSign(Source source)
		{
			return _signaturesSection.GetValue<string>(source.ToString());
		}
	}
}
