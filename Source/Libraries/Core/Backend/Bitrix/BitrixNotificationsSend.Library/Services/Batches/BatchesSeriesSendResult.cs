using System;

namespace BitrixNotificationsSend.Library.Services.Batches
{
	/// <summary>
	/// Результат отправки одной серии пакетов команд в Битрикс24
	/// </summary>
	public class BatchesSeriesSendResult<TItem> : BatchesSendResult<TItem>
	{
		/// <summary>
		/// Максимальное накопленное операционное время метода, сек
		/// </summary>
		public double OperatingSeconds { get; set; }

		/// <summary>
		/// Момент сброса операционного бюджета метода (UTC),
		/// null - если данные не приходили в ответах
		/// </summary>
		public DateTime? OperatingResetAt { get; set; }
	}
}
