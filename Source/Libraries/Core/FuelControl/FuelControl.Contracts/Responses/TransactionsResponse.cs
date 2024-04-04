using FuelControl.Contracts.Dto;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Responses
{
	/// <summary>
	/// Ответ сервера на запрос получения транзакций
	/// </summary>
	public class TransactionsResponse : ResponseBase
	{
		/// <summary>
		/// Данные множества транзакций
		/// </summary>
		[JsonPropertyName("data")]
		public TransactionsDataDto TransactionsData { get; set; }
	}
}
