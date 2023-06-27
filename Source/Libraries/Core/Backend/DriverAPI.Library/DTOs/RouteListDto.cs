namespace DriverAPI.Library.DTOs
{
	public class RouteListDto
	{
		public string ForwarderFullName { get; set; }
		public RouteListDtoCompletionStatus CompletionStatus { get; set; }
		public IncompletedRouteListDto IncompletedRouteList { get; set; }
		public CompletedRouteListDto CompletedRouteList { get; set; }
	}
}
