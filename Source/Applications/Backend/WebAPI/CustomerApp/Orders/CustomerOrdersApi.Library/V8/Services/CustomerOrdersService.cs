using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.V8.Dto.Orders;
using CustomerOrdersApi.Library.V8.Extensions;
using CustomerOrdersApi.Library.V8.Factories;
using CustomerOrdersApi.Library.V8.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mindbox.Library.Services;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Mindbox;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Database.Mindbox;
using VodovozBusiness.Services.Orders;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V8.Services
{
	public class CustomerOrdersService : SignatureService, ICustomerOrdersService
	{
		private readonly ILogger<CustomerOrdersService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISignatureManager _signatureManager;
		private readonly ICustomerOrderFactory _customerOrderFactory;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly ICustomerOrderRepository _customerOrderRepository;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IGenericRepository<OrderRating> _genericRatingRepository;
		private readonly IUnPaidOnlineOrderHandler _unPaidOnlineOrderHandler;
		private readonly ICourierTrackingService _courierTrackingService;
		private readonly IOptionsSnapshot<SignatureOptions> _signatureOptions;
		private readonly IOptionsMonitor<CourierCoordinatesOptions> _courierCoordinatesOptions;
		private readonly IMindboxOrderBonusesService _mindboxOrderBonusesService;
		private readonly IMindboxBalanceService _mindboxBalanceService;
		private readonly IMindboxSettings _mindboxSettings;

		public CustomerOrdersService(
			ILogger<CustomerOrdersService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISignatureManager signatureManager,
			ICustomerOrderFactory customerOrderFactory,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			ICustomerOrderRepository customerOrderRepository,
			IOnlineOrderRepository onlineOrderRepository,
			IRouteListRepository routeListRepository,
			IGenericRepository<OrderRating> genericRatingRepository,
			IUnPaidOnlineOrderHandler unPaidOnlineOrderHandler,
			ICourierTrackingService courierTrackingService,
			IOptionsSnapshot<SignatureOptions> signatureOptions,
			IOptionsMonitor<CourierCoordinatesOptions> courierCoordinatesOptions,
			IMindboxOrderBonusesService mindboxOrderBonusesService,
			IMindboxBalanceService mindboxBalanceService,
			IMindboxSettings mindboxSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_customerOrderFactory = customerOrderFactory ?? throw new ArgumentNullException(nameof(customerOrderFactory));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_customerOrderRepository = customerOrderRepository ?? throw new ArgumentNullException(nameof(customerOrderRepository));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_genericRatingRepository = genericRatingRepository ?? throw new ArgumentNullException(nameof(genericRatingRepository));
			_unPaidOnlineOrderHandler = unPaidOnlineOrderHandler ?? throw new ArgumentNullException(nameof(unPaidOnlineOrderHandler));
			_courierTrackingService = courierTrackingService ?? throw new ArgumentNullException(nameof(courierTrackingService));
			_signatureOptions = signatureOptions ?? throw new ArgumentNullException(nameof(signatureOptions));
			_courierCoordinatesOptions = courierCoordinatesOptions ?? throw new ArgumentNullException(nameof(courierCoordinatesOptions));
			_mindboxOrderBonusesService = mindboxOrderBonusesService ?? throw new ArgumentNullException(nameof(mindboxOrderBonusesService));
			_mindboxBalanceService = mindboxBalanceService ?? throw new ArgumentNullException(nameof(mindboxBalanceService));
			_mindboxSettings = mindboxSettings ?? throw new ArgumentNullException(nameof(mindboxSettings));
		}

		#region ValidateSignature

		public bool ValidateOrderSignature(ICreatingOnlineOrder creatingOnlineOrder, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(creatingOnlineOrder.Source, _signatureOptions.Value);

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
			var sourceSign = GetSourceSign(orderRatingInfo.Source, _signatureOptions.Value);

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
			var sourceSign = GetSourceSign(getDetailedOrderInfoDto.Source, _signatureOptions.Value);

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
			var sourceSign = GetSourceSign(getOrdersDto.Source, _signatureOptions.Value);

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

		/// <inheritdoc />
		public bool ValidateProfileInfoSignature(GetProfileInfoDto getProfileInfoDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(getProfileInfoDto.Source, _signatureOptions.Value);

			return _signatureManager.Validate(
				getProfileInfoDto.Signature,
				new ProfileInfoSignatureParams
				{
					CounterpartyId = getProfileInfoDto.ErpCounterpartyId.ToString(),
					ShopId = (int)getProfileInfoDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}

		/// <inheritdoc />
		public async Task<(int HttpCode, string Message, ProfileInfoDto Profile)> GetProfileInfo(
			GetProfileInfoDto getProfileInfoDto,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Получение профиля клиента");

			var counterparty = uow.GetById<Counterparty>(getProfileInfoDto.ErpCounterpartyId);

			if(counterparty is null)
			{
				return (400, "Контрагент не найден", null);
			}

			var externalCounterpartyBelongs = uow.Session.Query<ExternalCounterparty>()
				.Any(external =>
					external.ExternalCounterpartyId == getProfileInfoDto.ExternalCounterpartyId
					&& !external.IsArchive
					&& external.Phone != null
					&& external.Phone.Counterparty != null
					&& external.Phone.Counterparty.Id == getProfileInfoDto.ErpCounterpartyId);

			if(!externalCounterpartyBelongs)
			{
				return (400, "Контрагент не найден", null);
			}

			if(getProfileInfoDto.Source != Source.MobileApp || !_mindboxSettings.IsEnabled)
			{
				return (200, null, new ProfileInfoDto());
			}

			var state = uow.GetById<MindboxCounterpartyState>(getProfileInfoDto.ErpCounterpartyId);

			if(state?.Status != MindboxCounterpartyOutboxStatus.Sent)
			{
				return (200, null, new ProfileInfoDto { Bonuses = ProfileBonusesDto.Zero() });
			}

			var balance = await _mindboxBalanceService
				.GetAsync(getProfileInfoDto.ErpCounterpartyId, cancellationToken)
				.ConfigureAwait(false);

			if(!balance.IsAvailable)
			{
				return (200, null, new ProfileInfoDto { Bonuses = ProfileBonusesDto.Unavailable() });
			}

			return (200, null, new ProfileInfoDto
			{
				Bonuses = new ProfileBonusesDto
				{
					IsAvailable = true,
					Balance = balance.Balance,
					Pending = balance.Pending,
					Expiring = balance.ExpiringAmount.HasValue && balance.ExpiringDate.HasValue
						? new ProfileBonusesExpiringDto
						{
							Amount = balance.ExpiringAmount.Value,
							Date = balance.ExpiringDate.Value
						}
						: null
				}
			});
		}

		public bool ValidateOnlineOrderPaymentStatusUpdatedSignature(
			OnlineOrderPaymentStatusUpdatedDto paymentStatusUpdatedDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(paymentStatusUpdatedDto.Source, _signatureOptions.Value);

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
			var sourceSign = GetSourceSign(creatingInfoDto.Source, _signatureOptions.Value);

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

				if(onlineOrder is null)
				{
					onlineOrder = order?.OnlineOrder;
				}

				orderRating = _genericRatingRepository.Get(
						uow,
						x => x.Order.Id == order.Id)
					.FirstOrDefault();

				var driverPositionData = await _courierTrackingService.GetDriverPositionData(uow, order, cancellationToken);
				var isOrderWasSelectedAsNext =
					driverPositionData.EstablishedRoute || await _routeListRepository.IsOrderEverWasSelectedAsNext(uow, order.Id, cancellationToken);

				var orderInfo = await _customerOrderFactory.CreateDetailedOrderInfo(
					uow,
					order,
					orderRating,
					timers,
					onlineOrder,
					ratingAvailableFrom,
					driverPositionData.EstablishedRoute,
					isOrderWasSelectedAsNext,
					driverPositionData.CoordinatesLastUpdateTime,
					cancellationToken
					);

				await FillOrderBonusesAsync(
					orderInfo,
					onlineOrder,
					GetOrderLineIds(onlineOrder, order),
					cancellationToken);

				return orderInfo;
			}

			orderRating = _genericRatingRepository.Get(
					uow,
					x => x.OnlineOrder.Id == onlineOrder.Id)
				.FirstOrDefault();

			var onlineOrderInfo = await _customerOrderFactory.CreateDetailedOrderInfo(
				uow,
				onlineOrder,
				orderRating,
				timers,
				getDetailedOrderInfoDto.OrderId,
				ratingAvailableFrom,
				cancellationToken
			);

			await FillOrderBonusesAsync(onlineOrderInfo, onlineOrder, null, cancellationToken);

			return onlineOrderInfo;
		}

		private static IReadOnlyCollection<string> GetOrderLineIds(OnlineOrder onlineOrder, Order order)
		{
			if(onlineOrder is null || order is null || string.IsNullOrWhiteSpace(onlineOrder.MindboxSnapshot))
			{
				return null;
			}

			MindboxOnlineOrderState snapshot;

			try
			{
				snapshot = JsonSerializer.Deserialize<MindboxOnlineOrderState>(onlineOrder.MindboxSnapshot);
			}
			catch(JsonException)
			{
				return null;
			}

			if(snapshot?.Lines is null || order.OrderItems is null)
			{
				return null;
			}

			var result = new HashSet<string>(StringComparer.Ordinal);

			foreach(var orderItem in order.OrderItems)
			{
				var addedLineId = $"o{orderItem.Id}";

				foreach(var line in snapshot.Lines)
				{
					if(string.Equals(line.LineId, addedLineId, StringComparison.Ordinal)
						|| (orderItem.Nomenclature != null
							&& string.Equals(line.ProductId, $"product_{orderItem.Nomenclature.Id}", StringComparison.Ordinal))
						|| (orderItem.PromoSet != null
							&& string.Equals(line.ProductId, $"promoset_{orderItem.PromoSet.Id}", StringComparison.Ordinal)))
					{
						result.Add(line.LineId);
					}
				}
			}

			return result.Count == 0 ? null : result;
		}

		private async Task FillOrderBonusesAsync(
			DetailedOrderInfoDto orderInfo,
			OnlineOrder onlineOrder,
			IReadOnlyCollection<string> lineIds,
			CancellationToken cancellationToken)
		{
			if(orderInfo is null || onlineOrder is null)
			{
				return;
			}

			var bonuses = await _mindboxOrderBonusesService
				.GetAsync(onlineOrder, lineIds, cancellationToken)
				.ConfigureAwait(false);

			if(bonuses is null)
			{
				orderInfo.OrderBonuses = null;
				return;
			}

			orderInfo.OrderBonuses = new OrderBonusesDto
			{
				IsAvailable = bonuses.IsAvailable,
				WrittenOff = bonuses.WrittenOff,
				Earned = bonuses.Earned,
				EarnStatus = bonuses.EarnStatus.HasValue
					? (OrderBonusesEarnStatus)(int)bonuses.EarnStatus.Value
					: (OrderBonusesEarnStatus?)null
			};
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
				orderDto.UpdateTrackingAvailability(
					driverPositionData.EstablishedRoute,
					driverPositionData.CoordinatesLastUpdateTime,
					_courierCoordinatesOptions.CurrentValue.TrackingLostTimeout);
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
				_customerOrderRepository.GetCounterpartyOrdersWithoutOnlineOrders(
					uow, getCounterpartyOrdersDto.CounterpartyErpId, dateAvailabilityRating, orderStatuses);
			var onlineOrdersWithOrders =
				GetOnlineOrdersWithOrdersInfo(getCounterpartyOrdersDto, uow, dateAvailabilityRating, orderStatuses);
			var onlineOrdersWithoutOrders =
				_customerOrderRepository.GetCounterpartyOnlineOrdersWithoutOrder(
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
				_customerOrderRepository.GetCounterpartyOrdersFromOnlineOrders(
						uow, getCounterpartyOrdersDto.CounterpartyErpId, dateAvailabilityRating, orderStatuses)
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

			var requestForCall = creatingInfoDto.ToRequestForCall(nomenclature, counterparty);

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
	}
}
