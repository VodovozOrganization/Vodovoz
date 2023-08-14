using System;
using RouteListDtoCompletionStatus = DriverAPI.Library.DTOs.RouteListDtoCompletionStatus;
using IncompletedRouteListDto = DriverAPI.Library.DTOs.IncompletedRouteListDto;

namespace DriverAPI.Library.Deprecated2.DTOs
{
	/// <summary>
	/// Маршрутный лист
	/// </summary>
	[Obsolete("Будет удален с прекращением поддержки API v2")]
	public class RouteListDto
	{
		/// <summary>
		/// Имя экспедитора
		/// </summary>
		public string ForwarderFullName { get; set; }

		/// <summary>
		/// Статус завершенности
		/// </summary>
		public RouteListDtoCompletionStatus CompletionStatus { get; set; }

		/// <summary>
		/// Если не завершен, содержит информацию по не завершенному маршрутному листу
		/// </summary>
		public IncompletedRouteListDto IncompletedRouteList { get; set; }

		/// <summary>
		/// Если завершен, сожержит информацию по завершенному маршрутному листу
		/// </summary>
		public CompletedRouteListDto CompletedRouteList { get; set; }
	}
}
