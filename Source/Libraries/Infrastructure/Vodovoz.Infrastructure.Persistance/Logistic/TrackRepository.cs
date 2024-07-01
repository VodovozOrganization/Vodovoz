using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using Vodovoz.Domain.Logistic;
using NHibernate.Persister.Entity;
using Vodovoz.Core.Domain;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Infrastructure.Persistance.Logistic
{
	internal sealed class TrackRepository : ITrackRepository
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
			RouteList routeListsAlias = null;

			var lastTimeTrackQuery = QueryOver.Of(() => subPoint)
				.Where(() => subPoint.Track.Id == trackAlias.Id);

			if(beforeTime.HasValue)
			{
				lastTimeTrackQuery.Where(p => p.ReceiveTimeStamp < beforeTime);
			}

			lastTimeTrackQuery.Select(Projections.Max(() => subPoint.TimeStamp));

			return uow.Session.QueryOver<TrackPoint>()
				.JoinAlias(p => p.Track, () => trackAlias)
				.JoinAlias(() => trackAlias.RouteList, () => routeListsAlias)
				.Where(() => trackAlias.RouteList.Id.IsIn(routeListsIds))
				.WithSubquery.WhereProperty(p => p.TimeStamp).Eq(lastTimeTrackQuery)
				.SelectList(list => list
					.Select(() => routeListsAlias.Driver.Id).WithAlias(() => result.DriverId)
					.Select(() => trackAlias.RouteList.Id).WithAlias(() => result.RouteListId)
					.Select(x => x.TimeStamp).WithAlias(() => result.Time)
					.Select(x => x.Latitude).WithAlias(() => result.Latitude)
					.Select(x => x.Longitude).WithAlias(() => result.Longitude))
				.TransformUsing(Transformers.AliasToBean<DriverPosition>())
				.List<DriverPosition>();
		}

		public IList<DriverPositionWithFastDeliveryRadius> GetLastPointForRouteListsWithRadius(IUnitOfWork uow, int[] routeListsIds, DateTime? beforeTime = null)
		{
			IList<DriverPosition> driverPositions;

			if(beforeTime.HasValue)
			{
				driverPositions = GetLastPointForRouteLists(uow, routeListsIds, beforeTime);
			}

			else
			{
				driverPositions = GetLastPointForRouteLists(uow, routeListsIds);
			}

			return driverPositions
				.Select(pos => new DriverPositionWithFastDeliveryRadius()
				{
					DriverId = pos.DriverId,
					RouteListId = pos.RouteListId,
					Time = pos.Time,
					Latitude = pos.Latitude,
					Longitude = pos.Longitude,
					FastDeliveryRadius = (double)(uow.GetById<RouteList>(pos.RouteListId) ?? new RouteList()).GetFastDeliveryMaxDistanceValue(pos.Time)
				}).ToList();
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

		public IList<DriverPosition> GetLastRouteListFastDeliveryTrackPoints(IUnitOfWork uow, int[] routeListsIds, TimeSpan timeSpanDisconnected, DateTime? beforeTime = null)
		{
			Track trackAlias = null;
			TrackPoint subPoint = null;
			DriverPosition result = null;
			RouteList routeListsAlias = null;

			var lastTimeTrackQuery = QueryOver.Of(() => subPoint)
				.Where(() => subPoint.Track.Id == trackAlias.Id);

			if(beforeTime.HasValue)
			{
				lastTimeTrackQuery.Where(p => p.ReceiveTimeStamp < beforeTime)
					.And(() => subPoint.TimeStamp >= beforeTime.Value.Add(timeSpanDisconnected));
			}
			else
			{
				lastTimeTrackQuery.Where(() => subPoint.TimeStamp >= DateTime.Now.Add(timeSpanDisconnected));
			}

			lastTimeTrackQuery.Select(Projections.Max(() => subPoint.TimeStamp));

			return uow.Session.QueryOver<TrackPoint>()
				.JoinAlias(p => p.Track, () => trackAlias)
				.JoinAlias(() => trackAlias.RouteList, () => routeListsAlias)
				.Where(() => trackAlias.RouteList.Id.IsIn(routeListsIds))
				.WithSubquery.WhereProperty(p => p.TimeStamp).Eq(lastTimeTrackQuery)
				.SelectList(list => list
					.Select(() => routeListsAlias.Driver.Id).WithAlias(() => result.DriverId)
					.Select(() => trackAlias.RouteList.Id).WithAlias(() => result.RouteListId)
					.Select(x => x.TimeStamp).WithAlias(() => result.Time)
					.Select(x => x.Latitude).WithAlias(() => result.Latitude)
					.Select(x => x.Longitude).WithAlias(() => result.Longitude))
				.TransformUsing(Transformers.AliasToBean<DriverPosition>())
				.List<DriverPosition>();
		}

		public IList<DriverPositionWithFastDeliveryRadius> GetLastRouteListFastDeliveryTrackPointsWithRadius(IUnitOfWork uow, int[] routeListsIds, TimeSpan timeSpanDisconnected, DateTime? beforeTime = null)
		{
			IList<DriverPosition> driverPositions;

			if(beforeTime.HasValue)
			{
				driverPositions = GetLastRouteListFastDeliveryTrackPoints(uow, routeListsIds, timeSpanDisconnected, beforeTime);
			}
			else
			{
				driverPositions = GetLastRouteListFastDeliveryTrackPoints(uow, routeListsIds, timeSpanDisconnected);
			}

			return driverPositions
				.Select(pos => new DriverPositionWithFastDeliveryRadius()
				{
					DriverId = pos.DriverId,
					RouteListId = pos.RouteListId,
					Time = pos.Time,
					Latitude = pos.Latitude,
					Longitude = pos.Longitude,
					FastDeliveryRadius = (double)(uow.GetById<RouteList>(pos.RouteListId) ?? new RouteList()).GetFastDeliveryMaxDistanceValue(beforeTime.HasValue ? pos.Time : DateTime.Now)
				}).ToList();
		}
	}
}
