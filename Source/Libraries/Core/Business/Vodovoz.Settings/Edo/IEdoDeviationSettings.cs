using System;

namespace Vodovoz.Settings.Edo
{
	/// <summary>
	/// Настройки сервиса мониторинга отклонений документооборота ЭДО
	/// </summary>
	public interface IEdoDeviationSettings
	{
		/// <summary>
		/// Признак того, что воркеры мониторинга отклонений выполняют работу
		/// </summary>
		bool IsEnabled { get; }

		/// <summary>
		/// Интервал работы воркера регистрации отклонений
		/// </summary>
		TimeSpan RegistrationWorkerInterval { get; }

		/// <summary>
		/// Интервал работы воркера снятия отклонений
		/// </summary>
		TimeSpan ResolvingWorkerInterval { get; }

		/// <summary>
		/// Количество задач, обрабатываемых за один проход
		/// </summary>
		int BatchSize { get; }

		/// <summary>
		/// Дата, с которой отслеживается результат обработки кодов в ГИС МТ
		/// </summary>
		DateTime GisMtTrackingStartDate { get; }
	}
}
