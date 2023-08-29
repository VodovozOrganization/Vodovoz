namespace FirebaseCloudMessaging.Client.Options
{
	/// <summary>
	/// Firebase client settings
	/// </summary>
	public class FirebaseCloudMessagingSettings
	{
		/// <summary>
		/// Send push notifications api url-path
		/// </summary>
		public string SendMessageEndpointURI { get; set; }

		/// <summary>
		/// Firebase api base
		/// </summary>
		public string ApiBase { get; set; }

		/// <summary>
		/// Access token
		/// </summary>
		public string AccessToken { get; set; }
	}
}
