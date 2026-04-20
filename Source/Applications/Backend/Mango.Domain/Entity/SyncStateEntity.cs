using System;

namespace Mango.Domain.Entity
{
	/// <summary>
	/// Сущность для синхронизации времени запроса данных
	/// </summary>
	public class SyncStateEntity
	{
		/// <summary>
		/// Название сервиса для синхронизации
		/// </summary>
		public string Source { get; set; } = string.Empty;
		
		/// <summary>
		/// Последний запуск синхронизации
		/// </summary>
		public DateTime LastProcessedDate { get; set; }
		
		/// <summary>
		/// Обновлен в
		/// </summary>
		public DateTime UpdatedAtDate { get; set; }
	}
}
