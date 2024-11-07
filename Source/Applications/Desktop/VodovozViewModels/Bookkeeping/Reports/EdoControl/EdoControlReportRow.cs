using System;
using Vodovoz.Domain.Orders.Documents;
using static Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters.IncludeExcludeBookkeepingReportsFilterFactory;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl
{
	public class EdoControlReportRow
	{
		public int? EdoContainerId { get; set; }
		public string ClientName { get; set; }
		public int OrderId { get; set; }
		public int? RouteListId { get; set; }
		public DateTime? DeliveryDate { get; set; }
		public EdoDocFlowStatus? EdoStatus { get; set; }
		public EdoControlReportOrderDeliveryType OrderDeliveryType { get; set; }
		public EdoControlReportAddressTransferType AddressTransferType { get; set; }
	}
}
