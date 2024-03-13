using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Vodovoz.FirebaseCloudMessaging.Client.Requests
{
	/// <summary>
	/// Параметры <see href="https://tools.ietf.org/html/rfc8030#section-5">протокола Webpush</see><br/>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#webpushconfig"/>
	/// </summary>
	public class WebpushConfig
	{
		/// <summary>
		/// Заголовки HTTP, определенные в протоколе webpush.<br/>
		/// Обратитесь к <see href="https://tools.ietf.org/html/rfc8030#section-5">протоколу Webpush</see> для получения информации о поддерживаемых заголовках, например «TTL»: «15».<br/>
		/// <br/>
		/// Объект, содержащий список пар "key": value.Пример: { "name": "wrench", "mass": "1.3kg", "count": "3" } .
		/// </summary>
		public IDictionary<string, string> Headers { get; set; }

		/// <summary>
		/// Полезная нагрузка произвольного ключа/значения. Если он присутствует, он переопределит <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#Message.FIELDS.data">google.firebase.fcm.v1.Message.data</see> .<br/>
		/// <br/>
		/// Объект, содержащий список пар "key": value.Пример: { "name": "wrench", "mass": "1.3kg", "count": "3" } .
		/// </summary>
		public IDictionary<string, string> Data { get; set; }

		/// <summary>
		/// Параметры веб-уведомлений в виде объекта JSON.
		/// Поддерживает свойства экземпляра уведомления, определенные в <see href="https://developer.mozilla.org/en-US/docs/Web/API/Notification">API веб-уведомлений</see>.
		/// Если они присутствуют, поля «title» и «body» переопределяют <see cref="Notification.Title"> и <see cref="Notification.Body">.
		/// </summary>
		public IDictionary<string, object> Notification { get; set; }

		/// <summary>
		/// Параметры функций, предоставляемых FCM SDK для Интернета.
		/// </summary>
		[JsonPropertyName("fcm_options")]
		public WebpushFcmOptions WebpushFcmOptions { get; set; }
	}
}
