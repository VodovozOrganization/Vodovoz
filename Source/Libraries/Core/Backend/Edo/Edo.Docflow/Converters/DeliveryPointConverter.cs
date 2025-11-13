using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace Edo.Docflow.Converters
{
	public class DeliveryPointConverter : IDeliveryPointConverter
	{
		public DeliveryPointInfoForEdo ConvertDeliveryPointToDeliveryPointInfoForEdo(DeliveryPointEntity deliveryPoint)
		{
			if(deliveryPoint is null)
			{
				return null;
			}
			
			var deliveryPointInfo = new DeliveryPointInfoForEdo
			{
				Id = deliveryPoint.Id,
				CounterpartyId = deliveryPoint.Counterparty.Id,
				ShortAddress = deliveryPoint.ShortAddress,
				KPP = deliveryPoint.KPP
			};

			return deliveryPointInfo;
		}
	}
}
