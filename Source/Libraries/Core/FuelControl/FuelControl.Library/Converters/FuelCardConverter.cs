using FuelControl.Contracts.Dto;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Converters
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
