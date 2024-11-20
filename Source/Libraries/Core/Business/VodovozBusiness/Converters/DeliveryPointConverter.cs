using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Domain.Client;

namespace Vodovoz.Converters
{
	public class DeliveryPointConverter : IDeliveryPointConverter
	{
		public DeliveryPointInfoForEdo ConvertDeliveryPointToDeliveryPointInfoForEdo(DeliveryPoint deliveryPoint)
		{
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
