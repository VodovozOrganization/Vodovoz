using DriverApi.Contracts.V5;
using DriverAPI.Library.Exceptions;
using DriverAPI.Library.V5.Converters;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Errors;

namespace DriverAPI.Library.V5.Services
{
	internal class RouteListService : IRouteListService
	{
		private readonly ILogger<RouteListService> _logger;
		private readonly Vodovoz.Services.Logistics.IRouteListService _routeListService;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly RouteListConverter _routeListConverter;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IUnitOfWork _unitOfWork;

		public RouteListService(ILogger<RouteListService> logger,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			RouteListConverter routeListConverter,
			IEmployeeRepository employeeRepository,
			IUnitOfWork unitOfWork,
			Vodovoz.Services.Logistics.IRouteListService routeListService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_routeListConverter = routeListConverter ?? throw new ArgumentNullException(nameof(routeListConverter));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
		}

		/// <summary>
		/// Получение информации о маошрутном листе в требуемом формате
		/// </summary>
		/// <param name="routeListId">Идентификатор МЛ</param>
		/// <returns>APIRouteList</returns>
		public RouteListDto Get(int routeListId)
		{
			var routeList = _routeListRepository.GetRouteListById(_unitOfWork, routeListId)
				?? throw new DataNotFoundException(nameof(routeListId), $"Маршрутный лист {routeListId} не найден");

			IDictionary<int, string> spectiaConditionsToAccept = new Dictionary<int, string>();

			if(!routeList.SpecialConditionsAccepted)
			{
				spectiaConditionsToAccept = _routeListService.GetSpecialConditionsDictionaryFor(_unitOfWork, routeListId);
			}

			return _routeListConverter.ConvertToAPIRouteList(routeList, _routeListRepository.GetDeliveryItemsToReturn(_unitOfWork, routeListId), spectiaConditionsToAccept);
		}

		/// <summary>
		/// Получение информации о маршрутных листах в требуемом формате
		/// </summary>
		/// <param name="routeListsIds">Список идентификаторов МЛ</param>
		/// <returns>IEnumerable APIRouteList</returns>
		public IEnumerable<RouteListDto> Get(int[] routeListsIds)
		{
			var vodovozRouteLists = _routeListRepository.GetRouteListsByIds(_unitOfWork, routeListsIds);
			var routeLists = new List<RouteListDto>();

			foreach(var routeList in vodovozRouteLists)
			{
				try
				{
					IDictionary<int, string> spectiaConditionsToAccept = new Dictionary<int, string>();

					if(!routeList.SpecialConditionsAccepted)
					{
						spectiaConditionsToAccept = _routeListService.GetSpecialConditionsDictionaryFor(_unitOfWork, routeList.Id);
					}

					routeLists.Add(_routeListConverter.ConvertToAPIRouteList(routeList, _routeListRepository.GetDeliveryItemsToReturn(_unitOfWork, routeList.Id), spectiaConditionsToAccept));
				}
				catch(ConverterException e)
				{
					_logger.LogWarning(e, "Ошибка конвертации маршрутного листа {RouteListId}", routeList.Id);
				}
			}

			return routeLists;
		}

		/// <summary>
		/// Получение списка идентификаторов МЛ для водителя по его Email адресу
		/// </summary>
		/// <param name="login">Android - login</param>
		/// <returns>Список идентификаторов</returns>
		public Result<IEnumerable<int>> GetRouteListsIdsForDriverByAndroidLogin(string login)
		{
			var driver = _employeeRepository.GetEmployeeByAndroidLogin(_unitOfWork, login);

			if(driver is null)
			{
				return Result.Failure<IEnumerable<int>>(Vodovoz.Errors.Employees.Driver.NotFound);
			}

			return Result.Success(_routeListRepository.GetDriverRouteListsIds(
				_unitOfWork,
				driver,
				RouteListStatus.EnRoute));
		}

		public Result RegisterCoordinateForRouteListItem(int routeListAddressId, decimal latitude, decimal longitude, DateTime actionTime, int driverId)
		{
			var routeListAddress = _routeListItemRepository.GetRouteListItemById(_unitOfWork, routeListAddressId);

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ {RouteListItemId} не нейден", routeListAddressId);
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFound);
			}

			if(routeListAddress.Order is null)
			{
				return Result.Failure(Vodovoz.Errors.Orders.Order.NotFound);
			}

			var deliveryPoint = routeListAddress.Order.DeliveryPoint;

			if(deliveryPoint is null)
			{
				return Result.Failure(Vodovoz.Errors.Clients.DeliveryPoint.NotFound);
			}

			if(routeListAddress.RouteList.Driver.Id != driverId)
			{
				_logger.LogWarning("Попытка записи координаты точки доставки {RouteListAddressId} МЛ водителя {DriverId}, сотрудником {EmployeeId}",
					routeListAddressId,
					routeListAddress.RouteList.Driver.Id,
					driverId);

				return Result.Failure(Errors.Security.Authorization.RouteListAccessDenied);
			}

			if(routeListAddress.RouteList.Status != RouteListStatus.EnRoute)
			{
				_logger.LogWarning("Попытка записи координаты точки доставки в МЛ {RouteListId} в статусе {RouteListStatus}" +
					" адреса {RouteListAddressId} в статусе {RouteListAddressStatus} водителем {DriverId}",
					routeListAddress.RouteList.Id,
					routeListAddress.RouteList.Status,
					routeListAddressId,
					routeListAddress.Status,
					driverId);

				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Попытка записи координаты точки доставки в МЛ {RouteListId} в статусе {RouteListStatus}" +
					" адреса {RouteListAddressId} в статусе {RouteListAddressStatus} водителем {DriverId}",
					routeListAddress.RouteList.Id,
					routeListAddress.RouteList.Status,
					routeListAddressId,
					routeListAddress.Status,
					driverId);

				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotEnRouteState);
			}

			var coordinate = new DeliveryPointEstimatedCoordinate()
			{
				DeliveryPointId = deliveryPoint.Id,
				Latitude = latitude,
				Longitude = longitude,
				RegistrationTime = actionTime
			};

			_unitOfWork.Save(coordinate);
			_unitOfWork.Commit();

			return Result.Success();
		}

		public string GetActualDriverPushNotificationsTokenByOrderId(int orderId)
		{
			return _employeeRepository.GetEmployeePushTokenByOrderId(_unitOfWork, orderId);
		}

		public string GetPreActualDriverPushNotificationsTokenByOrderId(int orderId)
		{
			return _employeeRepository.GetPreviousRouteListEmployeePushTokenByOrderId(_unitOfWork, orderId);
		}

		public Result RollbackRouteListAddressStatusEnRoute(int routeListAddressId, int driverId)
		{
			if(routeListAddressId <= 0)
			{
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFound);
			}

			var routeListAddress = _routeListItemRepository.GetRouteListItemById(_unitOfWork, routeListAddressId);

			if(routeListAddress is null)
			{
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFound);
			}

			if(!IsRouteListBelongToDriver(routeListAddress.RouteList.Id, driverId))
			{
				_logger.LogWarning("Попытка вернуть в путь адрес МЛ {RouteListAddressId} сотрудником {EmployeeId}, водитель МЛ: {DriverId}",
					routeListAddressId,
					driverId,
					routeListAddress.RouteList.Driver?.Id);

				return Result.Failure(Errors.Security.Authorization.RouteListAccessDenied);
			}

			if(routeListAddress.RouteList.Status != RouteListStatus.EnRoute)
			{
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.Completed)
			{
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotCompletedState);
			}

			routeListAddress.RouteList.ChangeAddressStatus(_unitOfWork, routeListAddress.Id, RouteListItemStatus.EnRoute);

			_unitOfWork.Save(routeListAddress.RouteList);
			_unitOfWork.Save(routeListAddress);
			_unitOfWork.Commit();

			return Result.Success();
		}

		public bool IsRouteListBelongToDriver(int routeListId, int driverId)
		{
			var routeList = _routeListRepository.GetRouteListById(_unitOfWork, routeListId);

			return routeList?.Driver?.Id == driverId;
		}
	}
}
