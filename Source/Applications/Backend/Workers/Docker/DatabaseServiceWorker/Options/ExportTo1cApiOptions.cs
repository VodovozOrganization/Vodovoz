using System;

namespace DatabaseServiceWorker.Options
{
	/// <summary>
	/// Опции Api 1c
	/// </summary>
	public class ExportTo1cApiOptions
	{
		/// <summary>
		/// Интервал экспорта
		/// </summary>
		public TimeSpan ExportInterval { get; set; }

		/// <summary>
		/// Адрес для отправки изменений по безналу
		/// </summary>
		public string CashlessChangesApiUri { get; set; }

		/// <summary>
		/// Со скольки часов можно выполнять экспорт
		/// </summary>
		public int DoExportFromHour { get; set; }

		/// <summary>
		/// До скольки часов можно выполнять экспорт
		/// </summary>
		public int DoExportToHour { get; set; }
		
		/// <summary>
		/// Логин 1с апи
		/// </summary>
		public string Login { get; set; }
		
		/// <summary>
		/// Пароль 1с апи
		/// </summary>
		public string Password { get; set; }
	}
}
