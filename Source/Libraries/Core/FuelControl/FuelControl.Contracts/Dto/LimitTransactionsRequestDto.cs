using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Ограничение по числу транзакций за период для лимита по карте
	/// </summary>
	public class LimitTransactionsRequestDto
	{
		/// <summary>
		/// Количество транзакций по услуге
		/// </summary>
		[JsonPropertyName("count")]
		public int Count { get; set; }
	}
}
