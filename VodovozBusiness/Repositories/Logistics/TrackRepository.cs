using System;
using Vodovoz.Domain.Logistic;
using QSOrmProject;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace Vodovoz.Repository.Logistics
{
	public static class TrackRepository
	{
		public static Track GetTrackForRouteList (IUnitOfWork uow, int routeListId)
		{
			Track trackAlias = null;

			return uow.Session.QueryOver<Track> (() => trackAlias)
				.Where (() => trackAlias.RouteList.Id == routeListId)
				.SingleOrDefault ();
		}

		public static IList<TrackPoint> GetPointsForTrack(IUnitOfWork uow, int trackId)
		{
			return uow.Session.QueryOver<TrackPoint>()
				.Where(x => x.Track.Id == trackId)
				.List();
		}

		public static IList<TrackPoint> GetPointsForRouteList(IUnitOfWork uow, int routeListId)
		{
			Track trackAlias = null;

			return uow.Session.QueryOver<TrackPoint>()
				.JoinAlias(x => x.Track, () => trackAlias)
				.Where(() => trackAlias.RouteList.Id == routeListId)
				.List();
		}

/*		public static IList<TrackPoint> GetPointsForDrivers(IUnitOfWork uow, int[] driversIds, DateTime startTime)
		{
			Track trackAlias = null;
			return uow.Session.QueryOver<TrackPoint>()
				.JoinAlias(p => p.Track, () => trackAlias)
				.Where(() => trackAlias.Driver.Id.IsIn(driversIds))
				.Where()
		}
*/
		public static IList<DriverPosition> GetLastPointForDrivers(IUnitOfWork uow, int[] driversIds, DateTime? beforeTime = null)
		{
			Track trackAlias = null;
			Track subTrackAlias = null;
			TrackPoint subPoint = null;
			DriverPosition result = null;

			var lastTimeTrackQuery = QueryOver.Of<TrackPoint>(() => subPoint)
				.JoinAlias(p => p.Track, () => subTrackAlias)
				.Where(() => subTrackAlias.Driver.Id == trackAlias.Driver.Id);
			if (beforeTime.HasValue)
				lastTimeTrackQuery.Where(p => p.TimeStamp <= beforeTime);
			
			lastTimeTrackQuery.Select(Projections.Max(() => subPoint.TimeStamp));

			return uow.Session.QueryOver<TrackPoint>()
				.JoinAlias(p => p.Track, () => trackAlias)
				.Where(() => trackAlias.Driver.Id.IsIn(driversIds))
				.WithSubquery.WhereProperty(p => p.TimeStamp).Eq(lastTimeTrackQuery)
				.SelectList(list => list
					.Select(() => trackAlias.Driver.Id).WithAlias(() => result.DriverId)
					.Select(x => x.TimeStamp).WithAlias(() => result.Time)
					.Select(x => x.Latitude).WithAlias(() => result.Latitude)
					.Select(x => x.Longitude).WithAlias(() => result.Longitude)
				).TransformUsing(Transformers.AliasToBean<DriverPosition>())
				.List<DriverPosition>();
		}

		public class DriverPosition{
			public int DriverId { get; set;}
			public DateTime Time { get; set;}
			public Double Latitude { get; set;}
			public Double Longitude { get; set;}
		}
	}
}

