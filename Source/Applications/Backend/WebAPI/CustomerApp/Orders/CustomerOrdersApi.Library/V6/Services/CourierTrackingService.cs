using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.V6.Dto.Orders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Errors.Orders;

namespace CustomerOrdersApi.Library.V6.Services
{
	public class CourierTrackingService : ICourierTrackingService
	{
		private static readonly OrderStatus[] _orderCompleteStatuses = new OrderStatus[]
		{
			OrderStatus.Shipped,
			OrderStatus.UnloadingOnStock,
			OrderStatus.Closed
		};

		private readonly ILogger<CourierTrackingService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IOptionsMonitor<CourierCoordinatesOptions> _courierCoordinatesOptions;

		public CourierTrackingService(
			ILogger<CourierTrackingService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			IOnlineOrderRepository onlineOrderRepository,
			IRouteListRepository routeListRepository,
			IOptionsMonitor<CourierCoordinatesOptions> courierCoordinatesOptions
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_courierCoordinatesOptions = courierCoordinatesOptions ?? throw new ArgumentNullException(nameof(courierCoordinatesOptions));
		}

		public async Task<Result<CourierCoordinatesDto>> GetCourierCoordinates(
			GetCourierCoordinatesDto getCourierCoordinatesDto,
			CancellationToken cancellationToken = default)
		{
			if(getCourierCoordinatesDto is null)
			{
				throw new ArgumentNullException(nameof(getCourierCoordinatesDto));
			}

			if(getCourierCoordinatesDto.OrderId is null && getCourierCoordinatesDto.OnlineOrderId is null)
			{
				_logger.LogError("Не передан идентификатор заказа или онлайн-заказа для получения координат курьера");
				return Result.Failure<CourierCoordinatesDto>(OnlineOrderErrors.IncorrectOrdersData);
			}

			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис онлайн заказов. Получение координат курьера");

			Order order;

			if(getCourierCoordinatesDto.OnlineOrderId != null)
			{
				var onlineOrder = _onlineOrderRepository.GetOnlineOrderById(uow, getCourierCoordinatesDto.OnlineOrderId.Value);

				if(onlineOrder is null)
				{
					_logger.LogError(
						"Онлайн-заказ с идентификатором {OnlineOrderId} не найден",
						getCourierCoordinatesDto.OnlineOrderId);

					return Result.Failure<CourierCoordinatesDto>(OnlineOrderErrors.OnlineOrderNotFound);
				}

				var orders = _orderRepository.GetOrdersFromOnlineOrder(uow, onlineOrder.Id);

				if(!orders.Any())
				{
					_logger.LogError(
						"Не найдено ни одного заказа, связанного с онлайн-заказом с идентификатором {OnlineOrderId}",
						getCourierCoordinatesDto.OnlineOrderId);

					return Result.Failure<CourierCoordinatesDto>(OnlineOrderErrors.ErpOrderForOnlineOrderNotFound);
				}

				if(orders.Count() > 1 && getCourierCoordinatesDto.OrderId is null)
				{
					_logger.LogError(
						"Онлайн-заказ с идентификатором {OnlineOrderId} связан с несколькими заказами, но не был передан идентификатор конкретного заказа",
						getCourierCoordinatesDto.OnlineOrderId);

					return Result.Failure<CourierCoordinatesDto>(OnlineOrderErrors.OnlineOrderHasManyErpOrders);
				}

				if(getCourierCoordinatesDto.OrderId != null
					&& !orders.Any(x => x.Id == getCourierCoordinatesDto.OrderId))
				{
					_logger.LogError(
						"Онлайн-заказ с идентификатором {OnlineOrderId} не связан с заказом с идентификатором {OrderId}",
						getCourierCoordinatesDto.OnlineOrderId,
						getCourierCoordinatesDto.OrderId);

					return Result.Failure<CourierCoordinatesDto>(OnlineOrderErrors.ErpOrderNotRelatedToOnlineOrder);
				}

				order =
					getCourierCoordinatesDto.OrderId is null
					? orders.First()
					: orders.First(x => x.Id == getCourierCoordinatesDto.OrderId);
			}
			else
			{
				order = uow.GetById<Order>(getCourierCoordinatesDto.OrderId.Value);

				if(order is null)
				{
					_logger.LogError(
						"Заказ с идентификатором {OrderId} не найден",
						getCourierCoordinatesDto.OrderId);

					return Result.Failure<CourierCoordinatesDto>(OrderErrors.NotFound);
				}
			}

			if(order.Client.Id != getCourierCoordinatesDto.CounterpartyErpId)
			{
				_logger.LogError(
					"Заказ с идентификатором {OrderId} не принадлежит контрагенту с идентификатором {CounterpartyErpId}",
					order.Id,
					getCourierCoordinatesDto.CounterpartyErpId);

				return Result.Failure<CourierCoordinatesDto>(OrderErrors.OrderDoesNotBelongToCounterparty);
			}

			if(order.SelfDelivery)
			{
				_logger.LogError(
					"Заказ с идентификатором {OrderId} является самовывозом, координаты курьера недоступны",
					order.Id);

				return Result.Failure<CourierCoordinatesDto>(OnlineOrderErrors.CourierCoordinatesUnavailableSelfDeliveryOrders);
			}

			var isOrderComplete = _orderCompleteStatuses.Contains(order.OrderStatus);

			if(order.OrderStatus != OrderStatus.OnTheWay && !isOrderComplete)
			{
				_logger.LogError(
					"Заказ с идентификатором {OrderId} имеет недопустимый статус {OrderStatus} для получения координат курьера",
					order.Id,
					order.OrderStatus);

				return Result.Failure<CourierCoordinatesDto>(OnlineOrderErrors.OrderHasInvalidStatusForCourierCoordinates);
			}

			var courierCoordinatesDto = new CourierCoordinatesDto
			{
				TimeForRefresh = _courierCoordinatesOptions.CurrentValue.TimeForRefreshInMobileApp,
				ClientCoordinates = GetClientCoordinates(order)
			};

			if(isOrderComplete)
			{
				courierCoordinatesDto.TrackingStatus = CourierTrackingStatusTypeDto.Complete;
				return Result.Success(courierCoordinatesDto);
			}

			var driverPositionData =
				await GetDriverPositionData(uow, order, cancellationToken);

			if(!driverPositionData.EstablishedRoute)
			{
				_logger.LogError(
					"Заказ с идентификатором {OrderId} не имеет установленного маршрута",
					order.Id);

				return Result.Failure<CourierCoordinatesDto>(OnlineOrderErrors.OrderHasNoEstablishedRoute);
			}

			if(driverPositionData.CourierCoordinate is null || driverPositionData.CoordinatesLastUpdateTime is null)
			{
				_logger.LogError(
					"Заказ с идентификатором {OrderId} не имеет актуальных координат курьера",
					order.Id);

				return Result.Failure<CourierCoordinatesDto>(OnlineOrderErrors.CourierCoordinatesAreMissing);
			}

			var isTrackingLost =
				DateTime.Now - driverPositionData.CoordinatesLastUpdateTime.Value > _courierCoordinatesOptions.CurrentValue.TrackingLostTimeout;

			courierCoordinatesDto.TrackingStatus =
				isTrackingLost
				? CourierTrackingStatusTypeDto.Lost
				: CourierTrackingStatusTypeDto.Active;

			courierCoordinatesDto.CourierCoordinate = driverPositionData.CourierCoordinate;

			return Result.Success(courierCoordinatesDto);
		}

		public async Task<ICourierTrackingService.DriverPositionData> GetDriverPositionData(
			IUnitOfWork uow,
			Order order,
			CancellationToken cancellationToken = default)
		{
			var (establishedRoute, routeListId, selectedAt) = await GetEstablishedRoute(uow, order, cancellationToken);

			if(!establishedRoute)
			{
				return new ICourierTrackingService.DriverPositionData
				{
					EstablishedRoute = false,
					CourierCoordinate = null,
					CoordinatesLastUpdateTime = null
				};
			}

			(CoordinatesDto courierCoordinate, DateTime? coordinatesLastUpdateTime) =
				await GetDriverLastCoordinate(uow, routeListId, selectedAt, cancellationToken);

			return new ICourierTrackingService.DriverPositionData
			{
				EstablishedRoute = true,
				CourierCoordinate = courierCoordinate,
				CoordinatesLastUpdateTime = coordinatesLastUpdateTime
			};
		}

		public async Task<ICourierTrackingService.DriverPositionData> GetDriverPositionData(
			IUnitOfWork uow,
			OrderDto order,
			CancellationToken cancellationToken = default)
		{
			var (establishedRoute, routeListId, selectedAt) = await GetEstablishedRoute(uow, order, cancellationToken);

			if(!establishedRoute)
			{
				return new ICourierTrackingService.DriverPositionData
				{
					EstablishedRoute = false,
					CourierCoordinate = null,
					CoordinatesLastUpdateTime = null
				};
			}

			(CoordinatesDto courierCoordinate, DateTime? coordinatesLastUpdateTime) =
				await GetDriverLastCoordinate(uow, routeListId, selectedAt, cancellationToken);

			return new ICourierTrackingService.DriverPositionData
			{
				EstablishedRoute = true,
				CourierCoordinate = courierCoordinate,
				CoordinatesLastUpdateTime = coordinatesLastUpdateTime
			};
		}

		private async Task<(CoordinatesDto courierCoordinate, DateTime? coordinatesLastUpdateTime)> GetDriverLastCoordinate(IUnitOfWork uow, int? routeListId, DateTime? selectedAt, CancellationToken cancellationToken)
		{
			var trackPoint = await _routeListRepository.GetDriverLastCoordinate(uow, routeListId.Value, selectedAt.Value, cancellationToken);

			CoordinatesDto courierCoordinate = null;

			if(trackPoint != null)
			{
				courierCoordinate = new CoordinatesDto
				{
					Latitude = trackPoint.Latitude,
					Longitude = trackPoint.Longitude,
				};
			}

			var coordinatesLastUpdateTime = trackPoint?.ReceiveTimeStamp;
			return (courierCoordinate, coordinatesLastUpdateTime);
		}

		private async Task<(bool EstablishedRoute, int? RouteListId, DateTime? SelectedAt)> GetEstablishedRoute(
			IUnitOfWork uow,
			Order order,
			CancellationToken cancellationToken = default)
		{
			if(order.SelfDelivery || order.OrderStatus != OrderStatus.OnTheWay)
			{
				return (false, default, default);
			}

			return await GetEstablishedRoute(uow, order.Id, cancellationToken);
		}

		private async Task<(bool EstablishedRoute, int? RouteListId, DateTime? SelectedAt)> GetEstablishedRoute(
			IUnitOfWork uow,
			OrderDto order,
			CancellationToken cancellationToken = default)
		{
			if(order.OrderId is null
				|| order.IsSelfDelivery
				|| order.OrderStatus != ExternalOrderStatus.OrderDelivering)
			{
				return (false, default, default);
			}

			return await GetEstablishedRoute(uow, order.OrderId.Value, cancellationToken);
		}

		private async Task<(bool EstablishedRoute, int? RouteListId, DateTime? SelectedAt)> GetEstablishedRoute(
			IUnitOfWork uow,
			int orderId,
			CancellationToken cancellationToken = default)
		{
			var routeListItem =
				await _routeListRepository.GetEnRouteRouteListItemByOrderId(uow, orderId, cancellationToken);

			if(routeListItem == null)
			{
				return (false, default, default);
			}

			var routeListId = routeListItem.RouteList.Id;
			var driverId = routeListItem.RouteList.Driver.Id;

			var selectedAddress =
				await _routeListRepository.GetLastSelectedAddressForRouteList(uow, driverId, routeListId, cancellationToken);

			if(selectedAddress == null || selectedAddress.NextAddressId != routeListItem.Id)
			{
				return (false, default, default);
			}

			return (true, routeListId, selectedAddress.SelectedAt);
		}

		private CoordinatesDto GetClientCoordinates(Order order)
		{
			var deliveryPoint = order.DeliveryPoint;

			return deliveryPoint?.Latitude == null || deliveryPoint.Longitude == null
				? null
				: new CoordinatesDto
				{
					Latitude = (double)deliveryPoint.Latitude.Value,
					Longitude = (double)deliveryPoint.Longitude.Value
				};
		}
	}
}
