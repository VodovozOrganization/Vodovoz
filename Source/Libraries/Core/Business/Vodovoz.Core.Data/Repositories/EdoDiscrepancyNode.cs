using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Data.Repositories
{
	public class EdoDiscrepancyNode
	{
		public int OrderId { get; set; }
		public DateTime DeliveryDate { get; set; }
		public OrderStatus OrderStatus { get; set; }
		public int DocumentEdoTaskId { get; set; }
		public int ErpDocflowId { get; set; }
		public DateTime ErpDocflowTime { get; set; }
		public EdoDocumentStatus ErpDocflowStatus { get; set; }
		public Guid TaxcomDocflowId { get; set; }
		public EdoDocFlowStatus TaxcomDocflowStatus { get; set; }
		public DateTime TaxcomStatusTime { get; set; }
		public string CancellationOfferMessage { get; set; }
		public string CorrectionNoticeMessage { get; set; }
		public string DiscrepancyMessage { get; set; }
	}
}
