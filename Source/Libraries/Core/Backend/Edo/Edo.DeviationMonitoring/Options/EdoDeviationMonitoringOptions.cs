using System;

namespace Edo.DeviationMonitoring.Options
{
	/// <summary>
	/// Настройки сервиса мониторинга отклонений документооборота ЭДО
	/// </summary>
	public class EdoDeviationMonitoringOptions
	{
		/// <summary>
		/// Интервал работы воркера регистрации отклонений
		/// </summary>
		public TimeSpan RegistrationWorkerInterval { get; set; }

		/// <summary>
		/// Интервал работы воркера снятия отклонений
		/// </summary>
		public TimeSpan ResolvingWorkerInterval { get; set; }

		/// <summary>
		/// Количество задач, обрабатываемых за один проход
		/// </summary>
		public int BatchSize { get; set; }

		/// <summary>
		/// Дата, с которой отслеживается результат обработки кодов в ГИС МТ
		/// </summary>
		public DateTime GisMtTrackingStartDate { get; set; }
	}
}
