using FuelControl.Contracts.Dto;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Responses
{
	/// <summary>
	/// Ответ сервера на запрос получения списка продуктовых лимитов
	/// </summary>
	public class FuelLimitsResponse : ResponseBase
	{
		/// <summary>
		/// Данные множества продуктовых лимитов
		/// </summary>
		[JsonPropertyName("data")]
		public FuelLimitsDataDto FuelLimitsData { get; set; }
	}
}
