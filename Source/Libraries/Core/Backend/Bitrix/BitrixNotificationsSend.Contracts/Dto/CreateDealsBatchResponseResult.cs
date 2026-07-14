using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Результаты выполнения команд пакетного запроса в Битрикс24
	/// </summary>
	public class CreateDealsBatchResponseResult
	{
		/// <summary>
		/// Результаты успешных команд: объект "ключ команды - id созданной сделки".
		/// При отсутствии успешных команд Битрикс24 возвращает пустой массив вместо объекта
		/// </summary>
		[JsonPropertyName("result")]
		public JsonElement CreatedDeals { get; set; }

		/// <summary>
		/// Ошибки команд: объект "ключ команды - данные ошибки (error, error_description)".
		/// При отсутствии ошибок Битрикс24 возвращает пустой массив вместо объекта
		/// </summary>
		[JsonPropertyName("result_error")]
		public JsonElement Errors { get; set; }
	}
}
