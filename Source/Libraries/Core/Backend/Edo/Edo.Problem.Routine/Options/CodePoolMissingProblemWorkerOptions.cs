using System;

namespace Edo.Problem.Routine.Options
{
	/// <summary>
	/// Настройки воркера, обрабатывающего проблемы по нехватку кодов в пуле
	/// </summary>
	public class CodePoolMissingProblemWorkerOptions
	{
		/// <summary>
		/// Интервал срабатывания воркера
		/// </summary>
		public TimeSpan WorkerInterval { get; set; }

		/// <summary>
		/// Количество попыток
		/// </summary>
		public int MaxAttempts { get; set; }

		/// <summary>
		/// Количество проблемных задач, обрабатываемых за один раз
		/// </summary>
		public int BatchSize { get; set; }

		/// <summary>
		/// Интервал между попытками обработки проблемной задачи в часах
		/// </summary>
		public int RetryIntervalHours { get; set; }
	}
}
