using System;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class DriverPosition
	{
		public int DriverId { get; set; }
		public int RouteListId { get; set; }
		public DateTime Time { get; set; }
		public Double Latitude { get; set; }
		public Double Longitude { get; set; }
	}

	public class DriverPositionWithFastDeliveryRadius : DriverPosition
	{
		public double FastDeliveryRadius { get; set; }
	}
}
