namespace FuelControl.Library
{
	/// <summary>
	/// Наименования клиентов HttpClient для работы с API Газпрома.
	/// Используются при регистрации клиентов в DI контейнере и при запросе клиентов из него
	/// </summary>
	public static class GazpromHttpClientNames
	{
		/// <summary>
		/// Клиент для работы с API Газпрома с настройками по умолчанию
		/// </summary>
		public const string Default = "GazpromApi";

		/// <summary>
		/// Клиент для работы с API Газпрома с увеличенным таймаутом
		/// </summary>
		public const string WithTimeout = "GazpromApiTimed";
	}
}
