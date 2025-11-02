using System;

namespace DatabaseServiceWorker.Options
{
	public class ClearFastDeliveryAvailabilityHistoryOptions
	{
		/// <summary>
		/// Интервал проверки в базе наличия неудаленных записей истории проверки
		/// </summary>
		public TimeSpan ScanInterval { get; set; }

		/// <summary>
		/// Таймаут запроса удаления записей из БД
		/// </summary>
		public TimeSpan DeleteQueryTimeout { get; set; }
	}
}
