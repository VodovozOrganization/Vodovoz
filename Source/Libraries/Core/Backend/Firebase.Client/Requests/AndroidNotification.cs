using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Vodovoz.FirebaseCloudMessaging.Client.Requests
{
	/// <summary>
	/// Уведомление для отправки на устройства Android.<br/>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#androidnotification"/>
	/// </summary>
	public class AndroidNotification
	{
		/// <summary>
		/// Название уведомления. Если он присутствует, он переопределит <see cref="Notification.Title">
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Текст уведомления. Если он присутствует, он переопределит <see cref="Notification.Body">
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// Значок уведомления. Устанавливает значок уведомления myicon для рисуемого ресурса myicon.<br/>
		/// Если вы не отправите этот ключ в запросе, FCM отобразит значок средства запуска, указанный в манифесте вашего приложения.
		/// </summary>
		public string Icon { get; set; }

		/// <summary>
		/// Цвет значка уведомления, выраженный в формате #rrggbb.
		/// </summary>
		public string Color { get; set; }

		/// <summary>
		/// Звук, который воспроизводится, когда устройство получает уведомление.<br/>
		/// Поддерживает «по умолчанию» или имя файла звукового ресурса, включенного в приложение.<br/>
		/// Звуковые файлы должны находиться в /res/raw/.
		/// </summary>
		public string Sound { get; set; }

		/// <summary>
		/// Идентификатор, используемый для замены существующих уведомлений в панели уведомлений.<br/>
		/// Если не указано, каждый запрос создает новое уведомление.<br/>
		/// Если указано и уведомление с таким же тегом уже отображается, новое уведомление заменяет существующее в панели уведомлений.
		/// </summary>
		public string Tag { get; set; }

		/// <summary>
		/// Действие, связанное с щелчком пользователя по уведомлению.<br/>
		/// Если указано, действие с соответствующим фильтром намерений запускается, когда пользователь нажимает на уведомление.
		/// </summary>
		[JsonPropertyName("click_action")]
		public string ClickAction { get; set; }

		/// <summary>
		/// Ключ основной строки в строковых ресурсах приложения, используемый для локализации основного текста в соответствии с текущей локализацией пользователя.<br/>
		/// Дополнительную информацию см. в разделе «<see href="https://goo.gl/NdFZGI">Строковые ресурсы</see>».
		/// </summary>
		[JsonPropertyName("body_loc_key")]
		public string BodyLocKey { get; set; }

		/// <summary>
		/// Значения переменных строк, которые будут использоваться вместо спецификаторов формата в <see cref="BodyLocKey"/> и использоваться для локализации основного текста в соответствии с текущей локализацией пользователя.<br/>
		/// Дополнительную информацию см. в разделе «<see href="https://goo.gl/MalYE3">Форматирование и оформление</see>».
		/// </summary>
		[JsonPropertyName("body_loc_args")]
		public IEnumerable<string> BodyLocArgs { get; set; }


		/// <summary>
		/// Ключ строки заголовка в строковых ресурсах приложения, используемый для локализации текста заголовка в соответствии с текущей локализацией пользователя.<br/>
		/// Дополнительную информацию см. в разделе «<see href="https://goo.gl/NdFZGI">Строковые ресурсы</see>».
		/// </summary>
		[JsonPropertyName("title_loc_key")]
		public string TitleLocKey { get; set; }

		/// <summary>
		/// Значения переменных строк, которые будут использоваться вместо спецификаторов формата в <see cref="TitleLocKey"/> и использоваться для локализации текста заголовка в соответствии с текущей локализацией пользователя.<br/>
		/// Дополнительную информацию см. в разделе «<see href="https://goo.gl/MalYE3">Форматирование и оформление</see>».
		/// </summary>
		[JsonPropertyName("title_loc_args")]
		public IEnumerable<string> TitleLocArgs { get; set; }

		/// <summary>
		/// <see href="https://developer.android.com/guide/topics/ui/notifiers/notifications?hl=ru#ManageChannels">Идентификатор канала уведомления</see> (новое в Android O).<br/>
		/// Приложение должно создать канал с этим идентификатором канала, прежде чем будет получено какое-либо уведомление с этим идентификатором канала.<br/>
		/// Если вы не отправляете этот идентификатор канала в запросе или если предоставленный идентификатор канала еще не создан приложением, FCM использует идентификатор канала, указанный в манифесте приложения.
		/// </summary>
		[JsonPropertyName("channel_id")]
		public string ChannelId { get; set; }

		/// <summary>
		/// Устанавливает текст «тикера», который отправляется службам доступности.<br/>
		/// До уровня API 21 ( Lollipop ) задает текст, который отображается в строке состояния при первом поступлении уведомления.
		/// </summary>
		public string Ticker { get; set; }

		/// <summary>
		/// Если установлено значение false или не установлено, уведомление автоматически закрывается, когда пользователь щелкает его на панели.<br/>
		/// Если установлено значение true, уведомление сохраняется, даже когда пользователь щелкает его.
		/// </summary>
		public bool Sticky { get; set; }

		/// <summary>
		/// Установите время, когда произошло событие в уведомлении.<br/>
		/// Уведомления в панели отсортированы по этому времени.<br/>
		/// Момент времени представляется с помощью <see href="https://developers.google.com/protocol-buffers/docs/reference/java/com/google/protobuf/Timestamp?hl=ru">protobuf.Timestamp</see> .<br/>
		/// <br/>
		/// Временная метка в формате RFC3339 UTC «Зулу» с наносекундным разрешением и до девяти дробных цифр.<br/>
		/// Примеры: "2014-10-02T15:01:23Z" и "2014-10-02T15:01:23.045123456Z" .
		/// </summary>
		[JsonPropertyName("event_time")]
		public string EventType { get; set; }

		/// <summary>
		/// Установите, относится ли это уведомление только к текущему устройству.<br/>
		/// Некоторые уведомления можно перенаправить на другие устройства для удаленного отображения, например на часы Wear OS.<br/>
		/// Эту подсказку можно настроить так, чтобы рекомендовать не перекрывать это уведомление.<br/>
		/// См. <see href="https://developer.android.com/training/wearables/notifications/bridger?hl=ru#existing-method-of-preventing-bridging">руководства по Wear OS</see>
		/// </summary>
		[JsonPropertyName("local_only")]
		public bool LocalOnly { get; set; }

		/// <summary>
		/// Установите относительный приоритет для этого уведомления.<br/>
		/// Приоритет — это показатель того, какую часть внимания пользователя должно занять это уведомление.<br/>
		/// Уведомления с низким приоритетом могут быть скрыты от пользователя в определенных ситуациях, тогда как работа пользователя может быть прервана для уведомления с более высоким приоритетом.<br/>
		/// Эффект от установки одинаковых приоритетов может немного отличаться на разных платформах.<br/>
		/// Обратите внимание, что этот приоритет отличается от <see cref="AndroidMessagePriority">AndroidMessagePriority</see> .<br/>
		/// Этот приоритет обрабатывается клиентом после доставки сообщения, тогда как <see cref="AndroidMessagePriority">AndroidMessagePriority</see> — это концепция FCM, которая контролирует время доставки сообщения.<br/>
		/// </summary>
		[JsonPropertyName("notification_priority")]
		public NotificationPriority NotificationPriority { get; set; }

		/// <summary>
		/// Если установлено значение true, для уведомления используется звук платформы Android по умолчанию.<br/>
		/// Значения по умолчанию указаны в <see href="https://android.googlesource.com/platform/frameworks/base/+/master/core/res/res/values/config.xml">config.xml</see> .
		/// </summary>
		[JsonPropertyName("default_sound")]
		public bool DefaultSound { get; set; }

		/// <summary>
		/// Если установлено значение true, для уведомления используется стандартный шаблон вибрации платформы Android.<br/>
		/// Значения по умолчанию указаны в <see href="https://android.googlesource.com/platform/frameworks/base/+/master/core/res/res/values/config.xml">config.xml</see> .<br/>
		/// Если для <see cref="DefaultVibrateTimings"/> установлено значение true и также установлен <see cref="VibrateTimings"/> , вместо заданного пользователем <see cref="VibrateTimings"/> используется значение по умолчанию.
		/// </summary>
		[JsonPropertyName("default_vibrate_timings")]
		public bool DefaultVibrateTimings { get; set; }

		/// <summary>
		/// Если установлено значение true, для уведомления используйте настройки светодиодной подсветки платформы Android по умолчанию.<br/>
		/// Значения по умолчанию указаны в <see href="https://android.googlesource.com/platform/frameworks/base/+/master/core/res/res/values/config.xml">config.xml</see> .<br/>
		/// Если для <see cref="DefaultLightSettings"/> установлено значение true и <see cref="LightSettings"/> также установлено значение, вместо значения по умолчанию используется указанное пользователем <see cref="LightSettings"/> .
		/// </summary>
		[JsonPropertyName("default_light_settings")]
		public bool DefaultLightSettings { get; set; }

		/// <summary>
		/// Установите используемый шаблон вибрации. Передайте массив <see href="https://developers.google.com/protocol-buffers/docs/reference/google.protobuf?hl=ru#google.protobuf.Duration">protobuf.Duration</see> , чтобы включить или выключить вибратор.<br/>
		/// Первое значение указывает Duration ожидания перед включением вибратора. Следующее значение указывает Duration включения вибратора.<br/>
		/// Последующие значения чередуются между Duration для выключения и включения вибратора.<br/>
		/// Если <see cref="VibrateTimings"/> установлен, а для <see cref="DefaultVibrateTimings"/> установлено значение true , вместо заданного пользователем <see cref="VibrateTimings"/> используется значение по умолчанию.<br/>
		/// <br/>
		/// Длительность в секундах, содержащая до девяти дробных цифр и оканчивающаяся на « s ». Пример: "3.5s" .
		/// </summary>
		[JsonPropertyName("vibrate_timings")]
		public IEnumerable<string> VibrateTimings { get; set; }

		/// <summary>
		/// Установите <see href="https://developer.android.com/reference/android/app/Notification.html?hl=ru#visibility">Notification.visibility</see> уведомления.
		/// </summary>
		public Visibility Visibility { get; set; }

		/// <summary>
		/// Устанавливает количество элементов, которые представляет это уведомление.<br/>
		/// Может отображаться как количество значков для программ запуска, поддерживающих значки.<br/>
		/// См. <see href="https://developer.android.com/training/notify-user/badges?hl=ru">Значок уведомления</see> .<br/>
		/// Например, это может быть полезно, если вы используете только одно уведомление для представления нескольких новых сообщений, но хотите, чтобы здесь отображалось общее количество новых сообщений.<br/>
		/// Если значение равно нулю или не указано, системы, поддерживающие бейджи, используют значение по умолчанию, которое означает увеличение числа, отображаемого в меню длительного нажатия, каждый раз при поступлении нового уведомления.
		/// </summary>
		[JsonPropertyName("notification_count")]
		public int NotificationCount { get; set; }

		/// <summary>
		/// Настройки для управления частотой и цветом мигания светодиода уведомления, если светодиод доступен на устройстве.<br/>
		/// Общее время мигания контролируется ОС.
		/// </summary>
		public LightSettings LightSettings { get; set; }

		/// <summary>
		/// Содержит URL-адрес изображения, которое будет отображаться в уведомлении. Если он присутствует, он переопределит <see cref="Notification.Image"/> .
		/// </summary>
		public string Image { get; set; }
	}
}
