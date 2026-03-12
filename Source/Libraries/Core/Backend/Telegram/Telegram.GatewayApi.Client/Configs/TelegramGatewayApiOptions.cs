namespace Telegram.GatewayApi.Client.Configs
{
	/// <summary>
	/// Настройки для Telegram GatewayApi
	/// </summary>
	public class TelegramGatewayApiOptions
	{
		public const string Path = "TelegramGatewayApiOptions";

		/// <summary>
		/// Основной адрес Telegram Gateway api
		/// </summary>
		public string BaseUrl { get; set; }
		/// <summary>
		/// Токен для авторизации
		/// </summary>
		public string GatewayApiToken { get; set; }
		/// <summary>
		/// Эндпойнт отправки сообщения для генерации кода
		/// </summary>
		public string SendVerificationMessageEndpoint { get; set; }
		/// <summary>
		/// Эндпойнт проверки возможности отправок пользователю(зарегистрирован ли он в Telegram)
		/// </summary>
		public string CheckSendAbilityEndpoint { get; set; }
		/// <summary>
		/// Эндпойнт проверки статуса
		/// </summary>
		public string CheckVerificationStatusEndpoint { get; set; }
	}
}
