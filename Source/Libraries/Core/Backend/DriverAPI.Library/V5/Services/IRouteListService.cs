using DriverApi.Contracts.V5;
using System;
using System.Collections.Generic;
using Vodovoz.Errors;

namespace DriverAPI.Library.V5.Services
{
	public interface IRouteListService
	{
		RouteListDto Get(int routeListId);
		IEnumerable<RouteListDto> Get(int[] routeListsIds);
		Result<IEnumerable<int>> GetRouteListsIdsForDriverByAndroidLogin(string login);
		string GetActualDriverPushNotificationsTokenByOrderId(int orderId);
		Result RollbackRouteListAddressStatusEnRoute(int routeListAddressId, int driverId);
		bool IsRouteListBelongToDriver(int routeListId, int driverId);
		Result RegisterCoordinateForRouteListItem(int routeListAddressId, decimal latitude, decimal longitude, DateTime actionTime, int driverId);
		string GetPreActualDriverPushNotificationsTokenByOrderId(int orderId);
	}
}
