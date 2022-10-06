using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Profitability
{
	public interface IRouteListProfitabilityRepository
	{
		IEnumerable<RouteList> GetAllRouteListsWithProfitabilitiesByCalculatedMonth(IUnitOfWork uow, DateTime date);
		IEnumerable<RouteList> GetAllRouteListsWithProfitabilitiesByDate(IUnitOfWork uow, DateTime date);
		IEnumerable<RouteList> GetAllRouteListsWithProfitabilitiesBetweenDates(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo);
	}
}
