using FuelControl.Contracts.Dto;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Responses
{
	public class ResponseBase
	{
		/// <summary>
		/// Код ошибки, обязателен при любом результате выполнения запроса (200=успех)
		/// </summary>
		[JsonPropertyName("status")]
		public StatusDto Status { get; set; }

		/// <summary>
		/// Время ответа
		/// </summary>
		[JsonPropertyName("timestamp")]
		public int Timestamp { get; set; }
	}
}
