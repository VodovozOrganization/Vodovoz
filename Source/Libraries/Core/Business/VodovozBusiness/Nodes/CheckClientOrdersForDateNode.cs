using System;
using Vodovoz.Core.Domain.Clients;

namespace VodovozBusiness.Nodes
{
	public class CheckClientOrdersForDateNode
	{
		public Source Source { get; set; }
		public Guid? ExternalCounterpartyId { get; set; }
		public DateTime DeliveryDate { get; set; }
		public int? CounterpartyId { get; set; }
		public int? DeliveryPointId { get; set; }

		public static CheckClientOrdersForDateNode Create(
			Source source,
			Guid? externalCounterpartyId,
			DateTime deliveryDate,
			int? counterpartyId,
			int? deliveryPointId
		)
		{
			return new CheckClientOrdersForDateNode
			{
				Source = source,
				ExternalCounterpartyId = externalCounterpartyId,
				DeliveryDate = deliveryDate,
				CounterpartyId = counterpartyId,
				DeliveryPointId = deliveryPointId
			};
		}
	}
}
