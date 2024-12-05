using System;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges
{
	public class OrderChangesReportRow
	{
		public int RowNumber { get; set; }
		public string Counterparty { get; set; }
		public string DriverPhoneComment { get; set; }
		public DateTime? PaymentDate { get; set; }
		public int OrderId { get; set; }
		public decimal OrderSum { get; set; }
		public DateTime DeliveryDate { get; set; }
		public DateTime ChangeTime { get; set; }
		public string Nomenclature { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
		public string Driver { get; set; }
		public string Author { get; set; }
	}
}
