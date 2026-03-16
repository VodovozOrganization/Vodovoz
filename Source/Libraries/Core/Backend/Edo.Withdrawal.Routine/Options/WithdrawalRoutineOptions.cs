using System;

namespace Edo.Withdrawal.Routine.Options
{
	/// <summary>
	/// Настройки работы воркера
	/// </summary>
	public class WithdrawalRoutineOptions
	{
		/// <summary>
		/// Интервал выполнения воркера для проверки просроченных документооборотов и создания заявок на вывод кодов из оборота
		/// </summary>
		public TimeSpan TimedOutDocumentsWorkerInterval { get; set; }

		/// <summary>
		/// Интервал выполнения воркера для обновления статусов документов в ЧЗ
		/// </summary>
		public TimeSpan TrueMarkDocumentsStatusUpdateWorkerInterval { get; set; }
	}
}
