using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Ограничение по времени для лимита по карте
	/// </summary>
	public class LimitTermRequestDto
	{
		/// <summary>
		/// Строка из 7 нулей и единиц. 1 – ограничение применяется в этот день, 0 – нет.
		/// </summary>
		[JsonPropertyName("days")]
		public string Days { get; set; }

		/// <summary>
		/// Время обслуживания
		/// </summary>
		[JsonPropertyName("time")]
		public LimitTermTimeDto Time { get; set; }

		/// <summary>
		/// Способ применения ограничения:
		/// 1 – Ограничение применяется всегда (во все указанные дни недели).
		/// 2 – Ограничение применяется только в рабочие дни.
		/// 3 – Ограничение применяется только в выходные и праздничные дни.
		/// </summary>
		[JsonPropertyName("type")]
		public int Type { get; set; }
	}
}
