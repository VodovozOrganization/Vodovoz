using System;

namespace Edo.Problem.Routine.Options
{
	/// <summary>
	/// Настройки воркера обработки проблем с контактом чека.
	/// </summary>
	public class ReceiptContactProblemWorkerOptions
	{
		/// <summary>
		/// Таймаут обрабатываемых проблем.
		/// </summary>
		public TimeSpan ProblemTimeout { get; set; }

		/// <summary>
		/// Интервал работы воркера.
		/// </summary>
		public TimeSpan WorkerInterval { get; set; }

		/// <summary>
		/// Количество попыток повторной обработки до уведомления.
		/// </summary>
		public int RetryAttemptsBeforeNotification { get; set; }
	}
}
