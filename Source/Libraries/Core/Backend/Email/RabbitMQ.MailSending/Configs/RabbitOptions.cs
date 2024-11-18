namespace CustomerAppsApi.Library.Configs
{
	/// <summary>
	/// Настройки для очередей(rabbitMq)
	/// </summary>
	public class RabbitOptions
	{
		/// <summary>
		/// Секция конфига, где находятся настройки
		/// </summary>
		public const string Path = nameof(RabbitOptions);
		/// <summary>
		/// Exchange для сообщений по отправке писем
		/// </summary>
		public string EmailSendExchange { get; set; }
		/// <summary>
		/// Очередь для сообщений по отправке писем
		/// </summary>
		public string EmailSendQueue { get; set; }
		/// <summary>
		/// Exchange для сообщений с кодом авторизации
		/// </summary>
		public string AuthorizationCodesExchange { get; set; }
		/// <summary>
		/// Очередь для сообщений с кодом авторизации
		/// </summary>
		public string AuthorizationCodesQueue { get; set; }
		/// <summary>
		/// Очередь сообщений для обновления статуса отправки
		/// </summary>
		public string StatusUpdateQueue { get; set; }
	}
}
