using DriverAPI.Library.DTOs;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;

namespace DriverAPI.Library.DataAccess
{
	public class TrackPointsModel : ITrackPointsModel
	{
		private readonly ILogger<TrackPointsModel> _logger;
		private readonly ITrackRepository _trackRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IUnitOfWork _unitOfWork;

		public TrackPointsModel(ILogger<TrackPointsModel> logger, ITrackRepository trackRepository, IRouteListRepository routeListRepository, IUnitOfWork unitOfWork)
		{
			this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this._trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			this._routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			this._unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public void RegisterForRouteList(int routeListId, IEnumerable<TrackCoordinateDto> trackList)
		{
			var track = _trackRepository.GetTrackByRouteListId(_unitOfWork, routeListId);

			if (track == null)
			{
				var routeList = _routeListRepository.GetRouteListById(_unitOfWork, routeListId)
					?? throw new DataNotFoundException(nameof(routeListId), $"Маршрутный лист {routeListId} не найден");

				track = new Track()
				{
					RouteList = routeList,
					Driver = routeList.Driver
				};
			}

			var receivedPoints = new Dictionary<DateTime, TrackPoint>();

			foreach (var trackCoordinate in trackList)
			{
				if (track.TrackPoints.Any(t => t.TimeStamp == trackCoordinate.ActionTime))
				{
					_logger.LogInformation($"Уже зарегистрирована точка для времени {trackCoordinate.ActionTime}");
					continue;
				}

				var trackPoint = new TrackPoint()
				{
					Track = track,
					Latitude = decimal.ToDouble(trackCoordinate.Latitude),
					Longitude = decimal.ToDouble(trackCoordinate.Longitude),
					TimeStamp = trackCoordinate.ActionTime
				};

				track.TrackPoints.Add(trackPoint);
			}

			_unitOfWork.Save(track);
			_unitOfWork.Commit();
		}
	}
}
