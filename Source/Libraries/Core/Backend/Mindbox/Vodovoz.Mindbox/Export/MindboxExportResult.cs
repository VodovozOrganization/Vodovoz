using System.Collections.Generic;

namespace Vodovoz.Mindbox.Export
{
	/// <summary>
	/// Результат выгрузки контрагентов в Mindbox
	/// </summary>
	public sealed class MindboxExportResult
	{
		/// <summary>
		/// Всего контрагентов в выборке
		/// </summary>
		public int Total { get; set; }

		/// <summary>
		/// Отправлено контрагентов
		/// </summary>
		public int Sent { get; set; }

		/// <summary>
		/// Количество пакетов
		/// </summary>
		public int Batches { get; set; }

		/// <summary>
		/// Ошибки, возникшие при выгрузке
		/// </summary>
		public List<string> Errors { get; } = new List<string>();

		/// <summary>
		/// Признак успешной выгрузки
		/// </summary>
		public bool IsSuccess => Errors.Count == 0;
	}
}
