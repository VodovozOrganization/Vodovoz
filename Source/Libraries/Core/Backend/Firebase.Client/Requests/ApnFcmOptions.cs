using System.Text.Json.Serialization;

namespace Vodovoz.FirebaseCloudMessaging.Client.Requests
{
	/// <summary>
	/// Варианты функций, предоставляемых FCM SDK для iOS.<br/>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#apnsfcmoptions"/>
	/// </summary>
	public class ApnFcmOptions
	{
		/// <summary>
		/// Метка, связанная с аналитическими данными сообщения.
		/// </summary>
		[JsonPropertyName("analytics_label")]
		public string AnalyticsLabel { get; set; }

		/// <summary>
		/// Содержит URL-адрес изображения, которое будет отображаться в уведомлении. Если он присутствует, он переопределит <see cref="Notification.Image"/> .
		/// </summary>
		public string Image { get; set; }
	}
}
