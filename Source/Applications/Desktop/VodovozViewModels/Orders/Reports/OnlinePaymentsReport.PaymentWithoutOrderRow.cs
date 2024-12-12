using System;

namespace Vodovoz.ViewModels.Orders.Reports
{
	public partial class OnlinePaymentsReport
	{
		public class PaymentWithoutOrderRow
		{
			public DateTime DateTime { get; internal set; }
			public int Number { get; internal set; }
			public string Shop { get; internal set; }
			public decimal Sum { get; internal set; }
			public string Email { get; internal set; }
			public string Phone { get; internal set; }
			public string CounterpartyFullName { get; internal set; }
		}
	}
}
