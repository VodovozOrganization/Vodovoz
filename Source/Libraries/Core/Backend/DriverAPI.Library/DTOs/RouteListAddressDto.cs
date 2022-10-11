using System;

namespace DriverAPI.Library.DTOs
{
	public class RouteListAddressDto
	{
		public int Id { get; set; }
		public int OrderId { get; set; }
		public RouteListAddressDtoStatus Status { get; set; }
		public DateTime DeliveryIntervalStart { get; set; }
		public DateTime DeliveryIntervalEnd { get; set; }
		public AddressDto Address { get; set; }
	}
}
