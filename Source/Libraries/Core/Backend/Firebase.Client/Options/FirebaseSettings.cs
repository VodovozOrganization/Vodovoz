namespace Firebase.Client.Options
{
	/// <summary>
	/// Firebase client settings
	/// </summary>
	public class FirebaseSettings
	{
		/// <summary>
		/// Send push notifications api url-path
		/// </summary>
		public string SendPushNotificationEndpointURI { get; set; }

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
