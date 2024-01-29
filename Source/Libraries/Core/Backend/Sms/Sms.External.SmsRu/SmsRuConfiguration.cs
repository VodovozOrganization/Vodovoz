namespace Sms.External.SmsRu
{
	public class SmsRuConfiguration
	{
		/// <summary>
		/// Логин для доступа к сервису SMS.RU
		/// </summary>
		public string Login { get; set; }

		/// <summary>
		/// Пароль для доступа к сервису SMS.RU
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Является вашим секретным кодом, который используется во внешних программах
		/// </summary>
		public string ApiId { get; set; }

		/// <summary>
		/// Если вы участвуете в партнерской программе, укажите этот параметр в запросе
		/// </summary>
		public string PartnerId { get; set; }

		/// <summary>
		///  Ваш уникальный адрес (для отправки СМС по email)
		/// </summary>
		public string EmailToSmsGateEmail => ApiId + "@sms.ru";

		/// <summary>
		/// Ваш email адрес для отправки
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Логин для авторизации на SMTP-сервере
		/// </summary>
		public string SmtpLogin { get; set; }

		/// <summary>
		/// Пароль для авторизации на SMTP-сервере
		/// </summary>
		public string SmtpPassword { get; set; }

		/// <summary>
		/// SMTP-сервер
		/// </summary>
		public string SmtpServer { get; set; }

		/// <summary>
		/// Порт для авторизации на SMTP-сервере
		/// </summary>
		public int SmtpPort { get; set; }

		/// <summary>
		/// Флаг - использовать SSL при подключении к серверу SMTP
		/// </summary>
		public bool SmtpUseSSL { get; set; }

		/// <summary>
		/// Переводит все русские символы в латинские
		/// </summary>
		public bool Translit { get; set; }

		/// <summary>
		/// Имитирует отправку сообщения для тестирования ваших программ на правильность обработки ответов сервера. При этом само сообщение не отправляется и баланс не расходуется.
		/// </summary>
		public bool Test { get; set; }

		/// <summary>
		/// Номер с которого будет оправлено сообщение (необходимо согласование с администрацией Sms.ru)
		/// </summary>
		public string SmsNumberFrom { get; set; }

		/// <summary>
		/// Адрес сайта
		/// </summary>
		public string BaseUrl { get; set; }
	}
}
