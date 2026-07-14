using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Запрос на пакетное создание сделок в битриксе
	/// </summary>
	public class CreateDealsBatchRequest
	{
		/// <summary>
		/// Максимальное количество команд в одном пакетном запросе, ограничение Битрикс24
		/// </summary>
		public const int MaxCommandsCount = 50;

		/// <summary>
		/// Признак остановки выполнения пакета при ошибке в одной из команд:
		/// 0 - продолжать выполнение остальных команд, 1 - остановить
		/// </summary>
		[JsonPropertyName("halt")]
		public int Halt => 0;

		/// <summary>
		/// Команды создания сделок: ключ команды - строка вызова crm.deal.add с параметрами полей сделки
		/// </summary>
		[JsonPropertyName("cmd")]
		public IDictionary<string, string> Commands { get; set; }
	}
}
