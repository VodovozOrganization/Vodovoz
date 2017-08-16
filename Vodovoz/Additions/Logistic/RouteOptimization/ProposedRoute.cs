using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class ProposedRoute
	{
		public List<ProposedRoutePoint> Orders = new List<ProposedRoutePoint>();
		public AtWorkDriver Driver;

		public RouteList RealRoute;

		public long RouteCost;

		public Car Car {
			get {
				return Driver.Car;
			}
		}

		public ProposedRoute(AtWorkDriver driver)
		{
			Driver = driver;
		}
	}

	public class ProposedRoutePoint
	{
		public TimeSpan ProposedTimeStart;
		public TimeSpan ProposedTimeEnd;
		public Order Order;

		public string DebugMaxMin;

		public ProposedRoutePoint(TimeSpan timeStart, TimeSpan timeEnd, Order order)
		{
			ProposedTimeStart = timeStart;
			ProposedTimeEnd = timeEnd;
			Order = order;
		}
	}
}
