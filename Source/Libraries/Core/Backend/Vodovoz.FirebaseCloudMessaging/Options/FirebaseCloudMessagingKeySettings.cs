namespace FirebaseCloudMessaging.Client.Options
{
	/// <summary>
	/// Firebase client settings
	/// 
	/// К сожалению Google не умеет в нормальный код для своих библиотек,
	/// а Microsoft не предоставляет тега для изменение имени полей откуда читается информация в конфигурации,
	/// поэтому этот конфиг страшный. Не менять до исправления этих проблем!!!
	/// </summary>
	public class FirebaseCloudMessagingKeySettings
	{
		public string type { get; set; }
		public string project_id { get; set; }
		public string private_key_id { get; set; }
		public string private_key { get; set; }
		public string client_email { get; set; }
		public string client_id { get; set; }
		public string auth_uri { get; set; }
		public string token_uri { get; set; }
		public string auth_provider_x509_cert_url { get; set; }
		public string client_x509_cert_url { get; set; }
		public string universe_domain { get; set; }
	}
}
