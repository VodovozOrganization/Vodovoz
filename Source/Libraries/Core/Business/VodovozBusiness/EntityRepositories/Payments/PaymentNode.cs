using System;

namespace Vodovoz.EntityRepositories.Payments
{
	public class PaymentNode
	{
		public int PaymentNum { get; set; }
		public DateTime PaymentDate { get; set; }
		public decimal PaymentSum { get; set; }
		public bool IsManuallyCreated { get; set; }
		public string PayerName { get; set; }
		public int CounterpartyId { get; set; }
		public string CounterpartyInn { get; set; }
		public string CounterpartyName { get; set; }
		public string CounterpartyFullName { get; set; }
		public string PaymentPurpose { get; set; }
	}
}
