using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Services.Orders
{
	public partial class CreateOrderRequest
	{
		public int CounterpartyId { get; set; }
		public int DeliveryPointId { get; set; }
		public IEnumerable<SaleItem> SaleItems { get; set; }
		public int BottlesReturn { get; set; }
		public DateTime Date { get; set; }
		public int DeliveryScheduleId { get; set; }
		public PaymentType PaymentType { get; set; }
		public PaymentByTerminalSource? PaymentByTerminalSource { get; set; }
		public int? BanknoteForReturn { get; set; }
		public int? TareNonReturnReasonId { get; set; }
	}
}
