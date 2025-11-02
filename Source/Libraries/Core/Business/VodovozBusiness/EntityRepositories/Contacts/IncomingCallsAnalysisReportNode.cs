using System;

namespace Vodovoz.EntityRepositories
{
	public class IncomingCallsAnalysisReportNode
	{
		public string PhoneDigitsNumber { get; set; }
		public int? CounterpartyId { get; set; }
		public int? DeliveryPointId { get; set; }
		public int? LastOrderId { get; set; }
		public DateTime? LastOrderDeliveryDate { get; set; }
	}
}
