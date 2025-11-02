using System;
using QS.Project.DB;

namespace DatabaseServiceWorker.PowerBiWorker.Options
{
	public sealed class PowerBiExportOptions
	{
		/// <summary>
		/// Интервал генерации отчёта
		/// </summary>
		public TimeSpan Interval { get; init; }

		/// <summary>
		/// Путь для экспорта отчёта
		/// </summary>
		public string ExportPath { get; init; }

		/// <summary>
		/// Начальная дата выборки для отчётов
		/// </summary>
		public DateTime StartDate { get; init; }

		/// <summary>
		/// Колв-во предыдущих дней для экспорта дата выборки для отчётов
		/// </summary>
		public int NumberOfDaysToExport { get; init; }

		/// <summary>
		/// Логин для доступа к сетевой папке
		/// </summary>
		public string Login { get; set; }

		/// <summary>
		/// Пароль для доступа к сетевой папке
		/// </summary>
		public string Password { get; init; }

		/// <summary>
		/// БД PowerBi
		/// </summary>
		public DatabaseConnectionSettings TargetDataBaseConnectionSettings { get; init; }
	}
}
