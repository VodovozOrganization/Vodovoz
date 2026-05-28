using System;

namespace Edo.Problem.Routine.Options
{
	/// <summary>
	/// Настройки воркера, обрабатывающего проблемы с отправкой фискальных документов в ЭДО
	/// </summary>
	public class FiscalDocumentSendErrorProblemWorkerOptions
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
