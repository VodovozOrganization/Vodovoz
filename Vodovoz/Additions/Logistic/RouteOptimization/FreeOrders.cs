using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class FreeOrders
	{
		public DistrictInfo District {get; private set;}

		public List<Order> Orders;

		public FreeOrders(DistrictInfo district, List<Order> orders)
		{
			District = district;
			Orders = orders;
		}

		public FreeOrders Clone()
		{
			var freeorders = new FreeOrders(District, Orders.ToList());
			return freeorders;
		}
	}
}
