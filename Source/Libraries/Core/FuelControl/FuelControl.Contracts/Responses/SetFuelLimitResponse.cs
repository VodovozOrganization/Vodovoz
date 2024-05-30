using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Responses
{
	/// <summary>
	/// Ответ сервера на запрос создания продуктового лимита
	/// </summary>
	public class SetFuelLimitResponse : ResponseBase
	{
		/// <summary>
		/// Результат создания продуктового лимита
		/// </summary>
		[JsonPropertyName("data")]
		public IEnumerable<string> CreatedLimitsIds { get; set; }
	}
}
