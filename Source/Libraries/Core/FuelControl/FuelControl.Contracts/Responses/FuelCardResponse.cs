using FuelControl.Contracts.Dto;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Responses
{
	/// <summary>
	/// Ответ сервера на запрос получения списка топливных карт
	/// </summary>
	public class FuelCardResponse : ResponseBase
	{
		/// <summary>
		/// Данные множества топливных карт
		/// </summary>
		[JsonPropertyName("data")]
		public FuelCardsDataDto FuelCardsData { get; set; }
	}
}
