using System;

namespace DatabaseServiceWorker
{
	internal sealed class PowerBiExportOptions
	{
		/// <summary>
		/// Интервал генерации отчёта
		/// </summary>
		internal TimeSpan Interval { get; set; }

		/// <summary>
		/// Путь для экспорта отчёта
		/// </summary>
		internal string ExportPath { get; set; }

		/// <summary>
		/// Начальная дата выборки для отчётов
		/// </summary>
		internal DateTime StartDate { get; set; }

		/// <summary>
		/// Логин для доступа к сетевой папке
		/// </summary>
		internal string Login { get; set; }

		/// <summary>
		/// Пароль для доступа к сетевой папке
		/// </summary>
		internal string Password { get; set; }
	}
}
