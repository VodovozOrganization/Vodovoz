using System;
using Vodovoz.Core.Data.Clients;
using Vodovoz.Core.Data.Organizations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.Orders.OrdersWithoutShipment
{
	
	public abstract class OrderWithoutShipmentInfo
	{
		public int Id { get; set; }
		public DateTime CreationDate { get; set; }
		public Organization Organization { get; set; }
		public Counterparty Counterparty { get; set; }
		public decimal Sum { get; set; }
		public virtual string FileName => $"Счёт № Ф-{Id} от {CreationDate:d}";
		public abstract OrderDocumentType Type { get; }
	}
}
