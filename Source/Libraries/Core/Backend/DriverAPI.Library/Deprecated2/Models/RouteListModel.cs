using DriverAPI.Library.Deprecated2.DTOs;
using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using RouteListConverter = DriverAPI.Library.Deprecated2.Converters.RouteListConverter;
using ConverterException = DriverAPI.Library.Converters.ConverterException;

namespace DriverAPI.Library.Deprecated2.Models
{
	[Obsolete("Будет удален с прекращением поддержки API v2")]
	public class RouteListModel : IRouteListModel
	{
		private readonly ILogger<RouteListModel> _logger;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly RouteListConverter _routeListConverter;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IUnitOfWork _unitOfWork;

		public RouteListModel(ILogger<RouteListModel> logger,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			RouteListConverter routeListConverter,
			IEmployeeRepository employeeRepository,
			IUnitOfWork unitOfWork)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_routeListConverter = routeListConverter ?? throw new ArgumentNullException(nameof(routeListConverter));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		/// <summary>
		/// Получение информации о маошрутном листе в требуемом формате
		/// </summary>
		/// <param name="routeListId">Идентификатор МЛ</param>
		/// <returns>APIRouteList</returns>
		public RouteListDto Get(int routeListId)
		{
			var routeList = _routeListRepository.GetRouteListById(_unitOfWork, routeListId)
				?? throw new DataNotFoundException(nameof(routeListId), $"Маршрутный лист { routeListId } не найден");

			return _routeListConverter.convertToAPIRouteList(routeList, _routeListRepository.GetDeliveryItemsToReturn(_unitOfWork, routeListId));
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

			foreach (var routelist in vodovozRouteLists)
			{
				try
				{
					routeLists.Add(_routeListConverter.convertToAPIRouteList(routelist, _routeListRepository.GetDeliveryItemsToReturn(_unitOfWork, routelist.Id)));
				}
				catch (ConverterException e)
				{
					_logger.LogWarning(e, "Ошибка конвертации маршрутного листа {RouteListId}", routelist.Id);
				}
			}

			return routeLists;
		}

		/// <summary>
		/// Получение списка идентификаторов МЛ для водителя по его Email адресу
		/// </summary>
		/// <param name="login">Android - login</param>
		/// <returns>Список идентификаторов</returns>
		public IEnumerable<int> GetRouteListsIdsForDriverByAndroidLogin(string login)
		{
			var driver = _employeeRepository.GetDriverByAndroidLogin(_unitOfWork, login)
				?? throw new DataNotFoundException(nameof(login), $"Водитель не найден");

			return _routeListRepository.GetDriverRouteListsIds(
					_unitOfWork,
					driver,
					RouteListStatus.EnRoute
				);
		}

		public void RegisterCoordinateForRouteListItem(int routeListAddressId, decimal latitude, decimal longitude, DateTime actionTime, int driverId)
		{
			var routeListAddress = _routeListItemRepository.GetRouteListItemById(_unitOfWork, routeListAddressId)
				?? throw new ArgumentOutOfRangeException(nameof(routeListAddressId), $"Адрес МЛ {routeListAddressId} не нейден");

			var deliveryPoint = routeListAddress.Order?.DeliveryPoint
				?? throw new DataNotFoundException(nameof(routeListAddressId), $"Точка доставки для адреса не найдена");

			if(routeListAddress.RouteList.Driver.Id != driverId)
			{
				_logger.LogWarning("Попытка записи координаты точки доставки {RouteListAddressId} МЛ водителя {DriverId}, сотрудником {EmployeeId}",
					routeListAddressId,
					routeListAddress.RouteList.Driver.Id,
					driverId);
				throw new AccessViolationException("Нельзя записать координаты точки доставки для МЛ другого водителя");
			}

			if(routeListAddress.RouteList.Status != RouteListStatus.EnRoute
			|| routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Попытка записи координаты точки доставки в МЛ {RouteListId} в статусе {RouteListStatus}" +
					" адреса {RouteListAddressId} в статусе {RouteListAddressStatus} водителем {DriverId}",
					routeListAddress.RouteList.Id,
					routeListAddress.RouteList.Status,
					routeListAddressId,
					routeListAddress.Status,
					driverId);
				throw new AccessViolationException("Нельзя записать координаты точки доставки для этого адреса");
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
		}

		public string GetActualDriverPushNotificationsTokenByOrderId(int orderId)
		{
			return _employeeRepository.GetEmployeePushTokenByOrderId(_unitOfWork, orderId);
		}

		public void RollbackRouteListAddressStatusEnRoute(int routeListAddressId, int driverId)
		{
			if(routeListAddressId <= 0)
			{
				throw new DataNotFoundException(nameof(routeListAddressId), routeListAddressId, "Идентификатор адреса МЛ не может быть меньше или равен нулю");
			}

			var routeListAddress = _routeListItemRepository.GetRouteListItemById(_unitOfWork, routeListAddressId)
				?? throw new DataNotFoundException(nameof(routeListAddressId), routeListAddressId, "Указан идентификатор несуществующего адреса МЛ");

			if(!IsRouteListBelongToDriver(routeListAddress.RouteList.Id, driverId))
			{
				_logger.LogWarning("Попытка вернуть в путь адрес МЛ {RouteListAddressId} сотрудником {EmployeeId}, водитель МЛ: {DriverId}",
					routeListAddressId,
					driverId,
					routeListAddress.RouteList.Driver?.Id);
				throw new AccessViolationException("Нельзя вернуть в путь адрес не вашего МЛ");
			}

			if(routeListAddress.Status != RouteListItemStatus.Completed
			|| routeListAddress.RouteList.Status != RouteListStatus.EnRoute)
			{
				throw new InvalidOperationException("Адрес нельзя вернуть в путь");
			}

			routeListAddress.RouteList.ChangeAddressStatus(_unitOfWork, routeListAddress.Id, RouteListItemStatus.EnRoute);

			_unitOfWork.Save(routeListAddress.RouteList);
			_unitOfWork.Save(routeListAddress);
			_unitOfWork.Commit();
		}

		public bool IsRouteListBelongToDriver(int routeListId, int driverId)
		{
			var routeList = _routeListRepository.GetRouteListById(_unitOfWork, routeListId);

			return routeList?.Driver?.Id == driverId;
		}
	}
}
