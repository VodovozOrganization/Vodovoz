using FuelControl.Contracts.Dto;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Contracts.Converters
{
	public interface IFuelCardConverter
	{
		FuelCard ConvertToDomainFuelCard(FuelCardDto fuelCardDto);
	}
}
