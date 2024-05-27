using System;
using System.Collections.Generic;
using Vodovoz.Core.Data.Clients;

namespace Vodovoz.Core.Data.Orders
{
	public class Order
	{
		public int Id { get; set; }
		public decimal OrderSum { get; set; }
		public int? CounterpartyExternalOrderId { get; set; }
		public DateTime DeliveryDate { get; set; }
		public DateTime CreationDate { get; set; }
		public CounterpartyContract Contract { get; set; }
		public Counterparty Counterparty { get; set; }
		public DeliveryPoint DeliveryPoint { get; set; }
		public IList<OrderItem> OrderItems { get; set; }
	}
}
