using System;

namespace DatabaseServiceWorker.Options
{
	public class FuelTransactionsControlOptions
	{
		/// <summary>
		/// Интервал запуска работы воркера
		/// </summary>
		public TimeSpan ScanInterval { get; set; }

		/// <summary>
		/// День обновления всех предыдущих транзакция с начала месяца
		/// </summary>
		public int SavedTransactionsUpdateDay { get; set; }

		/// <summary>
		/// Минимальное время для запроса транзакций
		/// Например, если значение 5, то до 05:00 транзакции запрашиваться не будут
		/// </summary>
		public int TransactionsDataRequestMinHour { get; set; }

		/// <summary>
		/// Логин пользователя
		/// </summary>
		public string Login { get; set; }

		/// <summary>
		/// Пароль пользователя
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Ключ API пользователя
		/// </summary>
		public string ApiKey { get; set; }
	}
}
