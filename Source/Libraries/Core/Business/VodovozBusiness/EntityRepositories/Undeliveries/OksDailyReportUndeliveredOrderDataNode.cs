using System;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Undeliveries
{
	public class OksDailyReportUndeliveredOrderDataNode
	{
		public int UndeliveredOrderId { get; set; }
		public int? NewOrderId { get; set; }
		public GuiltyTypes GuiltySide { get; set; }
		public int? GuiltySubdivisionId { get; set; }
		public string GuiltySubdivisionName { get; set; }
		public UndeliveryStatus UndeliveryStatus { get; set; }
		public TransferType? TransferType { get; set; }
		public DateTime? OldOrderDeliveryDate { get; set; }
		public string ClientName { get; set; }
		public string Reason { get; set; }
		public string Drivers { get; set; }
		public string ResultComments { get; set; }
	}
}
