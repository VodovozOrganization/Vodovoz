using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Данные множества транзакций выдачи топлива
	/// </summary>
	public class TransactionsDataDto
	{
		/// <summary>
		/// Количество транзакций
		/// </summary>
		[JsonPropertyName("total_count")]
		public int TransactionsCount { get; set; }

		/// <summary>
		/// Транзакции выдачи топлива
		/// </summary>
		[JsonPropertyName("result")]
		public IEnumerable<TransactionDto> Transactions { get; set; }
	}
}
