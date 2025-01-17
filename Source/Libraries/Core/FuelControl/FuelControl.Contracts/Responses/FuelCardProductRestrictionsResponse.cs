using FuelControl.Contracts.Dto;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Responses
{
	/// <summary>
	/// Ответ сервера на запрос получения списка товарных ограничителей
	/// </summary>
	public class FuelCardProductRestrictionsResponse : ResponseBase
	{
		/// <summary>
		/// Данные множества товарных ограничителей
		/// </summary>
		[JsonPropertyName("data")]
		public FuelCardProductRestrictionsDataDto FuelCardProductRestrictionsData { get; set; }
	}
}
