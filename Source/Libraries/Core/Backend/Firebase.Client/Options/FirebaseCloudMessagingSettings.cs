namespace FirebaseCloudMessaging.Client.Options
{
	/// <summary>
	/// Firebase client settings
	/// </summary>
	public class FirebaseCloudMessagingSettings
	{
		/// <summary>
		/// Идентификатор приложения
		/// </summary>
		public string ApplicationId { get; set; }

		/// <summary>
		/// Access token
		/// </summary>
		public string AccessToken { get; set; }

		/// <summary>
		/// Базовый URL Firebase Cloud Messaging Api
		/// </summary>
		public string ApiBase { get; set; }
	}
}
