using System;

namespace Edo.Problem.Routine.Options
{
	/// <summary>
	/// Настройки воркера, обрабатывающего проблемы по нехватку кодов в пуле
	/// </summary>
	public class OrderEdoCodePoolMissingProblemWorkerOptions
	{
		/// <summary>
		/// Интервал срабатывания воркера
		/// </summary>
		public TimeSpan WorkerInterval { get; set; } = TimeSpan.FromHours(6);

		/// <summary>
		/// Количество попыток
		/// </summary>
		public int MaxAttempts { get; set; } = 4;

		/// <summary>
		/// Количество проблемных задач, обрабатываемых за один раз
		/// </summary>
		public int BatchSize { get; set; } = 50;
	}
}
