using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto
{
	public class PackageEmailMessage
	{
		public PackageEmailMessage()
		{
			Headers = new Dictionary<string, string>();
			Users = new List<PackageEmailUser>();
		}

		/// <summary>
		/// Email отправителя
		/// </summary>
		[JsonPropertyName("email_from")]
		public string EmailFrom { get; set; }

		/// <summary>
		/// Имя отправителя
		/// </summary>
		[JsonPropertyName("name_from")]
		public string NameFrom { get; set; }

		/// <summary>
		/// Тема письма
		/// </summary>
		[JsonPropertyName("subject")]
		public string Subject { get; set; }

		/// <summary>
		/// Проверка по локальному (клиентскому) стоп-листу <br/>
		/// Включает проверку по локальному стоп-листу. <br/>
		/// Для подключения данной фичи обратитесь в службу поддержки
		/// </summary>
		[JsonPropertyName("check_local_stop_list")]
		public bool CheckLocalStopList { get; set; }

		/// <summary>
		/// Включает необходимость промодерировать рассылку со стороны SamOtpravil <br/>
		/// Флаг актуален для whitelabel клиентов
		/// </summary>
		[JsonPropertyName("is_moderate")]
		public bool IsModerate { get; set; }

		/// <summary>
		/// Вёрстка письма
		/// </summary>
		[JsonPropertyName("message_text")]
		public string MessageText { get; set; }

		/// <summary>
		/// Свои заголовки <br/>
		/// В формате пары заголовок-значение
		/// пример: "headers": { "param1": "value1", "param2": "value2" }
		/// </summary>
		[JsonPropertyName("headers")]
		public IDictionary<string, string> Headers { get; set; }

		/// <summary>
		/// Отслеживать открытия <br/>
		/// Для работы необходимо передавать параметр x_track_id
		/// </summary>
		[JsonPropertyName("track_open")]
		public bool TrackOpen { get; set; }

		/// <summary>
		/// Отслеживать клики <br/>
		/// Для работы необходимо передавать параметр x_track_id
		/// </summary>
		[JsonPropertyName("track_click")]
		public bool TrackClick { get; set; }

		/// <summary>
		/// Домен треккинга <br/>
		/// Можно настроить свой домен трекинга
		/// </summary>
		[JsonPropertyName("track_domain")]
		public string TrackDomain { get; set; }

		/// <summary>
		/// Не рендерить переменные <br/>
		/// Позволяет передавать текст в сыром виде
		/// </summary>
		[JsonPropertyName("users")]
		public ICollection<PackageEmailUser> Users { get; set; }
	}
}
