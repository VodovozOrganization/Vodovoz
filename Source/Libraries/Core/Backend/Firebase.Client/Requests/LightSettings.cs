using System.Text.Json.Serialization;

namespace Vodovoz.FirebaseCloudMessaging.Client.Requests
{
	/// <summary>
	/// Настройки света<br/>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#lightsettings"/>
	/// </summary>
	public class LightSettings
	{
		/// <summary>
		/// Необходимый. Установите color светодиода с помощью <see href="https://github.com/googleapis/googleapis/blob/master/google/type/color.proto">google.type.Color</see> .
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// Необходимый. Наряду с <see cref="LightOffDuration"> определите частоту мигания светодиода.<br/>
		/// Разрешение определяется <see href="https://developers.google.com/protocol-buffers/docs/reference/google.protobuf?hl=ru#google.protobuf.Duration">proto.Duration</see><br/>
		/// <br/>
		/// Длительность в секундах, содержащая до девяти дробных цифр и оканчивающаяся на « s ».<br/>
		/// Пример: "3.5s" .<br/>
		/// </summary>
		[JsonPropertyName("light_on_duration")]
		public string LightOnDuration { get; set; }

		/// <summary>
		/// Необходимый. Наряду с <see cref="LightOnDuration"> определите частоту мигания светодиода.
		/// Разрешение определяется <see href="https://developers.google.com/protocol-buffers/docs/reference/google.protobuf?hl=ru#google.protobuf.Duration">proto.Duration</see><br/>
		/// <br/>
		/// Длительность в секундах, содержащая до девяти дробных цифр и оканчивающаяся на « s ».<br/>
		/// Пример: "3.5s" .<br/>
		/// </summary>
		[JsonPropertyName("light_off_duration")]
		public string LightOffDuration { get; set; }
	}
}
