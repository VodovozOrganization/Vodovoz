using System;
using System.Collections.Generic;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Результат пакетного запроса batch.json в Битрикс24
	/// </summary>
	public class BitrixBatchResult
	{
		/// <summary>
		/// Ключи успешно выполненных команд
		/// </summary>
		public IList<string> SuccessfulCommandKeys { get; set; } = new List<string>();

		/// <summary>
		/// Ошибки выполнения отдельных команд пакета
		/// </summary>
		public IList<BitrixBatchItemError> Errors { get; set; } = new List<BitrixBatchItemError>();

		/// <summary>
		/// Максимальное накопленное операционное время метода из ответа Битрикс24, сек
		/// </summary>
		public double OperatingSeconds { get; set; }

		/// <summary>
		/// Момент сброса операционного бюджета метода (UTC) из ответа Битрикс24,
		/// null - если данные не пришли в ответе
		/// </summary>
		public DateTime? OperatingResetAt { get; set; }
	}
}
