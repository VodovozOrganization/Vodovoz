using FuelControl.Contracts.Dto;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Converters
{
	public class TransactionConverter
	{
		public FuelTransaction ConvertToDomainFuelTransaction(TransactionDto transactionDto)
		{
			return new FuelTransaction
			{
				TransactionId = transactionDto.Id,
				TransactionDate = transactionDto.TransactionDate,
				CardId = transactionDto.CardId,
				SalePointId = transactionDto.SalePointId,
				ProductId = transactionDto.ProductId,
				ProductCategoryId = transactionDto.ProductCategoryId,
				ProductItemsCount = transactionDto.Quantity,
				PricePerItem = transactionDto.Price,
				TotalSum = transactionDto.Sum,
				CardNumber = transactionDto.CardNumber
			};
		}
	}
}
