using System;
using QS.Project.Journal;
using Vodovoz.Domain.Payments;
namespace Vodovoz.JournalNodes
{
	public class PaymentJournalNode : JournalEntityNodeBase<Payment>
	{
		public override string Title => $"{PaymentNum} основание {PaymentPurpose}";

		public int PaymentNum { get; set; }

		public DateTime Date { get; set; }

		public decimal Total { get; set; }

		public string Orders { get; set; }

		public string Counterparty { get; set; }

		public string Organization { get; set; }

		public string PaymentPurpose { get; set; }

		public PaymentState Status { get; set; }

		public string ProfitCategory { get; set; }
	}
}
