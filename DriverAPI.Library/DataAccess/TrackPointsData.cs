using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using QS.DomainModel.UoW;

namespace DriverAPI.Library.DataAccess
{
    public class TrackPointsData : ITrackPointsData
    {
        private readonly ILogger<TrackPointsData> logger;
        private readonly ITrackRepository trackRepository;
        private readonly IRouteListRepository routeListRepository;
        private readonly IUnitOfWork unitOfWork;

        public TrackPointsData(ILogger<TrackPointsData> logger, ITrackRepository trackRepository, IRouteListRepository routeListRepository, IUnitOfWork unitOfWork)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
            this.routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public void RegisterForRouteList(int routeListId, IEnumerable<APITrackCoordinate> trackList)
        {
            var track = trackRepository.GetTrackByRouteListId(unitOfWork, routeListId);

            if (track == null) // Должен ли создаваться трек, если его нету?
            {
                var routeList = routeListRepository.GetRouteLists(unitOfWork, new[] { routeListId }).SingleOrDefault();
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
                    logger.LogInformation($"Уже зарегистрирована точка для времени {trackCoordinate.ActionTime}");
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

            unitOfWork.Save(track);
            unitOfWork.Commit();
        }
    }
}
