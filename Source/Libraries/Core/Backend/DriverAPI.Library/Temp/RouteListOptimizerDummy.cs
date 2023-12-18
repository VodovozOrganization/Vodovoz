using System;
using Vodovoz.Application.Services.Logistics.RouteOptimization;
using Vodovoz.Domain.Logistic;

namespace DriverAPI.Library.Temp
{
	internal class RouteListOptimizerDummy : IRouteOptimizer
	{
		public int MaxTimeSeconds { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Action<string> StatisticsTxtAction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public void CreateRoutes(DateTime date, TimeSpan drvStartTime, TimeSpan drvEndTime)
		{
			throw new NotImplementedException();
		}

		public IProposedRoute RebuidOneRoute(RouteList route)
		{
			throw new NotImplementedException();
		}
	}
}
