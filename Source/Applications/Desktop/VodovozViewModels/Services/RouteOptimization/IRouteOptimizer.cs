using System;
using System.Collections.Generic;
using System.ComponentModel;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Services.RouteOptimization
{
	public interface IRouteOptimizer : INotifyPropertyChanged
	{
		int MaxTimeSeconds { get; set; }
		Action<string> StatisticsTxtAction { get; set; }
		List<string> WarningMessages { get; }
		List<ProposedRoute> ProposedRoutes { get; }
		IUnitOfWork UoW { get; set; }
		IList<RouteList> Routes { get; set; }
		IList<Order> Orders { get; set; }
		IList<AtWorkDriver> Drivers { get; set; }
		IList<AtWorkForwarder> Forwarders { get; set; }

		void CreateRoutes(DateTime date, TimeSpan drvStartTime, TimeSpan drvEndTime, Func<string, bool> askIfAvailableFunc);
		ProposedRoute RebuidOneRoute(RouteList route);
	}
}
