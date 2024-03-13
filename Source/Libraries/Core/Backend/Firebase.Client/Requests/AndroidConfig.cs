using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Vodovoz.FirebaseCloudMessaging.Client.Requests
{
	/// <summary>
	/// Специальные параметры Android для сообщений, отправляемых через <see href="https://goo.gl/4GLdUl">сервер соединений FCM</see> .<br/>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#androidconfig"/>
	/// </summary>
	public class AndroidConfig
	{
		/// <summary>
		/// Идентификатор группы сообщений, которые можно свернуть, чтобы при возобновлении доставки отправлялось только последнее сообщение.<br/>
		/// В любой момент времени допускается максимум 4 различных ключа свертывания.
		/// </summary>
		[JsonPropertyName("collapse_key")]
		public string CollapseKey { get; set; }

		/// <summary>
		/// Приоритет сообщения. Может принимать «нормальные» и «высокие» значения.
		/// Дополнительную информацию см. в разделе <see href="https://goo.gl/GjONJv">Установка приоритета сообщения</see> .
		/// </summary>
		public AndroidMessagePriority Priority { get; set; }

		/// <summary>
		/// Как долго (в секундах) сообщение должно храниться в хранилище FCM, если устройство находится в автономном режиме.<br/>
		/// Максимальное время поддержки составляет 4 недели, а значение по умолчанию — 4 недели, если оно не установлено.<br/>
		/// Установите значение 0, если хотите отправить сообщение немедленно.<br/>
		/// В формате JSON тип Duration кодируется как строка, а не как объект, где строка заканчивается суффиксом «s» (указывающим секунды) и ей предшествует количество секунд, при этом наносекунды выражаются как дробные секунды.<br/>
		/// Например, 3 секунды с 0 наносекундами должны быть закодированы в формате JSON как «3 с», а 3 секунды и 1 наносекунда должны быть выражены в формате JSON как «3.000000001 с».<br/>
		/// TTL будет округлен до ближайшей секунды.<br/>
		/// <br/>
		/// Длительность в секундах, содержащая до девяти дробных цифр и оканчивающаяся на « s ». Пример: "3.5s" .
		/// </summary>
		public string Ttl { get; set; }

		/// <summary>
		/// Имя пакета приложения, которому должен соответствовать регистрационный токен, чтобы получить сообщение.
		/// </summary>
		[JsonPropertyName("restricted_package_name")]
		public string RestrictedPackageName { get; set; }

		/// <summary>
		/// Полезная нагрузка произвольного ключа/значения. Если он присутствует, он переопределит <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#Message.FIELDS.data">google.firebase.fcm.v1.Message.data</see> .<br/>
		/// <br/>
		/// Объект, содержащий список пар "key": value.Пример: { "name": "wrench", "mass": "1.3kg", "count": "3" } .
		/// </summary>
		public IDictionary<string, string> Data { get; set; }

		/// <summary>
		/// Уведомление для отправки на устройства Android.
		/// </summary>
		public AndroidNotification Notification { get; set; }

		/// <summary>
		/// Варианты функций, предоставляемых FCM SDK для Android.
		/// </summary>
		[JsonPropertyName("fcm_options")]
		public AndroidFcmOptions FcmOptions { get; set; }

		/// <summary>
		/// Если установлено значение true, сообщениям будет разрешено доставляться в приложение, пока устройство находится в режиме прямой загрузки.<br/>
		/// См. раздел <see href="https://developer.android.com/training/articles/direct-boot?hl=ru">Поддержка режима прямой загрузки</see> .
		/// </summary>
		[JsonPropertyName("direct_boot_ok")]
		public bool DirectBootOk { get; set; }
	}
}
