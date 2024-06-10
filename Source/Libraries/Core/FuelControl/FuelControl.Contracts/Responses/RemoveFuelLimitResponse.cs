using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Responses
{
	/// <summary>
	/// Ответ сервера на запрос удаления продуктового лимита
	/// </summary>
	public class RemoveFuelLimitResponse : ResponseBase
	{
		/// <summary>
		/// Результат удаления продуктового лимита
		/// </summary>
		[JsonPropertyName("data")]
		public bool IsRemovalSuccessful { get; set; }
	}
}
