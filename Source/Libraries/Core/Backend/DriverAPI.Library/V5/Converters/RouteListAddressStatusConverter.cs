using DriverApi.Contracts.V5;
using Vodovoz.Domain.Logistic;

namespace DriverAPI.Library.V5.Converters
{
	/// <summary>
	/// Конвертер статуса адреса маршрутного листа
	/// </summary>
	public class RouteListAddressStatusConverter
	{
		/// <summary>
		/// Метод конвертации в статус Api
		/// </summary>
		/// <param name="routeListItemStatus">Статус адреса маршрутного листа ДВ</param>
		/// <returns></returns>
		/// <exception cref="ConverterException"></exception>
		public RouteListAddressDtoStatus ConvertToAPIRouteListAddressStatus(RouteListItemStatus routeListItemStatus)
		{
			switch(routeListItemStatus)
			{
				case RouteListItemStatus.EnRoute:
					return RouteListAddressDtoStatus.EnRoute;
				case RouteListItemStatus.Completed:
					return RouteListAddressDtoStatus.Completed;
				case RouteListItemStatus.Canceled:
					return RouteListAddressDtoStatus.Canceled;
				case RouteListItemStatus.Overdue:
					return RouteListAddressDtoStatus.Overdue;
				case RouteListItemStatus.Transfered:
					return RouteListAddressDtoStatus.Transfered;
				default:
					throw new ConverterException(nameof(routeListItemStatus), routeListItemStatus, $"Значение {routeListItemStatus} не поддерживается");
			}
		}
	}
}
