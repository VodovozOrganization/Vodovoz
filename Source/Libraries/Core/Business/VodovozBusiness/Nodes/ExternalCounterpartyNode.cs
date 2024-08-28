using System;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Nodes
{
	public class ExternalCounterpartyNode
	{
		public int Id { get; set; }
		public Guid ExternalCounterpartyId { get; set; }
		public string Phone { get; set; }
		public CounterpartyFrom CounterpartyFrom { get; set; }
	}
}
