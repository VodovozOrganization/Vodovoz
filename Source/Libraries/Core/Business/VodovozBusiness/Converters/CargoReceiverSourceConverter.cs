using System;
using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace VodovozBusiness.Converters
{
	public class CargoReceiverSourceConverter : ICargoReceiverSourceConverter
	{
		public CargoReceiverSourceType ConvertCargoReceiverSourceToCargoReceiverSourceType(CargoReceiverSource cargoReceiverSource)
		{
			switch(cargoReceiverSource)
			{
				case CargoReceiverSource.FromCounterparty:
					return CargoReceiverSourceType.FromCounterparty;
				case CargoReceiverSource.FromDeliveryPoint:
					return CargoReceiverSourceType.FromDeliveryPoint;
				case CargoReceiverSource.Special:
					return CargoReceiverSourceType.Special;
				default:
					throw new ArgumentOutOfRangeException(nameof(cargoReceiverSource), cargoReceiverSource, null);
			}
		}
	}
}
