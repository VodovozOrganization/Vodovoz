using FuelControl.Contracts.Dto;
using System;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Converters
{
	public class FuelLimitConverter : IFuelLimitConverter
	{
		public FuelLimit ConvertDtoToDomainFuelLimit(FuelLimitDto fuelLimitDto)
		{
			return new FuelLimit
			{
				LimitId = fuelLimitDto.Id,
				ContractId = fuelLimitDto.ContractId,
				Amount = fuelLimitDto.Amount?.Value,
				Sum = fuelLimitDto.Sum?.Value,
				TransctionsCount = fuelLimitDto.Transactions?.Count ?? 0,
				TransactionsOccured = fuelLimitDto.Transactions?.Occured ?? 0,
				LastEditDate = DateTime.Parse(fuelLimitDto.LatEditDate)
			};
		}
	}
}
