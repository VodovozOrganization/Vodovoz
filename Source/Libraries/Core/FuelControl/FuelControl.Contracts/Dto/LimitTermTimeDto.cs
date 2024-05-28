using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Время обслуживания по лимиту
	/// </summary>
	public class LimitTermTimeDto
	{
		/// <summary>
		/// От
		/// </summary>
		[JsonPropertyName("from")]
		public string From { get; set; }

		/// <summary>
		/// До
		/// </summary>
		[JsonPropertyName("to")]
		public string To { get; set; }
	}
}
