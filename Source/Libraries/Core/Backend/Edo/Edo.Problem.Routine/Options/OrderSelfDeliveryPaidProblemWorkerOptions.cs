using System;

namespace Edo.Problem.Routine.Options
{
	/// <summary>
	/// Настройки воркера, обрабатывающего проблемы с оплатой при самовывозе
	/// </summary>
	public class OrderSelfDeliveryPaidProblemWorkerOptions
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
