using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using Vodovoz.Domain.Logistic;
using NHibernate.Persister.Entity;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class TrackRepository : ITrackRepository
	{
		public Track GetTrackByRouteListId(IUnitOfWork unitOfWork, int routeListId)
		{
			return unitOfWork.Session.Query<Track>().SingleOrDefault(t => t.RouteList.Id == routeListId);
		}
		
		public IList<TrackPoint> GetPointsForTrack(IUnitOfWork uow, int trackId)
		{
			return uow.Session.QueryOver<TrackPoint>()
				.Where(x => x.Track.Id == trackId)
				.List();
		}

		public IList<TrackPoint> GetPointsForRouteList(IUnitOfWork uow, int routeListId)
		{
			Track trackAlias = null;

			return uow.Session.QueryOver<TrackPoint>()
				.JoinAlias(x => x.Track, () => trackAlias)
				.Where(() => trackAlias.RouteList.Id == routeListId)
				.List();
		}

		public IList<DriverPosition> GetLastPointForRouteLists(IUnitOfWork uow, int[] routeListsIds, DateTime? beforeTime = null)
		{
			Track trackAlias = null;
			TrackPoint subPoint = null;
			DriverPosition result = null;

			var lastTimeTrackQuery = QueryOver.Of<TrackPoint>(() => subPoint)
				.Where(() => subPoint.Track.Id == trackAlias.Id);
			
			if (beforeTime.HasValue)
			{
				lastTimeTrackQuery.Where(p => p.TimeStamp <= beforeTime);
			}

			lastTimeTrackQuery.Select(Projections.Max(() => subPoint.TimeStamp));

			return uow.Session.QueryOver<TrackPoint>()
				.JoinAlias(p => p.Track, () => trackAlias)
				.Where(() => trackAlias.RouteList.Id.IsIn(routeListsIds))
				.WithSubquery.WhereProperty(p => p.TimeStamp).Eq(lastTimeTrackQuery)
				.SelectList(list => list
					.Select(() => trackAlias.Driver.Id).WithAlias(() => result.DriverId)
					.Select(() => trackAlias.RouteList.Id).WithAlias(() => result.RouteListId)
					.Select(x => x.TimeStamp).WithAlias(() => result.Time)
					.Select(x => x.Latitude).WithAlias(() => result.Latitude)
					.Select(x => x.Longitude).WithAlias(() => result.Longitude)
				).TransformUsing(Transformers.AliasToBean<DriverPosition>())
				.List<DriverPosition>();
		}

		public DateTime GetMinTrackPointDate(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<TrackPoint>()
				.Select(Projections.Min<TrackPoint>(tp => tp.TimeStamp))
				.SingleOrDefault<DateTime>();
		}

		public bool TrackPointsExists(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo)
		{
			return uow.Session.QueryOver<TrackPoint>()
				.Where(Restrictions.Between(Projections.Property<TrackPoint>(tp => tp.TimeStamp), dateFrom, dateTo))
				.List()
				.Any();
		}

		public void DeleteTrackPoints(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo)
		{
			var factory = uow.Session.SessionFactory;
			var tpPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(TrackPoint));

			var timeStampColumn = tpPersister.GetPropertyColumnNames(nameof(TrackPoint.TimeStamp)).First();

			var query = $"DELETE "
				+ $"FROM {tpPersister.TableName} "
				+ $"WHERE {tpPersister.TableName}.{timeStampColumn} BETWEEN '{dateFrom:yyyy-MM-dd}' AND '{dateTo:yyyy-MM-dd HH:mm:ss}';";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}
	}
}
