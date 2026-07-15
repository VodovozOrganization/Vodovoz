using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Данные ошибки выполнения команды пакетного запроса из ответа Битрикс24
	/// </summary>
	public class BitrixBatchCommandError
	{
		/// <summary>
		/// Код (текст) ошибки, например OPERATION_TIME_LIMIT
		/// </summary>
		[JsonPropertyName("error")]
		public string Error { get; set; }

		/// <summary>
		/// Описание ошибки
		/// </summary>
		[JsonPropertyName("error_description")]
		public string ErrorDescription { get; set; }
	}
}
