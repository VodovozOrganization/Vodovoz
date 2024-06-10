using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Лимит топливной карты
	/// </summary>
	public class FuelLimitRequestDto
	{
		/// <summary>
		/// ID лимита
		/// </summary>
		[JsonPropertyName("id")]
		public string Id { get; set; }

		/// <summary>
		/// ID карты
		/// </summary>
		[JsonPropertyName("card_id")]
		public string CardId { get; set; }

		/// <summary>
		/// ID группы карт
		/// </summary>
		[JsonPropertyName("group_id")]
		public string GroupId { get; set; }

		/// <summary>
		/// ID договора
		/// </summary>
		[JsonPropertyName("contract_id")]
		public string ContractId { get; set; }

		/// <summary>
		/// ID группы продукта
		/// </summary>
		[JsonPropertyName("productGroup")]
		public string ProductGroup { get; set; }

		/// <summary>
		/// ID типа продукта
		/// </summary>
		[JsonPropertyName("productType")]
		public string ProductType { get; set; }

		/// <summary>
		/// Ограничение по количеству
		/// </summary>
		[JsonPropertyName("amount")]
		public LimitAmountRequestDto Amount { get; set; }

		/// <summary>
		/// Ограничение по сумме.
		/// </summary>
		[JsonPropertyName("sum")]
		public LimitSumRequestDto Sum { get; set; }

		/// <summary>
		/// Ограничение по времени
		/// </summary>
		[JsonPropertyName("term")]
		public LimitTermRequestDto Term { get; set; }

		/// <summary>
		/// Длительность, период времени
		/// </summary>
		[JsonPropertyName("time")]
		public LimitTimePeriodRequestDto Time { get; set; }

		/// <summary>
		/// Ограничение по числу транзакций за период
		/// </summary>
		[JsonPropertyName("transactions")]
		public LimitTransactionsRequestDto Transactions { get; set; }
	}
}
