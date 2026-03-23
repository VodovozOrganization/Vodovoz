namespace CustomerNotifications.Publisher.Cache
{
	/// <summary>
	/// Настройки подключения к Garnet/Redis, включая URL, пароль и время жизни ключей.
	/// </summary>
	public class GarnetConnection
	{
		/// <summary>
		/// Адрес сервера Garnet/Redis.
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// Пароль для подключения.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Строка подключения, сформированная на основе URL и пароля.
		/// </summary>
		public string ConnectionString => $"{Url},password={Password}";
	}
}
