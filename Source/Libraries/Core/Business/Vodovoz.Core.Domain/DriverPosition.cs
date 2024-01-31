using System;

namespace Vodovoz.Core.Domain
{
	public class DriverPosition
	{
		public int DriverId { get; set; }
		public int RouteListId { get; set; }
		public DateTime Time { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}

	public class DriverPositionWithFastDeliveryRadius : DriverPosition
	{
		public double FastDeliveryRadius { get; set; }
	}
}
