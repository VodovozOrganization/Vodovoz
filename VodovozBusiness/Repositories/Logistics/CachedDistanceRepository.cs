using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Repository.Logistics
{
	public static class CachedDistanceRepository
	{
		public static IList<CachedDistance> GetCache (IUnitOfWork uow, long[] hash){
			return uow.Session.QueryOver<CachedDistance>()
				      .Where(x => x.FromGeoHash.IsIn(hash) || x.ToGeoHash.IsIn(hash))
				      .List();
		}
	}
}
