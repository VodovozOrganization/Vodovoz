﻿using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Profitability;
using Vodovoz.EntityRepositories.Profitability;

namespace Vodovoz.Infrastructure.Persistance.Profitability
{
	internal sealed class RouteListProfitabilityRepository : IRouteListProfitabilityRepository
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

		public IEnumerable<RouteList> GetAllRouteListsWithProfitabilitiesByDate(IUnitOfWork uow, DateTime date, CarModel carModel = null)
		{
			RouteList resultAlias = null;
			RouteListProfitability routeListProfitabilityAlias = null;
			CarModel carModelAlias = null;
			Car carAlias = null;

			if(carModel == null || carModel.Id == 0)
			{
				return new List<RouteList>();
			}
			
			var query = uow.Session.QueryOver(() => resultAlias)
				.JoinAlias(() => resultAlias.RouteListProfitability, () => routeListProfitabilityAlias)
				.JoinAlias(() => resultAlias.Car, () => carAlias)
				.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.Where(() => resultAlias.Date >= date)
				.Where(() => carAlias.CarModel.Id == carModel.Id);
			
			return query.List();
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
