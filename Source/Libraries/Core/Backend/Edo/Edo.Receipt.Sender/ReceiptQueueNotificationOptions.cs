using System;

namespace Edo.Receipt.Sender
{
	/// <summary>
	/// Интервалы проверки очереди чеков и повторных уведомлений.
	/// </summary>
	public class ReceiptQueueNotificationOptions
	{
		/// <summary>
		/// Интервал проверки очереди чеков.
		/// </summary>
		public TimeSpan WorkerInterval { get; set; }

		/// <summary>
		/// Минимальное время нахождения чека в очереди, после которого он попадает
		/// в проверку статуса и уведомления о зависших чеках.
		/// </summary>
		public TimeSpan MinQueueAge { get; set; }

		/// <summary>
		/// Интервал между уведомлениями об одном чеке.
		/// </summary>
		public TimeSpan RepeatInterval { get; set; }

		/// <summary>
		/// Глубина выборки по времени попадания чека в очередь, в днях.
		/// </summary>
		public int LookbackDays { get; set; }
	}
}
