using System;

namespace DriverAPI.Library.Models
{
	public class APIRouteListAddress
	{
		public int Id { get; set; }
		public int OrderId { get; set; }
		public APIRouteListAddressStatus Status { get; set; }
		public DateTime DeliveryTime { get; set; }
		public int FullBottlesCount { get; set; }
		public APIAddress Address { get; set; }
	}
}
