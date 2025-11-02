using FuelControl.Contracts.Dto;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Converters
{
	public interface ITransactionConverter
	{
		FuelTransaction ConvertToDomainFuelTransaction(TransactionDto transactionDto);
	}
}
