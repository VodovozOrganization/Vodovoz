using System.Collections.Generic;

namespace DriverApi.Contracts.V5.Responses
{
	/// <summary>
	/// Детализированная информация по маршрутным листам
	/// </summary>
	public class GetRouteListsDetailsResponse
	{
		/// <summary>
		/// Маршрутные листы
		/// </summary>
		public IEnumerable<RouteListDto> RouteLists { get; set; }

		/// <summary>
		/// Заказы
		/// </summary>
		public IEnumerable<OrderDto> Orders { get; set; }
	}
}
