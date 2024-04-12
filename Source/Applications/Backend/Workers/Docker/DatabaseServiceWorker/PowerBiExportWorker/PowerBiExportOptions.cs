using System;

namespace DatabaseServiceWorker
{
	internal sealed class PowerBiExportOptions
	{
		/// <summary>
		/// Интервал генерации отчёта
		/// </summary>
		public TimeSpan Interval { get; set; }

		/// <summary>
		/// Путь для экспорта отчёта
		/// </summary>
		public string ExportPath { get; set; }

		/// <summary>
		/// Начальная дата выборки для отчётов
		/// </summary>
		public DateTime StartDate { get; set; }

		/// <summary>
		/// Логин для доступа к сетевой папке
		/// </summary>
		public string Login { get; set; }

		/// <summary>
		/// Пароль для доступа к сетевой папке
		/// </summary>
		public string Password { get; set; }
	}
}
