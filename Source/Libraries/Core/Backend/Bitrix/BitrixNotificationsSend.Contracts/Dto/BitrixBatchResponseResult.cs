using BitrixNotificationsSend.Contracts.JsonConverters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Результаты выполнения команд пакетного запроса в Битрикс24
	/// </summary>
	public class BitrixBatchResponseResult
	{
		/// <summary>
		/// Результаты успешных команд: "ключ команды - id созданной сущности"
		/// </summary>
		[JsonPropertyName("result")]
		[JsonConverter(typeof(EmptyArrayAsEmptyDictionaryConverter<long>))]
		public Dictionary<string, long> SuccessfulCommands { get; set; } = new Dictionary<string, long>();

		/// <summary>
		/// Ошибки команд: "ключ команды - данные ошибки"
		/// </summary>
		[JsonPropertyName("result_error")]
		[JsonConverter(typeof(EmptyArrayAsEmptyDictionaryConverter<BitrixBatchCommandError>))]
		public Dictionary<string, BitrixBatchCommandError> Errors { get; set; } =
			new Dictionary<string, BitrixBatchCommandError>();

		/// <summary>
		/// Данные о времени выполнения команд: "ключ команды - данные о времени",
		/// в том числе накопленное операционное время метода и момент сброса операционного бюджета
		/// </summary>
		[JsonPropertyName("result_time")]
		[JsonConverter(typeof(EmptyArrayAsEmptyDictionaryConverter<BitrixBatchCommandTime>))]
		public Dictionary<string, BitrixBatchCommandTime> CommandsTime { get; set; } =
			new Dictionary<string, BitrixBatchCommandTime>();
	}
}
