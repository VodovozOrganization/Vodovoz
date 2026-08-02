using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Пакетный запрос batch.json в Битрикс24
	/// </summary>
	public class BitrixBatchRequest
	{
		/// <summary>
		/// Признак остановки выполнения пакета при ошибке в одной из команд:
		/// 0 - продолжать выполнение остальных команд, 1 - остановить
		/// </summary>
		[JsonPropertyName("halt")]
		public int Halt => 0;

		/// <summary>
		/// Команды пакета: ключ команды - строка вызова REST-метода с параметрами,
		/// не более <see cref="BitrixApiLimits.MaxBatchCommandsCount"/> команд
		/// </summary>
		[JsonPropertyName("cmd")]
		public IDictionary<string, string> Commands { get; set; }
	}
}
