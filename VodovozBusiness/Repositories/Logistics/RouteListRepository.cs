using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Repository.Logistics
{
	[Obsolete("Используйте одноимённый класс из Vodovoz.EntityRepositories.Logistic")]
	public static class RouteListRepository
	{
		[Obsolete]
		public static IList<RouteList> GetDriverRouteLists(IUnitOfWork uow, Employee driver, RouteListStatus status, DateTime date)
		{
			return new EntityRepositories.Logistic.RouteListRepository().GetDriverRouteLists(uow, driver, status, date);
		}

		[Obsolete]
		public static QueryOver<RouteList> GetRoutesAtDay(DateTime date, List<int> geographicGroupsIds)
		{
			return new EntityRepositories.Logistic.RouteListRepository().GetRoutesAtDay(date, geographicGroupsIds);
		}
	}
}