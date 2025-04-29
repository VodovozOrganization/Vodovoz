using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Responses
{
	/// <summary>
	/// Ответ сервера на запрос создания товарного ограничителя
	/// </summary>
	public class SetFuelCardProductRestrictionResponse : ResponseBase
	{
		/// <summary>
		/// Результат создания продуктового товарного ограничителя
		/// </summary>
		[JsonPropertyName("data")]
		public IEnumerable<long> CreatedRestrictionsIds { get; set; }
	}
}
