namespace DriverAPI.Library.Models
{
	public class APIRouteList
	{
		public APIRouteListCompletionStatus CompletionStatus { get; set; }
		public APIIncompletedRouteList IncompletedRouteList { get; set; }
		public APICompletedRouteList CompletedRouteList { get; set; }
	}
}
