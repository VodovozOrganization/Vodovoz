using System.Collections.Generic;
using DriverAPI.Library.DTOs;

namespace DriverAPI.Library.Deprecated3.DTOs
{
	/// <summary>
	/// Незавершенный маршрутный лист
	/// </summary>
	public class IncompletedRouteListDto
	{
		/// <summary>
		/// Номер маршрутного листа
		/// </summary>
		public int RouteListId { get; set; }

		/// <summary>
		/// Статус маршрутного листа
		/// </summary>
		public RouteListDtoStatus RouteListStatus { get; set; }

		/// <summary>
		/// Адреса маршрутного листа
		/// </summary>
		public IEnumerable<RouteListAddressDto> RouteListAddresses { get; set; }
	}
}
