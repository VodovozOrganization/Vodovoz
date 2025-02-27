using DriverApi.Contracts.V6;
using Vodovoz.Domain.Logistic;

namespace DriverAPI.Library.V6.Converters
{
	/// <summary>
	/// Конвертер статуса завершенности маршрутного листа
	/// </summary>
	public class RouteListCompletionStatusConverter
	{
		/// <summary>
		/// Конвертация в статус завершенности маршрутного листа Api
		/// </summary>
		/// <param name="routeListStatus"></param>
		/// <returns></returns>
		/// <exception cref="ConverterException"></exception>
		public RouteListDtoCompletionStatus ConvertToAPIRouteListCompletionStatus(RouteListStatus routeListStatus)
		{
			switch(routeListStatus)
			{
				case RouteListStatus.New:
				case RouteListStatus.Confirmed:
				case RouteListStatus.InLoading:
				case RouteListStatus.EnRoute:
					return RouteListDtoCompletionStatus.Incompleted;
				case RouteListStatus.Delivered:
				case RouteListStatus.OnClosing:
				case RouteListStatus.MileageCheck:
				case RouteListStatus.Closed:
					return RouteListDtoCompletionStatus.Completed;
				default:
					throw new ConverterException(nameof(routeListStatus), routeListStatus, $"Значение {routeListStatus} не поддерживается");
			}
		}
	}
}
