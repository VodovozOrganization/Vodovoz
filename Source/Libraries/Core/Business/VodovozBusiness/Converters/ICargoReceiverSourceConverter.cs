using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace VodovozBusiness.Converters
{
	public interface ICargoReceiverSourceConverter
	{
		CargoReceiverSourceType ConvertCargoReceiverSourceToCargoReceiverSourceType(CargoReceiverSource cargoReceiverSource);
	}
}
