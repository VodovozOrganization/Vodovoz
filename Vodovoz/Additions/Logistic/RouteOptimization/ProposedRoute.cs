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
		public DateTime ProposedTime;
		public Order Order;

		public string DebugMaxMin;

		public ProposedRoutePoint(DateTime time, Order order)
		{
			ProposedTime = time;
			Order = order;
		}
	}
}
