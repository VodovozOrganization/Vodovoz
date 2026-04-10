namespace CustomerPushNotifications.Transport
{
	/// <summary>
	/// Настройки транспорта для отправки уведомлений через RabbitMQ.
	/// </summary>
	public class CustomerNotificationTransportSettings
	{
		/// <summary>
		/// Адрес хоста RabbitMQ.
		/// </summary>
		public string Host { get; set; }

		/// <summary>
		/// Порт подключения к RabbitMQ.
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		/// Виртуальный хост RabbitMQ.
		/// </summary>
		public string VirtualHost { get; set; }

		/// <summary>
		/// Имя пользователя для подключения.
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// Пароль для подключения.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Использовать ли SSL при подключении.
		/// </summary>
		public bool UseSSL { get; set; }

		/// <summary>
		/// Разрешённые ошибки SSL-политики, задаваемые строкой перечисления <see cref="System.Net.Security.SslPolicyErrors"/>.
		/// </summary>
		public string AllowSslPolicyErrors { get; set; }
	}
}
