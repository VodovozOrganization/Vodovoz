using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.EntityRepositories.Profitability
{
	public interface IRouteListProfitabilityRepository
	{
		IEnumerable<RouteList> GetAllRouteListsWithProfitabilitiesByCalculatedMonth(IUnitOfWork uow, DateTime date);
		IEnumerable<RouteList> GetAllRouteListsWithProfitabilitiesByDate(IUnitOfWork uow, DateTime date, CarModel carModel = null, FuelType fuelType = null);
		IEnumerable<RouteList> GetAllRouteListsWithProfitabilitiesBetweenDates(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo);
	}
}
