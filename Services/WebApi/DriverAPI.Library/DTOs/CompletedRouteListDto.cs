namespace DriverAPI.Library.Models
{
	public class CompletedRouteListDto
	{
		public int RouteListId { get; set; }
		public RouteListDtoStatus RouteListStatus { get; set; }
		public decimal CashMoney { get; set; }
		public decimal TerminalMoney { get; set; }
		public int TerminalOrdersCount { get; set; }
		public int FullBottlesToReturn { get; set; }
		public int EmptyBottlesToReturn { get; set; }
	}
}
