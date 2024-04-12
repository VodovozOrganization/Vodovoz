using System;

namespace DatabaseServiceWorker.Options
{
	internal class PowerBiExportOptions
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
		public DateTime StardDate { get; set; }
	}
}
