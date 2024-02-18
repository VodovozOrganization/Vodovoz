using System;
using Vodovoz.Domain.Client;

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
