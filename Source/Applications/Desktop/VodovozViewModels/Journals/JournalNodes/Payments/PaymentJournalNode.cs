using System;
using QS.Project.Journal;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Payments;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Payments
{
	public class PaymentJournalNode : JournalEntityNodeBase<Payment>
	{
		public override string Title => $"{PaymentNum} основание {PaymentPurpose}";

		public int PaymentNum { get; set; }
		public DateTime Date { get; set; }
		public decimal Total { get; set; }
		public decimal UnAllocatedSum { get; set; }
		public string Orders { get; set; }
		public string PayerName { get; set; }
		public string OrganizationBank { get; set; }
		public string OrganizationAccountNumber { get; set; }
		public string CounterpartyName { get; set; }
		public string Organization { get; set; }
		public string PaymentPurpose { get; set; }
		public PaymentState? Status { get; set; }
		public string ProfitCategory { get; set; }
		public bool IsManualCreated { get; set; }
	}
}
