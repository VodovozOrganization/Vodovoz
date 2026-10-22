using System;

namespace Vodovoz.Mindbox.Export
{
	/// <summary>
	/// Параметры выгрузки контрагентов в Mindbox
	/// </summary>
	public sealed class MindboxExportOptions
	{
		/// <summary>
		/// Дата создания контрагента, начиная с которой выполняется выгрузка
		/// </summary>
		public DateTime FromDate { get; set; } = new DateTime(2025, 1, 1);

		/// <summary>
		/// Только сформировать CSV, не отправляя в Mindbox
		/// </summary>
		public bool DryRun { get; set; }

		/// <summary>
		/// Путь к файлу, в который нужно сохранить CSV вместо отправки
		/// </summary>
		public string OutputFile { get; set; }

		/// <summary>
		/// Размер пакета строк; если не задан, используется значение из настроек
		/// </summary>
		public int? BatchSize { get; set; }
	}
}
