using System;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Application.Services.Logistics
{
	public interface IRouteOptimizer
	{
		int MaxTimeSeconds { get; set; }
		Action<string> StatisticsTxtAction { get; set; }

		void CreateRoutes(DateTime date, TimeSpan drvStartTime, TimeSpan drvEndTime);
		IProposedRoute RebuidOneRoute(RouteList route);
	}
}
