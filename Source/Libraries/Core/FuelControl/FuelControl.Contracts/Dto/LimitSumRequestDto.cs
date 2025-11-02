using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Ограничение по сумме для лимита по карте
	/// </summary>
	public class LimitSumRequestDto
	{
		/// <summary>
		/// Валюта
		/// </summary>
		[JsonPropertyName("currency")]
		public string Currency { get; set; }

		/// <summary>
		/// Суммарный размер ограничения
		/// </summary>
		[JsonPropertyName("value")]
		public int Value { get; set; }
	}
}
