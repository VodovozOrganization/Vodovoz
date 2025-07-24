using System;

namespace DatabaseServiceWorker.Options
{
	public class ExportTo1cOptions
	{
		/// <summary>
		/// Интервал экспорта
		/// </summary>
		public TimeSpan ExportInterval { get; set; }
	}
}
