using System;

namespace Edo.Withdrawal.Routine.Options
{
	/// <summary>
	/// Настройки работы воркера
	/// </summary>
	public class WithdrawalRoutineOptions
	{
		/// <summary>
		/// Интервал
		/// </summary>
		public TimeSpan Interval { get; set; }
	}
}
