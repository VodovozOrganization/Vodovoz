using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Controllers
{
	public interface IRouteListProfitabilityController
	{
		void CalculateNewRouteListProfitability(IUnitOfWork uow, RouteList routeList);
		void ReCalculateRouteListProfitability(IUnitOfWork uow, RouteList routeList, bool useDataFromDataBase = false);
		void RecalculateRouteListProfitabilitiesByCalculatedMonth(
			IUnitOfWork uow, DateTime date, bool useDataFromDataBase, IProgressBarDisplayable progressBarDisplayable);
		void RecalculateRouteListProfitabilitiesByDate(IUnitOfWork uow, DateTime date);
		void RecalculateRouteListProfitabilitiesBetweenDates(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo);
	}
}
