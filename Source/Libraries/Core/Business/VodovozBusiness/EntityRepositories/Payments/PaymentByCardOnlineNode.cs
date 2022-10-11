using System;

namespace Vodovoz.EntityRepositories.Payments
{
	public class PaymentByCardOnlineNode
	{
		public int Number { get; set; }
		public decimal Sum { get; set; }
		public DateTime Date { get; set; }
	}
}