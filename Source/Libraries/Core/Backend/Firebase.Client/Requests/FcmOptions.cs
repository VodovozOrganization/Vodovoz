using System.Text.Json.Serialization;

namespace Vodovoz.FirebaseCloudMessaging.Client.Requests
{
	/// <summary>
	/// Независимые от платформы опции для функций, предоставляемых пакетами FCM SDK.<br/>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#fcmoptions"/>
	/// </summary>
	public class FcmOptions
	{
		/// <summary>
		/// Метка, связанная с аналитическими данными сообщения.
		/// </summary>
		[JsonPropertyName("analytics_label")]
		public string AnalyticsLabel { get; set; }
	}
}
