﻿using System.Collections.Generic;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Repository.Logistics
{
	public static class CachedDistanceRepository
	{
		public static IList<CachedDistance> GetCache (IUnitOfWork uow, long[] hash){
			return uow.Session.QueryOver<CachedDistance>()
				      .Where(x => x.FromGeoHash.IsIn(hash) && x.ToGeoHash.IsIn(hash))
				      .List();
		}

		public static IList<CachedDistance> GetCache(IUnitOfWork uow, WayHash[] hashes)
		{
			var query = uow.Session.QueryOver<CachedDistance>();
			
			var disjunction = new Disjunction();
			foreach(var hash in hashes)
			{
				disjunction.Add<CachedDistance>(x => x.FromGeoHash == hash.FromHash && x.ToGeoHash == hash.ToHash);
			}
			return query.Where(disjunction).List();
		}

	}
}
