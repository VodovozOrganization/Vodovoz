using DriverApi.Contracts.V6;
using Vodovoz.Domain.Logistic;

namespace DriverAPI.Library.V6.Converters
{
	/// <summary>
	/// Конвертер статуса маршрутного листа
	/// </summary>
	public class RouteListStatusConverter
	{
		/// <summary>
		/// Метод конвертации в DTO
		/// </summary>
		/// <param name="routeListStatus">Статус маршрутного листа ДВ</param>
		/// <returns></returns>
		/// <exception cref="ConverterException"></exception>
		public RouteListDtoStatus ConvertToAPIRouteListStatus(RouteListStatus routeListStatus)
		{
			switch(routeListStatus)
			{
				case RouteListStatus.New:
					return RouteListDtoStatus.New;
				case RouteListStatus.Confirmed:
					return RouteListDtoStatus.Confirmed;
				case RouteListStatus.InLoading:
					return RouteListDtoStatus.InLoading;
				case RouteListStatus.EnRoute:
					return RouteListDtoStatus.EnRoute;
				case RouteListStatus.Delivered:
					return RouteListDtoStatus.Delivered;
				case RouteListStatus.OnClosing:
					return RouteListDtoStatus.OnClosing;
				case RouteListStatus.MileageCheck:
					return RouteListDtoStatus.MileageCheck;
				case RouteListStatus.Closed:
					return RouteListDtoStatus.Closed;
				default:
					throw new ConverterException(nameof(routeListStatus), routeListStatus, $"Значение {routeListStatus} не поддерживается");
			}
		}
	}
}
