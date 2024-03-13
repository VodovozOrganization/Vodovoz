using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Vodovoz.FirebaseCloudMessaging.Client.Requests
{
	/// <summary>
	/// Сообщение<br/>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#resource:-message"/>
	/// </summary>
	public class MessageRequest
	{
		private string _token;
		private string _topic;
		private string _condition;

		/// <summary>
		/// Произвольные полезные данные «ключ/значение», которые должны быть в кодировке UTF-8.<br/>
		/// Ключ не должен быть зарезервированным словом («from», «message_type» или любым словом, начинающимся с «google» или «gcm»).<br/>
		/// При отправке полезных данных, содержащих только поля данных, на устройства iOS в <see cref="ApnsConfig"/> разрешен только обычный приоритет ( "apns-priority": "5" ).<br/>
		/// <br/>
		/// Объект, содержащий список пар "key": value.Пример: { "name": "wrench", "mass": "1.3kg", "count": "3" } .
		/// </summary>
		public IDictionary<string, string> Data { get; set; }

		/// <summary>
		/// Базовый шаблон уведомления для использования на всех платформах.
		/// </summary>
		public Notification Notification { get; set; }

		/// <summary>
		/// Специальные параметры Android для сообщений, отправляемых через <see href="https://goo.gl/4GLdUl">сервер соединений FCM</see> .
		/// </summary>
		[JsonPropertyName("android")]
		public AndroidConfig AndroidConfig { get; set; }

		/// <summary>
		/// Параметры протокола <see href="https://tools.ietf.org/html/rfc8030">Webpush</see> .
		/// </summary>
		[JsonPropertyName("webpush")]
		public WebpushConfig WebpushConfig { get; set; }

		/// <summary>
		/// Специальные параметры <see href="https://goo.gl/MXRTPa">службы push-уведомлений Apple</see> .
		/// </summary>
		[JsonPropertyName("apns")]
		public ApnsConfig ApnsConfig { get; set; }

		/// <summary>
		/// Шаблон для опций функций FCM SDK для использования на всех платформах.
		/// </summary>
		[JsonPropertyName("fcm_options")]
		public FcmOptions FcmOptions { get; set; }

		/// <summary>
		/// Регистрационный токен для отправки сообщения.<br/>
		/// Цель для отправки сообщения. target может быть только одно из следующих:<br/>
		///  - Token<br/>
		///  - Topic<br/>
		///  - Condition<br/>
		/// </summary>
		public string Token
		{
			get => _token;
			set
			{
				if(Topic != null || Condition != null)
				{
					throw new InvalidOperationException("Может быть заполнено только 1 из полей Token, Topic или Condition");
				}

				_token = value;
			}
		}

		/// <summary>
		/// Название темы, в которую нужно отправить сообщение, например «погода». Примечание. Префикс «/topics/» указывать не следует.<br/>
		/// Цель для отправки сообщения. target может быть только одно из следующих:<br/>
		///  - Token<br/>
		///  - Topic<br/>
		///  - Condition<br/>
		/// </summary>
		public string Topic
		{
			get => _topic;
			set
			{
				if(Token != null || Condition != null)
				{
					throw new InvalidOperationException("Может быть заполнено только 1 из полей Token, Topic или Condition");
				}

				_topic = value;
			}
		}

		/// <summary>
		/// Условие отправки сообщения, например, «foo» в темах &amp;&amp; «bar» в темах».<br/>
		/// Цель для отправки сообщения. target может быть только одно из следующих:<br/>
		///  - Token<br/>
		///  - Topic<br/>
		///  - Condition<br/>
		/// </summary>
		public string Condition
		{
			get => _condition;
			set
			{
				if(Token != null || Topic != null)
				{
					throw new InvalidOperationException("Может быть заполнено только 1 из полей Token, Topic или Condition");
				}

				_condition = value;
			}
		}
	}
}
