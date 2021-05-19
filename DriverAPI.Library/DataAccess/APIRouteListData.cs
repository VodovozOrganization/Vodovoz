using DriverAPI.Library.Converters;
using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;

namespace DriverAPI.Library.DataAccess
{
	public class APIRouteListData : IAPIRouteListData
	{
		private readonly ILogger<APIRouteListData> logger;
		private readonly IRouteListRepository routeListRepository;
		private readonly IRouteListItemRepository routeListItemRepository;
		private readonly RouteListConverter routeListConverter;
		private readonly IEmployeeRepository employeeRepository;
		private readonly IUnitOfWork unitOfWork;

		public APIRouteListData(ILogger<APIRouteListData> logger,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			RouteListConverter routeListConverter,
			IEmployeeRepository employeeRepository,
			IUnitOfWork unitOfWork)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			this.routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			this.routeListConverter = routeListConverter ?? throw new ArgumentNullException(nameof(routeListConverter));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		/// <summary>
		/// Получение информации о маошрутном листе в требуемом формате
		/// </summary>
		/// <param name="routeListId">Идентификатор МЛ</param>
		/// <returns>APIRouteList</returns>
		public APIRouteList Get(int routeListId)
		{
			var routeList = routeListRepository.GetRouteList(unitOfWork, routeListId)
				?? throw new DataNotFoundException(nameof(routeListId), $"Маршрутный лист {routeListId} не найден");

			return routeListConverter.convertToAPIRouteList(routeList);
		}

		/// <summary>
		/// Получение информации о маршрутных листах в требуемом формате
		/// </summary>
		/// <param name="routeListsIds">Список идентификаторов МЛ</param>
		/// <returns>IEnumerable APIRouteList</returns>
		public IEnumerable<APIRouteList> Get(int[] routeListsIds)
		{
			var vodovozRouteLists = routeListRepository.GetRouteLists(unitOfWork, routeListsIds);
			var routeLists = new List<APIRouteList>();

			foreach (var routelist in vodovozRouteLists)
			{
				try
				{
					routeLists.Add(routeListConverter.convertToAPIRouteList(routelist));
				}
				catch (ConverterException e)
				{
					logger.LogWarning(e, $"Ошибка конвертирования маршрутного листа {routelist.Id}");
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
			var driver = employeeRepository.GetDriverByAndroidLogin(unitOfWork, login)
				?? throw new DataNotFoundException(nameof(login), $"Водитель не найден");

			return routeListRepository.GetDriverRouteListsIds(
					unitOfWork,
					driver,
					Vodovoz.Domain.Logistic.RouteListStatus.EnRoute
				);
		}

		public void RegisterCoordinateForRouteListItem(int routeListAddressId, decimal latitude, decimal longitude, DateTime actionTime)
		{
			var deliveryPoint = routeListItemRepository.GetRouteListItemById(unitOfWork, routeListAddressId)?.Order?.DeliveryPoint
				?? throw new DataNotFoundException(nameof(routeListAddressId), $"Точка доставки для адреса не найдена");

			var coordinate = new DeliveryPointEstimatedCoordinate()
			{
				DeliveryPointId = deliveryPoint.Id,
				Latitude = latitude,
				Longitude = longitude,
				RegistrationTime = actionTime
			};

			deliveryPoint.DeliveryPointEstimatedCoordinates.Add(coordinate);

			unitOfWork.Save(coordinate);
			unitOfWork.Save(deliveryPoint);
			unitOfWork.Commit();
		}

		public string GetActualDriverPushNotificationsTokenByOrderId(int orderId)
		{
			var fcmToken = routeListRepository.GetRouteListByOrderId(unitOfWork, orderId).Driver?.AndroidToken
				?? throw new DataNotFoundException(nameof(orderId), $"Не найден токен для PUSH-сообщения водителя заказа {orderId}");

			return fcmToken;
		}
	}
}
