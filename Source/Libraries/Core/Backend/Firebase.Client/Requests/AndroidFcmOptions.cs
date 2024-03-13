using System.Text.Json.Serialization;

namespace Vodovoz.FirebaseCloudMessaging.Client.Requests
{
	/// <summary>
	/// Варианты функций, предоставляемых FCM SDK для Android.<br/>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#androidfcmoptions"/>
	/// </summary>
	public class AndroidFcmOptions
	{
		/// <summary>
		/// Метка, связанная с аналитическими данными сообщения.
		/// </summary>
		[JsonPropertyName("analytics_label")]
		public string AnalyticsLabel { get; set; }
	}
}
