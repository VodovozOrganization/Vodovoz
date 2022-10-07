using DriverAPI.Library.DTOs;
using Vodovoz.Domain.Logistic;

namespace DriverAPI.Library.Converters
{
	public class RouteListStatusConverter
	{
		public RouteListDtoStatus convertToAPIRouteListStatus(RouteListStatus routeListStatus)
		{
			switch (routeListStatus)
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
