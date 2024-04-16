using FuelControl.Contracts.Dto;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Contracts.Converters
{
	public interface ITransactionConverter
	{
		FuelTransaction ConvertToDomainFuelTransaction(TransactionDto transactionDto);
	}
}
