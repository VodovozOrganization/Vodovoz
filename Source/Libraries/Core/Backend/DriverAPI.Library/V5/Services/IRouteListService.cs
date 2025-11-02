using DriverApi.Contracts.V5;
using DriverApi.Contracts.V5.Responses;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;

namespace DriverAPI.Library.V5.Services
{
	/// <summary>
	/// Сервис Маршрутных листов DriverApi
	/// </summary>
	public interface IRouteListService
	{
		/// <summary>
		/// Получение информации о маошрутном листе в требуемом формате
		/// </summary>
		/// <param name="routeListId">Идентификатор МЛ</param>
		/// <returns>Информация о маршрутном листе <see cref="RouteListDto"/></returns>
		RouteListDto GetRouteList(int routeListId);

		/// <summary>
		/// Получение информации о маршрутных листах в требуемом формате
		/// </summary>
		/// <param name="routeListsIds">Список идентификаторов МЛ</param>
		/// <returns>Список информаций о маршрутных листах <see cref="IEnumerable{T}"/> <see cref="RouteListDto"/></returns>
		IEnumerable<RouteListDto> GetRouteLists(int[] routeListsIds);

		/// <summary>
		/// Получение списка идентификаторов МЛ для водителя по его Email адресу
		/// </summary>
		/// <param name="login">Android - login</param>
		/// <returns>Список идентификаторов <see cref="IEnumerable{T}"/> <see cref="int"/></returns>
		Result<IEnumerable<int>> GetRouteListsIdsForDriverByAndroidLogin(string login);

		/// <summary>
		/// Получение актуального токена водителя в Firebase по идентификатору заказа
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>Токен <see cref="string"/></returns>
		string GetActualDriverPushNotificationsTokenByOrderId(int orderId);

		/// <summary>
		/// Возвращение адреса маршрутного листа в путь
		/// </summary>
		/// <param name="routeListAddressId">Идентификатор адреса маршрутного листа</param>
		/// <param name="driverId">Идентификатор водителя, инициировавшего возвращение в путь адреса маршрутного листа</param>
		/// <returns><see cref="Result"/></returns>
		Result RollbackRouteListAddressStatusEnRoute(int routeListAddressId, int driverId);

		/// <summary>
		/// Проверка принадлежности маршрутного листа водителю
		/// </summary>
		/// <param name="routeListId">Идентификатор маршрутного листа</param>
		/// <param name="driverId">Идентификатор водителя</param>
		/// <returns>Принадлежит ли маршрутный лист водителю <see cref="bool"/></returns>
		bool IsRouteListBelongToDriver(int routeListId, int driverId);

		/// <summary>
		/// Регистрация предполагаемых координат адресу маршрутного листа
		/// </summary>
		/// <param name="routeListAddressId">Идентификатор адреса маршрутного листа</param>
		/// <param name="latitude">Широта</param>
		/// <param name="longitude">Долгота</param>
		/// <param name="actionTime">Время определения координат на стороне клиента Api</param>
		/// <param name="driverId">Идентификатор водителя</param>
		/// <returns><see cref="Result"/></returns>
		Result RegisterCoordinateForRouteListItem(int routeListAddressId, decimal latitude, decimal longitude, DateTime actionTime, int driverId);

		/// <summary>
		/// Получение переносов адрресов маршрутного листа для водителя
		/// </summary>
		/// <param name="driverId"></param>
		/// <returns>идентификатор сотрудника-водителя</returns>
		Result<DriverTransfersInfoResponse> GetDriverDriverTransfers(int driverId);

		/// <summary>
		/// Получение информации о входящем переносе
		/// </summary>
		/// <param name="routeListAddressId">идентификатор адреса маршрутного листа</param>
		/// <returns></returns>
		Result<RouteListAddressIncomingTransferDto> GetIncomingTransferInfo(int routeListAddressId);

		/// <summary>
		/// Получение информации о исходящем переносе
		/// </summary>
		/// <param name="routeListAddressId">идентификатор адреса маршрутного листа</param>
		/// <returns></returns>
		Result<RouteListAddressOutgoingTransferDto> GetOutgoingTransferInfo(int routeListAddressId);
	}
}
