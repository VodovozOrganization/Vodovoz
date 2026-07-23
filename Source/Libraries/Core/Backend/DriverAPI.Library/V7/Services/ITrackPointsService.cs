using DriverApi.Contracts.V7;
using System.Collections.Generic;

namespace DriverAPI.Library.V7.Services
{
	/// <summary>
	/// Сервис для работы с координатами трека
	/// </summary>
	public interface ITrackPointsService
	{
		/// <summary>
		/// Регистрация координат трека МЛ
		/// </summary>
		/// <param name="routeListId">Номер МЛ</param>
		/// <param name="trackList">Список координат трека</param>
		/// <param name="driverId">Идентификатор водителя</param>
		void RegisterForRouteList(int routeListId, IList<TrackCoordinateDto> trackList, int driverId);
	}
}
