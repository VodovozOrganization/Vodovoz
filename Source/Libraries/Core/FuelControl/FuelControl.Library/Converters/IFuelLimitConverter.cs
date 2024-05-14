using FuelControl.Contracts.Dto;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Converters
{
	public interface IFuelLimitConverter
	{
		FuelLimit ConvertDtoToDomainFuelLimit(FuelLimitResponseDto fuelLimitDto);
	}
}