using FuelControl.Contracts.Dto;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Converters
{
	public interface IFuelLimitConverter
	{
		FuelLimit ConvertResponseDtoToFuelLimit(FuelLimitResponseDto fuelLimitDto);
		FuelLimitRequestDto ConvertFuelLimitToRequestDto(FuelLimit fuelLimit, string literUnitId, string rubleCurrencyId);
	}
}
