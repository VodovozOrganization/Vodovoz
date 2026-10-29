using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Ответ Битрикс24 на пакетный запрос batch.json
	/// </summary>
	public class BitrixBatchResponse
	{
		/// <summary>
		/// Результаты выполнения команд пакета
		/// </summary>
		[JsonPropertyName("result")]
		public BitrixBatchResponseResult Result { get; set; }
	}
}
