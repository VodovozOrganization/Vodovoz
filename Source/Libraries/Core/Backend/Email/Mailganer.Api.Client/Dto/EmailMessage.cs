using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto
{
	public class EmailMessage
	{
		public EmailMessage()
		{
			Params = new Dictionary<string, string>();
			Headers = new Dictionary<string, string>();
			Attachments = new List<EmailAttachment>();
		}

		/// <summary>
		/// Отправитель <br/>
		/// Формат: ИмяОтправителя <ИмейлОтправителя>
		/// </summary>
		[JsonPropertyName("email_from")]
		public string From { get; set; }

		/// <summary>
		/// Получатель <br/>
		/// Формат: ИмейлПолучателя
		/// </summary>
		[JsonPropertyName("email_to")]
		public string To { get; set; }

		/// <summary>
		/// Тема письма
		/// </summary>
		[JsonPropertyName("subject")]
		public string Subject { get; set; }

		/// <summary>
		/// Вёрстка письма
		/// </summary>
		[JsonPropertyName("message_text")]
		public string MessageText { get; set; }

		/// <summary>
		/// Переменные для подстановки в тело письма <br/>
		/// Переменные типа пара переменная-значение <br/>
		/// пример: "params": { "param1": "value1", "param2": "value2" }
		/// </summary>
		[JsonPropertyName("params")]
		public IDictionary<string, string> Params { get; set; }

		/// <summary>
		/// Валидация имейла <br/>
		/// Включает проверку по стоп-листам
		/// </summary>
		[JsonPropertyName("check_stop_list")]
		public bool CheckStopList { get; set; }

		/// <summary>
		/// Проверка по локальному (клиентскому) стоп-листу <br/>
		/// Выключает проверку по локальному стоп-листу. По дефолту true. <br/>
		/// Для подключения данной фичи обратитесь в службу поддержки
		/// </summary>
		[JsonPropertyName("check_local_stop_list")]
		public bool CheckLocalStopList { get; set; } = true;

		/// <summary>
		/// X-Track-ID Пользовательский ID.
		/// Должен быть уникальным для каждой отправки. <br/>
		/// Хорошая практика формирования X-Track-ID - {{login}}-{{timestamp}}-{{your-id}} <br/>
		/// login - ваш логин к SMTP, <br/>
		/// timestamp - временная метка запроса, <br/>
		/// your-id - любой понятный вам ID
		/// </summary>
		[JsonPropertyName("x_track_id")]
		public string TrackId { get; set; }

		/// <summary>
		/// Домен для DKIM
		/// Домен, которым подписать письмо. Если не указан, то домен берётся из email_from
		/// </summary>
		[JsonPropertyName("domain_for_dkim")]
		public string DomainForDkim { get; set; }

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
		/// Свои заголовки <br/>
		/// В формате пары заголовок-значение
		/// пример: "headers": { "param1": "value1", "param2": "value2" }
		/// </summary>
		[JsonPropertyName("headers")]
		public IDictionary<string, string> Headers { get; set; }

		/// <summary>
		/// Ссылка отписки <br/>
		/// Зарезервированная переменная ссылка отписки
		/// </summary>
		[JsonPropertyName("params.sys_unsubscribe_url")]
		public string UnsubscribeUrl { get; set; }

		/// <summary>
		/// Не рендерить переменные <br/>
		/// Позволяет передавать текст в сыром виде
		/// </summary>
		[JsonPropertyName("raw")]
		public bool IsRaw { get; set; }

		/// <summary>
		/// Не рендерить переменные <br/>
		/// Позволяет передавать текст в сыром виде
		/// </summary>
		[JsonPropertyName("attach_files")]
		public ICollection<EmailAttachment> Attachments { get; set; }
	}
}
