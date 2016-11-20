using System;
using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Repository.Logistics
{
	public static class RouteColumnRepository
	{
		public static IList<RouteColumn> ActiveColumns (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<RouteColumn> ().List<RouteColumn> ();
		}

		public static IList<Nomenclature> NomenclaturesForColumn(IUnitOfWork uow, RouteColumn column)
		{
			return uow.Session.QueryOver<Nomenclature> ()
				.Where(x => x.RouteListColumn.Id == column.Id)
				.List();
		}
	}
}

