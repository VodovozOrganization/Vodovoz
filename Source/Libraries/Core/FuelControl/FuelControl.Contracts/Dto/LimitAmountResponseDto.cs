using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Ограничение по количеству для лимита по карте
	/// </summary>
	public class LimitAmountResponseDto
	{
		/// <summary>
		/// Единица измерения
		/// </summary>
		[JsonPropertyName("unit")]
		public string Unit { get; set; }

		/// <summary>
		/// Суммарное количество ограничения
		/// </summary>
		[JsonPropertyName("value")]
		public decimal Value { get; set; }

		/// <summary>
		/// Использованное количество ограничения
		/// </summary>
		[JsonPropertyName("used")]
		public decimal Used { get; set; }
	}
}
