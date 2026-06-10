using System;

namespace Edo.Problem.Routine.Options
{
	/// <summary>
	/// Настройки воркера, обрабатывающего проблемы со статусом заказа
	/// </summary>
	public class OrderStatusProblemWorkerOptions
	{
		/// <summary>
		/// Таймаут обрабатываемых проблем
		/// </summary>
		public TimeSpan ProblemTimeout { get; set; }

		/// <summary>
		/// Интервал работы воркера
		/// </summary>
		public TimeSpan WorkerInterval { get; set; }
	}
}
