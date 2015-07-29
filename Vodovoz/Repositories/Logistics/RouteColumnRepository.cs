using System;
using QSOrmProject;
using NHibernate.Criterion;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Repository.Logistics
{
	public static class RouteColumnRepository
	{
		public static IList<RouteColumn> ActiveColumns (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<RouteColumn> ().List<RouteColumn> ();
		}
	}
}

