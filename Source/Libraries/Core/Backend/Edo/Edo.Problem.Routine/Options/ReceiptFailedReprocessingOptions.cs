using System;
using System.Collections.Generic;

namespace Edo.Problem.Routine.Options
{
	/// <summary>
	/// Настройки сервиса и воркера переобработки чеков с ошибкой фискализации
	/// </summary>
	public class ReceiptFailedReprocessingOptions
	{
		/// <summary>
		/// Название секции в файле конфигурации
		/// </summary>
		public const string SectionName = "ReceiptFailedReprocessing";

		/// <summary>
		/// Глубина поиска чеков в днях
		/// </summary>
		public int LookbackDays { get; set; }

		/// <summary>
		/// Минимальный возраст чека с ошибкой в часах для повторной отправки
		/// </summary>
		public int FailedOlderThanHours { get; set; }

		/// <summary>
		/// Максимальное количество попыток автоматической повторной отправки чека в кассу
		/// </summary>
		public int MaxReprocessingAttempts { get; set; }

		/// <summary>
		/// Интервал запуска воркера
		/// </summary>
		public TimeSpan WorkerInterval { get; set; }

		/// <summary>
		/// Часы планового запуска по московскому времени
		/// </summary>
		public int[] ScheduleRunHours { get; set; }

		/// <summary>
		/// Подстроки критических ошибок фискального накопителя, при которых повторная автоматическая отправка блокируется
		/// </summary>
		public IList<string> CriticalFiscalDriveErrors { get; set; }

		/// <summary>
		/// Подстроки критических ошибок валидации реквизитов документа, требующих ручного разбора оператором
		/// </summary>
		public IList<string> CriticalValidationErrors { get; set; }
	}
}
