using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Ответ Битрикс24 на пакетный запрос создания сделок
	/// </summary>
	public class CreateDealsBatchResponse
	{
		/// <summary>
		/// Результаты выполнения команд пакета
		/// </summary>
		[JsonPropertyName("result")]
		public CreateDealsBatchResponseResult Result { get; set; }
	}
}
