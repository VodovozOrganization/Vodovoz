using DriverApi.Contracts.V5;
using DriverAPI.Library.Exceptions;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;

namespace DriverAPI.Library.V5.Services
{
	internal class TrackPointsService : ITrackPointsService
	{
		private readonly ILogger<TrackPointsService> _logger;
		private readonly ITrackRepository _trackRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListService _routeListModel;
		private readonly IUnitOfWork _unitOfWork;

		public TrackPointsService(ILogger<TrackPointsService> logger,
			ITrackRepository trackRepository,
			IRouteListRepository routeListRepository,
			IRouteListService routeListModel,
			IUnitOfWork unitOfWork)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListModel = routeListModel ?? throw new ArgumentNullException(nameof(routeListModel));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public void RegisterForRouteList(int routeListId, IList<TrackCoordinateDto> trackList, int driverId)
		{
			var track = _trackRepository.GetTrackByRouteListId(_unitOfWork, routeListId);

			var routeList = _routeListRepository.GetRouteListById(_unitOfWork, routeListId)
					?? throw new DataNotFoundException(nameof(routeListId), $"Маршрутный лист {routeListId} не найден");

			if(routeList.Status != RouteListStatus.EnRoute
			&& routeList.Status != RouteListStatus.Delivered)
			{
				_logger.LogWarning("Попытка записать трек для МЛ {RouteListId}, МЛ в статусе '{RouteListStatus}'",
					routeListId,
					routeList.Status);
				throw new InvalidOperationException($"Нельзя записать трек для МЛ {routeListId}, МЛ в статусе недоступном для записи трека");
			}

			if(!_routeListModel.IsRouteListBelongToDriver(routeListId, driverId))
			{
				_logger.LogWarning("Сотрудник ({EmployeeId}) попытался зарегистрировать трек для МЛ {RouteListId} водителя {DriverId}",
					driverId,
					routeListId,
					routeList.Driver.Id);
				throw new InvalidOperationException("Нельзя регистрировать координаты трека к чужому МЛ");
			}

			if(track == null)
			{
				track = new Track
				{
					RouteList = routeList,
					Driver = routeList.Driver
				};
			}

			foreach(var trackPoint in trackList)
			{
				trackPoint.Latitude = Math.Round(trackPoint.Latitude, 8);
				trackPoint.Longitude = Math.Round(trackPoint.Longitude, 8);
				trackPoint.ActionTimeUtc =
					new DateTime(
						trackPoint.ActionTimeUtc.Year,
						trackPoint.ActionTimeUtc.Month,
						trackPoint.ActionTimeUtc.Day,
						trackPoint.ActionTimeUtc.Hour,
						trackPoint.ActionTimeUtc.Minute,
						trackPoint.ActionTimeUtc.Second,
						trackPoint.ActionTimeUtc.Kind
					);
			}

			var trackPoints = trackList
				.GroupBy(x =>
					new
					{
						x.ActionTimeUtc,
						x.Latitude,
						x.Longitude
					})
				.Select(group =>
					new TrackPoint
					{
						Track = track,
						Latitude = decimal.ToDouble(group.Key.Latitude),
						Longitude = decimal.ToDouble(group.Key.Longitude),
						TimeStamp = group.Key.ActionTimeUtc.ToLocalTime()
					});

			foreach(var trackPoint in trackPoints)
			{
				if(track.TrackPoints.Any(t => t.TimeStamp == trackPoint.TimeStamp))
				{
					_logger.LogInformation("Уже зарегистрирована точка для времени {TrackPointTimeStamp}", trackPoint.TimeStamp);
					continue;
				}

				track.TrackPoints.Add(trackPoint);
			}

			_unitOfWork.Save(track);
			_unitOfWork.Commit();
		}
	}
}
