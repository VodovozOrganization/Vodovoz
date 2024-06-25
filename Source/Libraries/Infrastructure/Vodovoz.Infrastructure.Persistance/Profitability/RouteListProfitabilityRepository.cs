using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Profitability;

namespace Vodovoz.EntityRepositories.Profitability
{
	public class RouteListProfitabilityRepository : IRouteListProfitabilityRepository
	{
		public IEnumerable<RouteList> GetAllRouteListsWithProfitabilitiesByCalculatedMonth(IUnitOfWork uow, DateTime date)
		{
			RouteList resultAlias = null;
			RouteListProfitability routeListProfitabilityAlias = null;

			var query = uow.Session.QueryOver(() => resultAlias)
				.JoinAlias(() => resultAlias.RouteListProfitability, () => routeListProfitabilityAlias)
				.Where(() => resultAlias.Date.Month == date.Month && resultAlias.Date.Year == date.Year
					|| routeListProfitabilityAlias.ProfitabilityConstantsCalculatedMonth == date)
				.List();

			return query;
		}
		
		public IEnumerable<RouteList> GetAllRouteListsWithProfitabilitiesByDate(IUnitOfWork uow, DateTime date)
		{
			RouteList resultAlias = null;
			RouteListProfitability routeListProfitabilityAlias = null;

			var query = uow.Session.QueryOver(() => resultAlias)
				.JoinAlias(() => resultAlias.RouteListProfitability, () => routeListProfitabilityAlias)
				.Where(() => resultAlias.Date >= date)
				.List();

			return query;
		}
		
		public IEnumerable<RouteList> GetAllRouteListsWithProfitabilitiesBetweenDates(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo)
		{
			RouteList resultAlias = null;
			RouteListProfitability routeListProfitabilityAlias = null;

			var query = uow.Session.QueryOver(() => resultAlias)
				.JoinAlias(() => resultAlias.RouteListProfitability, () => routeListProfitabilityAlias)
				.Where(() => resultAlias.Date >= dateFrom)
				.And(() => resultAlias.Date < dateTo)
				.List();

			return query;
		}
	}
}
