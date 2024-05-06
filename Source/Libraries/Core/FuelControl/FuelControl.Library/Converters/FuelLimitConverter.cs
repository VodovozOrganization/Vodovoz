using FuelControl.Contracts.Dto;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Converters
{
	public class FuelLimitConverter : IFuelLimitConverter
	{
		public FuelLimit ConvertDtoToDomainFuelLimit(FuelLimitDto fuelLimitDto)
		{
			return new FuelLimit
			{

			};
		}
	}
}
