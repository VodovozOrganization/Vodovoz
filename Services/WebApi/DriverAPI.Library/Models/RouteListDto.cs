namespace DriverAPI.Library.Models
{
	public class RouteListDto
	{
		public RouteListDtoCompletionStatus CompletionStatus { get; set; }
		public APIIncompletedRouteList IncompletedRouteList { get; set; }
		public CompletedRouteListDto CompletedRouteList { get; set; }
	}
}
