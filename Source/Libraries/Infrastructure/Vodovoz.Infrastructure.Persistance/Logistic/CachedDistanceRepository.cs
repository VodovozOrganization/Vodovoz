using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Persister.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Infrastructure.Persistance.Logistic
{
	internal sealed class CachedDistanceRepository : ICachedDistanceRepository
	{
		public IList<CachedDistance> GetCache(IUnitOfWork uow, long[] hash)
		{
			return uow.Session.QueryOver<CachedDistance>()
					  .Where(x => x.FromGeoHash.IsIn(hash) && x.ToGeoHash.IsIn(hash))
					  .List();
		}

		public IList<CachedDistance> GetCache(IUnitOfWork uow, WayHash[] hashes)
		{
			var query = uow.Session.QueryOver<CachedDistance>();

			var disjunction = new Disjunction();

			foreach(var hash in hashes)
			{
				disjunction.Add<CachedDistance>(x => x.FromGeoHash == hash.FromHash && x.ToGeoHash == hash.ToHash);
			}

			return query.Where(disjunction).List();
		}

		public CachedDistance GetFirstCacheByCreateDate(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<CachedDistance>()
				.OrderBy(cd => cd.Created).Asc
				.Take(1)
				.SingleOrDefault();
		}

		#region Удаление кэша

		public void DeleteCachedDistance(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo)
		{
			var factory = uow.Session.SessionFactory;
			var cdPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(CachedDistance));

			var createdColumn = cdPersister.GetPropertyColumnNames(nameof(CachedDistance.Created)).First();

			var query = $"DELETE "
				+ $"FROM {cdPersister.TableName} "
				+ $"WHERE {cdPersister.TableName}.{createdColumn} BETWEEN '{dateFrom:yyyy-MM-dd}' AND '{dateTo:yyyy-MM-dd HH:mm:ss}';";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		#endregion
	}
}
