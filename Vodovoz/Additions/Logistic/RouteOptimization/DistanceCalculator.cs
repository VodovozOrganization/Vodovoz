using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using Vodovoz.Domain.Client;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public static class DistanceCalculator
	{
		static Point BasePoint = new Point(Constants.BaseLatitude, Constants.BaseLongitude);

		public static double GetDistance(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			return DistanceOp.Distance(fromDP.NetTopologyPoint, toDP.NetTopologyPoint);
		}

		public static double GetDistanceFromBase(DeliveryPoint toDP)
		{
			return DistanceOp.Distance(BasePoint, toDP.NetTopologyPoint);
		}

		public static double GetDistanceToBase(DeliveryPoint fromDP)
		{
			return DistanceOp.Distance(fromDP.NetTopologyPoint, BasePoint);
		}

	}
}
