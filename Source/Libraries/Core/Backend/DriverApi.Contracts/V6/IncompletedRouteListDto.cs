using System.Collections.Generic;

namespace DriverApi.Contracts.V6
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

		/// <summary>
		/// Условия необходимые для принятия перед началом работы с МЛ
		/// </summary>
		public IEnumerable<RouteListSpecialConditionDto> SpecialConditionsToAccept { get; set; }
	}
}
