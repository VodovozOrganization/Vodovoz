using System;
using Vodovoz.Core.Domain.Documents;
using static Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters.IncludeExcludeBookkeepingReportsFilterFactory;
using ClosedXML.Report.Utils;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl
{
	public class EdoControlReportOrderData
	{
		public int OrderId { get; set; }
		public DateTime DeliveryDate { get; set; }
		public int ClientId { get; set; }
		public string ClientName { get; set; }
		public int? RouteListId { get; set; }
		public int? EdoContainerId { get; set; }
		public EdoDocFlowStatus? OldEdoDocflowStatus { get; set; }
		public EdoDocumentStatus? NewEdoDocflowStatus { get; set; }
		public EdoControlReportOrderDeliveryType OrderDeliveryType { get; set; }
		public EdoControlReportAddressTransferType AddressTransferType { get; set; }
		public EdoControlReportDocFlowStatus EdoStatus =>
			OldEdoDocflowStatus is null
			? NewEdoDocflowStatus is null
				? EdoControlReportDocFlowStatus.Unsended
				: NewEdoDocflowStatus.Value.ToString().ToEnum<EdoControlReportDocFlowStatus>()
			: OldEdoDocflowStatus.Value.ToString().ToEnum<EdoControlReportDocFlowStatus>();
	}
}
