using BitrixNotificationsSend.Contracts.JsonConverters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Результаты выполнения команд пакетного запроса в Битрикс24
	/// </summary>
	public class CreateDealsBatchResponseResult
	{
		/// <summary>
		/// Результаты успешных команд: "ключ команды - id созданной сделки"
		/// </summary>
		[JsonPropertyName("result")]
		[JsonConverter(typeof(EmptyArrayAsEmptyDictionaryConverter<long>))]
		public Dictionary<string, long> CreatedDeals { get; set; } = new Dictionary<string, long>();

		/// <summary>
		/// Ошибки команд: "ключ команды - данные ошибки"
		/// </summary>
		[JsonPropertyName("result_error")]
		[JsonConverter(typeof(EmptyArrayAsEmptyDictionaryConverter<CreateDealsBatchCommandError>))]
		public Dictionary<string, CreateDealsBatchCommandError> Errors { get; set; } =
			new Dictionary<string, CreateDealsBatchCommandError>();

		/// <summary>
		/// Данные о времени выполнения команд: "ключ команды - данные о времени",
		/// в том числе накопленное операционное время метода и момент сброса операционного бюджета
		/// </summary>
		[JsonPropertyName("result_time")]
		[JsonConverter(typeof(EmptyArrayAsEmptyDictionaryConverter<CreateDealsBatchCommandTime>))]
		public Dictionary<string, CreateDealsBatchCommandTime> CommandsTime { get; set; } =
			new Dictionary<string, CreateDealsBatchCommandTime>();
	}
}
