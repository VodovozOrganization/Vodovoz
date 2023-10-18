using DriverAPI.Library.DTOs;
using System;

namespace DriverAPI.Library.Deprecated3.DTOs
{
	/// <summary>
	/// Маршрутный лист
	/// </summary>
	[Obsolete("Будет удален с прекращением поддержки API v3")]
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
