using FuelControl.Contracts.Dto;
using System;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Converters
{
	public class FuelLimitConverter : IFuelLimitConverter
	{
		public FuelLimit ConvertResponseDtoToFuelLimit(FuelLimitResponseDto fuelLimitDto)
		{
			return new FuelLimit
			{
				LimitId = fuelLimitDto.Id,
				CardId = fuelLimitDto.CardId,
				ContractId = fuelLimitDto.ContractId,
				Amount = fuelLimitDto.Amount?.Value,
				UsedAmount = fuelLimitDto.Amount?.Used,
				Sum = fuelLimitDto.Sum?.Value,
				UsedSum = fuelLimitDto.Sum?.Used,
				TransctionsCount = fuelLimitDto.Transactions?.Count ?? 0,
				TransactionsOccured = fuelLimitDto.Transactions?.Occured ?? 0,
				LastEditDate = string.IsNullOrWhiteSpace(fuelLimitDto.LatEditDate) ? (DateTime?)default : DateTime.ParseExact(fuelLimitDto.LatEditDate, "MM/dd/yyyy HH:mm:ss", null)
			};
		}

		public FuelLimitRequestDto ConvertFuelLimitToRequestDto(FuelLimit fuelLimit, string literUnitId, string rubleCurrencyId)
		{
			var requestDto = new FuelLimitRequestDto
			{
				CardId = fuelLimit.CardId,
				ContractId = fuelLimit.ContractId,
				ProductGroup = fuelLimit.ProductGroup,
				ProductType = fuelLimit.ProductType,
				Term = new LimitTermRequestDto { Type = (int)fuelLimit.TermType },
				Time = new LimitTimePeriodRequestDto { Number = fuelLimit.Period, Type = (int)fuelLimit.PeriodUnit },
				Transactions = new LimitTransactionsRequestDto { Count = fuelLimit.TransctionsCount }
			};

			if(fuelLimit.Amount.HasValue)
			{
				if(string.IsNullOrWhiteSpace(literUnitId))
				{
					throw new ArgumentException($"'{nameof(literUnitId)}' cannot be null or whitespace.", nameof(literUnitId));
				}

				requestDto.Amount = new LimitAmountRequestDto
				{
					Unit = literUnitId,
					Value = (int)fuelLimit.Amount.Value
				};
			}

			if(fuelLimit.Sum.HasValue)
			{
				if(string.IsNullOrWhiteSpace(rubleCurrencyId))
				{
					throw new ArgumentException($"'{nameof(rubleCurrencyId)}' cannot be null or whitespace.", nameof(rubleCurrencyId));
				}

				requestDto.Sum = new LimitSumRequestDto
				{
					Currency = rubleCurrencyId,
					Value = (int)fuelLimit.Sum.Value
				};
			}

			return requestDto;
		}
	}
}
