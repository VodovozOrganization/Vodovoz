using System;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public class MarketingReportClientHistoryNode
	{
		public int ClientId { get; set; }
		public DateTime FirstOrderDate { get; set; }
		public DateTime LastOrderDate { get; set; }
		public int OrdersCount { get; set; }
	}
}
