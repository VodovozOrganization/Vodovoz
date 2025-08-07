using System;

namespace DatabaseServiceWorker.Options
{
	public class ExportTo1cOptions
	{
		/// <summary>
		/// Интервал экспорта
		/// </summary>
		public TimeSpan ExportInterval { get; init; }
		
		/// <summary>
		/// Путь для экспорта
		/// </summary>
		public string ExportPath { get; init; }
		
		/// <summary>
		/// Логин для доступа к сетевой папке
		/// </summary>
		public string Login { get; init; }

		/// <summary>
		/// Пароль для доступа к сетевой папке
		/// </summary>
		public string Password { get; init; }
	}
}
