using FuelControl.Contracts.Dto;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Converters
{
	public interface IFuelCardConverter
	{
		FuelCard ConvertToDomainFuelCard(FuelCardDto fuelCardDto);
	}
}
