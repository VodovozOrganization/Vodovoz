﻿using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class ProposedRoute
	{
		public List<Order> Orders = new List<Order>();
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
}
