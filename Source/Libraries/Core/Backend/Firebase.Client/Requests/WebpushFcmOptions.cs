using System.Text.Json.Serialization;

namespace Vodovoz.FirebaseCloudMessaging.Client.Requests
{
	/// <summary>
	/// Параметры функций, предоставляемых FCM SDK для Интернета.<br/>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#webpushfcmoptions"/>
	/// </summary>
	public class WebpushFcmOptions
	{
		/// <summary>
		/// Ссылка, которая открывается, когда пользователь нажимает на уведомление.<br/>
		/// Для всех значений URL требуется HTTPS.
		/// </summary>
		public string Link { get; set; }

		/// <summary>
		/// Метка, связанная с аналитическими данными сообщения.
		/// </summary>
		[JsonPropertyName("analytics_label")]
		public string AnalyticsLabel { get; set; }
	}
}
