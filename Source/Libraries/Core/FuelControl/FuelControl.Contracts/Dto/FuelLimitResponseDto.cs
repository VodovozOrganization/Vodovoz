using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Лимит топливной карты
	/// </summary>
	public class FuelLimitResponseDto
	{
		/// <summary>
		/// ID лимита
		/// </summary>
		[JsonPropertyName("id")]
		public string Id { get; set; }

		/// <summary>
		/// W4 Id
		/// </summary>
		[JsonPropertyName("w4_id")]
		public string W4Id { get; set; }

		/// <summary>
		/// Id карты
		/// </summary>
		[JsonPropertyName("card_id")]
		public string CardId { get; set; }

		/// <summary>
		/// Id группы карт
		/// </summary>
		[JsonPropertyName("group_id")]
		public string GroupId { get; set; }

		/// <summary>
		/// Id договора
		/// </summary>
		[JsonPropertyName("contract_id")]
		public string ContractId { get; set; }

		/// <summary>
		/// Ограничение по сумме
		/// </summary>
		[JsonPropertyName("sum")]
		public LimitSumResponseDto Sum { get; set; }

		/// <summary>
		/// Ограничение по количеству
		/// </summary>
		[JsonPropertyName("amount")]
		public LimitAmountResponseDto Amount { get; set; }

		/// <summary>
		/// Лимт по времени
		/// </summary>
		[JsonPropertyName("term")]
		public LimitTermResponseDto Term { get; set; }

		/// <summary>
		/// Ограничение по числу транзакций за период
		/// </summary>
		[JsonPropertyName("transactions")]
		public LimitTransactionsResponseDto Transactions { get; set; }

		/// <summary>
		/// Длительность, период времени
		/// </summary>
		[JsonPropertyName("time")]
		public LimitTimePeriodResponseDto TimePeriod { get; set; }

		/// <summary>
		/// Дата последнего изменения
		/// </summary>
		[JsonPropertyName("date")]
		public string LatEditDate { get; set; }

		/// <summary>
		/// Id типа продукта
		/// </summary>
		[JsonPropertyName("productType")]
		public string ProductType { get; set; }

		/// <summary>
		/// Id группы продукта
		/// </summary>
		[JsonPropertyName("productGroup")]
		public string ProductGroup { get; set; }

		/// <summary>
		/// Название типа продукта
		/// </summary>
		[JsonPropertyName("productTypeName")]
		public string ProductTypeName { get; set; }

		/// <summary>
		/// Название группы продукта
		/// </summary>
		[JsonPropertyName("productGroupName")]
		public string ProductGroupName { get; set; }
	}
}
