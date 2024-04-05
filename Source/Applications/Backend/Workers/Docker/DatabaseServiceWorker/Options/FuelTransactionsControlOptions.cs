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
		/// Время жизни сессии после авторизации
		/// </summary>
		public int SessionLifetimeInDays { get; set; }

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

		/// <summary>
		/// Id контракта организации на которую оформлены карты
		/// </summary>
		public string OrganizationContractId { get; set; }

		/// <summary>
		/// Адрес API сервера
		/// </summary>
		public string BaseAddress { get; set; }
	}
}
