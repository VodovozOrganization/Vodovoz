using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class ExternalCounterpartyMatchingNode
	{
		public int EntityId { get; set; }
		public int? ExternalCounterpartyId { get; set; }
		public int? DeliveryPointCounterpartyId { get; set; }
		public int? PhoneId { get; set; }
		public string DeliveryPointCounterpartyName { get; set; }
		public DateTime? DeliveryPointCounterpartyLastOrderDate { get; set; }
		public string EntityType { get; set; }
		public PersonType PersonType { get; set; }
		public string Title { get; set; }
		public DateTime? LastOrderDate { get; set; }
	}
}
