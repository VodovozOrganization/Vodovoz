using System;

namespace Edo.Problem.Routine.Options
{
	/// <summary>
	/// Настройки воркера, обрабатывающего отложенную ночную отправку чеков
	/// </summary>
	public class ReceiptNightSendProblemWorkerOptions
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
