using System;

namespace Vodovoz.EntityRepositories.Payments
{
	public class PaymentWriteOffNode
	{
		public int Id { get; set; }
		public int PaymentNumber { get; set; }
		public DateTime Date { get; set; }
		public decimal Sum { get; set; }
		public string Reason { get; set; }
	}
}
