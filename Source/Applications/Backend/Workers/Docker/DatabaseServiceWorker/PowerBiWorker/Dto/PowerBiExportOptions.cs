using QS.Project.DB;
using System;

namespace DatabaseServiceWorker.PowerBiWorker.Dto
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
		/// Колв-во предыдущих дней для экспорта дата выборки для отчётов
		/// </summary>
		public int NumberOfDaysToExport { get; set; }

		/// <summary>
		/// Логин для доступа к сетевой папке
		/// </summary>
		public string Login { get; set; }

		/// <summary>
		/// Пароль для доступа к сетевой папке
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// БД PowerBi
		/// </summary>
		public DatabaseConnectionSettings TargetDataBase { get; set; }

	}
}
