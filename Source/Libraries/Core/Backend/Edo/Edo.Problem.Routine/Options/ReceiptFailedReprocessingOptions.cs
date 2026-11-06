using System;
using System.Collections.Generic;

namespace Edo.Problem.Routine.Options
{
	/// <summary>
	/// Настройки сервиса и воркера переобработки чеков с ошибкой фискализации
	/// </summary>
	public class ReceiptFailedReprocessingOptions
	{
		public const string SectionName = "ReceiptFailedReprocessing";

		/// <summary>
		/// Глубина поиска чеков в днях (по умолчанию 7)
		/// </summary>
		public int LookbackDays { get; set; } = 7;

		/// <summary>
		/// Минимальный возраст чека с ошибкой в часах для повторной отправки (по умолчанию 24)
		/// </summary>
		public int FailedOlderThanHours { get; set; } = 24;

		/// <summary>
		/// Интервал запуска воркера (по умолчанию 30 минут)
		/// </summary>
		public TimeSpan WorkerInterval { get; set; } = TimeSpan.FromMinutes(30);

		/// <summary>
		/// Часы планового запуска по московскому времени (по умолчанию 8:00 и 17:00)
		/// </summary>
		public int[] ScheduleRunHours { get; set; } = new[] { 8, 17 };

		/// <summary>
		/// Тестовый режим имитации (заглушка: без реальной отправки чека в МодульКассу)
		/// </summary>
		public bool DryRun { get; set; } = false;
	}
}
