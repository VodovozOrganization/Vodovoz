using DriverAPI.Library.Models;
using System;

namespace DriverAPI.Models
{
	public class RouteListAddressCoordinate
	{
		public int RouteListAddressId { get; set; }
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
		public DateTime ActionTime { get; set; }
		public APIActionType ActionType { get; set; }
	}
}
