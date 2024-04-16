using FuelControl.Contracts.Dto;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Contracts.Converters
{
	public class FuelCardConverter : IFuelCardConverter
	{
		public FuelCard ConvertToDomainFuelCard(FuelCardDto fuelCardDto)
		{
			return new FuelCard
			{
				CardId = fuelCardDto.CardId,
				CardNumber = fuelCardDto.Number
			};
		}
	}
}
