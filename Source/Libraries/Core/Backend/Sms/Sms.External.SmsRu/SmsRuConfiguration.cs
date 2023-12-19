namespace Sms.External.SmsRu
{
	public class SmsRuConfiguration : ISmsRuConfiguration
	{
		private readonly string login;
		private readonly string password;
		private readonly string apiId;
		private readonly string partnerId;
		private readonly string emailToSmsGateEmail;
		private readonly string email;
		private readonly string smsNumberFrom;
		private readonly string smtpLogin;
		private readonly string smtpPassword;
		private readonly string smtpServer;
		private readonly int smtpPort;
		private readonly bool smtpUsrSSL;
		private readonly bool translit;
		private readonly bool test;
		private readonly string _baseUrl;

		public SmsRuConfiguration(
			string login,
			string password,
			string apiId,
			string partnerId,
			string email,
			string smsNumberFrom,
			string smtpLogin,
			string smtpPassword,
			string smtpServer,
			int smtpPort,
			bool smtpUseSSL,
			bool translit,
			bool test,
			string baseUrl
			)
		{
			this.login = login;
			this.password = password;
			this.apiId = apiId;
			this.partnerId = partnerId;
			emailToSmsGateEmail = apiId + "@sms.ru";
			this.email = email;
			this.smsNumberFrom = smsNumberFrom;
			this.smtpLogin = smtpLogin;
			this.smtpPassword = smtpPassword;
			this.smtpServer = smtpServer;
			this.smtpPort = smtpPort;
			smtpUsrSSL = smtpUseSSL;
			this.translit = translit;
			this.test = test;
			_baseUrl = baseUrl;
		}

		/// <summary>
		/// Логин для доступа к сервису SMS.RU
		/// </summary>
		public string Login => login;

		/// <summary>
		/// Пароль для доступа к сервису SMS.RU
		/// </summary>
		public string Password => password;

		/// <summary>
		/// Является вашим секретным кодом, который используется во внешних программах
		/// </summary>
		public string ApiId => apiId;

		/// <summary>
		/// Если вы участвуете в партнерской программе, укажите этот параметр в запросе
		/// </summary>
		public string PartnerId => partnerId;

		/// <summary>
		///  Ваш уникальный адрес (для отправки СМС по email)
		/// </summary>
		public string EmailToSmsGateEmail => emailToSmsGateEmail;

		/// <summary>
		/// Ваш email адрес для отправки
		/// </summary>
		public string Email => email;

		/// <summary>
		/// Логин для авторизации на SMTP-сервере
		/// </summary>
		public string SmtpLogin => smtpLogin;

		/// <summary>
		/// Пароль для авторизации на SMTP-сервере
		/// </summary>
		public string SmtpPassword => smtpPassword;

		/// <summary>
		/// SMTP-сервер
		/// </summary>
		public string SmtpServer => smtpServer;

		/// <summary>
		/// Порт для авторизации на SMTP-сервере
		/// </summary>
		public int SmtpPort => smtpPort;

		/// <summary>
		/// Флаг - использовать SSL при подключении к серверу SMTP
		/// </summary>
		public bool SmtpUseSSL => smtpUsrSSL;

		/// <summary>
		/// Переводит все русские символы в латинские
		/// </summary>
		public bool Translit => translit;

		/// <summary>
		/// Имитирует отправку сообщения для тестирования ваших программ на правильность обработки ответов сервера. При этом само сообщение не отправляется и баланс не расходуется.
		/// </summary>
		public bool Test => test;

		/// <summary>
		/// Номер с которого будет оправлено сообщение (необходимо согласование с администрацией Sms.ru)
		/// </summary>
		public string SmsNumberFrom => smsNumberFrom;

		/// <summary>
		/// Адрес сайта
		/// </summary>
		public string BaseUrl => _baseUrl;
	}
}
