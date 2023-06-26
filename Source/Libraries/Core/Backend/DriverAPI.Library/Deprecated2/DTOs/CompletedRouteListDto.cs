using System;
using System.Collections.Generic;
using DriverAPI.Library.DTOs;

namespace DriverAPI.Library.Deprecated2.DTOs
{
	[Obsolete("Будет удален с прекращением поддержки API v2")]
	public class CompletedRouteListDto
	{
		public int RouteListId { get; set; }
		public RouteListDtoStatus RouteListStatus { get; set; }
		public decimal CashMoney { get; set; }
		public decimal TerminalMoney { get; set; }
		public int TerminalOrdersCount { get; set; }
		public int FullBottlesToReturn { get; set; }
		public int EmptyBottlesToReturn { get; set; }
		public IEnumerable<OrdersReturnItemDto> OrdersReturnItems { get; set; }
	}
}
