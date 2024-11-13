namespace Mailjet.Api.Abstractions.Configs
{
	/// <summary>
	/// Конфиг для mailjet
	/// </summary>
	public class MailjetOptions
	{
		/// <summary>
		/// Путь к секции настроек
		/// </summary>
		public const string Path = "Mailjet";
		/// <summary>
		/// Базовый адрес сервиса
		/// </summary>
		public string BaseUri { get; set; }
		/// <summary>
		/// Пользователь
		/// </summary>
		public string Username { get; set; }
		/// <summary>
		/// Пароль
		/// </summary>
		public string Password { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public bool Sandbox { get; set; }
	}
}
