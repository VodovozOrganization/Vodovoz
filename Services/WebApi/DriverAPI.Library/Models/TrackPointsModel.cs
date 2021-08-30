using DriverAPI.Library.DTOs;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;

namespace DriverAPI.Library.Models
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

			var routeList = _routeListRepository.GetRouteListById(_unitOfWork, routeListId)
					?? throw new DataNotFoundException(nameof(routeListId), $"Маршрутный лист {routeListId} не найден");

			if(routeList.Status != RouteListStatus.EnRoute
			&& routeList.Status != RouteListStatus.Delivered)
			{
				_logger.LogWarning($"Попытка записать трек для МЛ {routeListId}, МЛ в статусе '{routeList.Status}'");
				throw new InvalidOperationException($"Нельзя записать трек для МЛ {routeListId}, МЛ в статусе недоступном для записи трека");
			}

			if (track == null)
			{
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
