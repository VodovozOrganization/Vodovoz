using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class DistrictInfo
	{
		public LogisticsArea District;
		public List<Order> OrdersInDistrict = new List<Order>();

		public DistrictInfo(LogisticsArea district)
		{
			District = district;
		}
	}
}
