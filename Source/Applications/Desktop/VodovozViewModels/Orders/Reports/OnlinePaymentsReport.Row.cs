using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Orders.Reports
{
	public partial class OnlinePaymentsReport
	{
		public class Row
		{
			public DateTime? DeliveryDate { get; internal set; }
			public int OrderId { get; internal set; }
			public string CcounterpartyFullName { get; internal set; }
			public string Address { get; internal set; }
			public int? OnlineOrderId { get; internal set; }
			public int TotalSumFromBank { get; internal set; }
			public decimal OrderTotalSum { get; internal set; }
			public OrderStatus OrderStatus { get; internal set; }
			public string Author { get; internal set; }
			public string PaymentDateTimeOrError { get; internal set; }
			public PaymentType OrderPaymentType { get; internal set; }
			public bool IsFutureOrder { get; internal set; }
			public string NumberAndShop { get; internal set; }
		}
	}
}
