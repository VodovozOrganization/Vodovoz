using System;
using Vodovoz.Core.Data.Clients;

namespace Vodovoz.Core.Data.Orders.OrdersWithoutShipment
{
	public abstract class OrderWithoutShipment
	{
		public int Id { get; set; }
		public DateTime CreatedDate { get; set; }
		public Counterparty Counterparty { get; set; }
		public decimal Sum { get; set; }
	}
}
