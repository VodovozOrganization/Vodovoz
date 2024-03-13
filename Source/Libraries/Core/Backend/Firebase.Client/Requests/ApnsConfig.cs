using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Vodovoz.FirebaseCloudMessaging.Client.Requests
{
	/// <summary>
	/// Специальные параметры <see href="https://goo.gl/MXRTPa">службы push-уведомлений Apple</see> .<br/>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#apnsconfig">
	/// </summary>
	public class ApnsConfig
	{
		/// <summary>
		/// Заголовки HTTP-запросов, определенные в службе push-уведомлений Apple.<br/>
		/// Обратитесь к заголовкам запросов APN, чтобы узнать о поддерживаемых заголовках, таких как apns-expiration и apns-priority .<br/>
		/// <br/>
		/// Серверная часть устанавливает значение по умолчанию для apns-expiration равное 30 дням, и значение по умолчанию для apns-priority равное 10, если оно не установлено явно.<br/>
		/// <br/>
		/// Объект, содержащий список пар "key": value.Пример: { "name": "wrench", "mass": "1.3kg", "count": "3" } .
		/// </summary>
		public IDictionary<string, string> Headers { get; set; }

		/// <summary>
		/// Полезная нагрузка APN в виде объекта JSON, включая словарь aps и пользовательскую полезную нагрузку.
		/// См. Справочник по ключам полезной нагрузки .
		/// Если он присутствует, он переопределяет <see cref="Notification.Title"> и <see cref="Notification.Body"> .
		/// </summary>
		public object Payload { get; set; }

		/// <summary>
		/// Варианты функций, предоставляемых FCM SDK для iOS.
		/// </summary>
		[JsonPropertyName("fcm_options")]
		public ApnFcmOptions FcmOptions { get; set; }
	}
}
