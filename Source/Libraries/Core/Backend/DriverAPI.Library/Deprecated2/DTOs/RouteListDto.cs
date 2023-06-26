using System;
using DriverAPI.Library.DTOs;

namespace DriverAPI.Library.Deprecated2.DTOs
{
	public class RouteListDto
	{
		[Obsolete("Будет удален с прекращением поддержки API v2")]
		public string ForwarderFullName { get; set; }
		public RouteListDtoCompletionStatus CompletionStatus { get; set; }
		public IncompletedRouteListDto IncompletedRouteList { get; set; }
		public CompletedRouteListDto CompletedRouteList { get; set; }
	}
}
