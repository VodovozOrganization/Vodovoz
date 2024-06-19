using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Ограничение по сумме для лимита по карте
	/// </summary>
	public class LimitSumResponseDto
	{
		/// <summary>
		/// Валюта
		/// </summary>
		[JsonPropertyName("currency")]
		public string Currency { get; set; }

		/// <summary>
		/// Сокращенное название валюты
		/// </summary>
		[JsonPropertyName("currencyName")]
		public string CurrencyName { get; set; }

		/// <summary>
		/// Суммарный размер ограничения
		/// </summary>
		[JsonPropertyName("value")]
		public decimal Value { get; set; }

		/// <summary>
		/// Использованный объем ограничения
		/// </summary>
		[JsonPropertyName("used")]
		public decimal Used { get; set; }
	}
}
