using NHibernate.Criterion;
using NHibernate.Persister.Entity;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class OldTrackPointRepository : IOldTrackPointRepository
	{
		public DateTime GetMaxOldTrackPointDate(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<OldTrackPoint>()
				.Select(Projections.Max<OldTrackPoint>(otp => otp.TimeStamp))
				.SingleOrDefault<DateTime>();
		}

		#region Архивация track_points

		public void TrackPointsArchiving(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var factory = uow.Session.SessionFactory;
			var tpPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(TrackPoint));
			var oldTpPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(OldTrackPoint));

			var timeStampColumn = tpPersister.GetPropertyColumnNames(nameof(TrackPoint.TimeStamp)).First();

			var query = $"INSERT INTO {oldTpPersister.TableName} "
				+ $"(SELECT * "
				+ $"FROM {tpPersister.TableName} "
				+ $"WHERE {tpPersister.TableName}.{timeStampColumn} BETWEEN '{dateTimeFrom:yyyy-MM-dd}' AND '{dateTimeTo:yyyy-MM-dd HH:mm:ss}');";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		#endregion
	}
}
