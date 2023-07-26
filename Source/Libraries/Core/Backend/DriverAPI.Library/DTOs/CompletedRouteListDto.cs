using System.Collections.Generic;

namespace DriverAPI.Library.DTOs
{
	public class CompletedRouteListDto
	{
		public int RouteListId { get; set; }
		public RouteListDtoStatus RouteListStatus { get; set; }
		public decimal CashMoney { get; set; }
		public decimal TerminalCardMoney { get; set; }
		public decimal TerminalQRMoney { get; set; }
		public int TerminalOrdersCount { get; set; }
		public int FullBottlesToReturn { get; set; }
		public int EmptyBottlesToReturn { get; set; }
		public IEnumerable<OrdersReturnItemDto> OrdersReturnItems { get; set; }
	}
}
