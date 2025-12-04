using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto.Responses
{
	/// <summary>
	/// Представляет ответ сервиса Mailganer о проверке адреса на наличие в стоп-листе
	/// </summary>
	public class StopListSearchResponse : MailganerResponseBase
	{
		/// <summary>
		/// Информация о времени жизни (TTL) записей для доменов
		/// </summary>
		[JsonPropertyName("ttl")]
		public IEnumerable<TtlDto> Ttls { get; set; }

		/// <summary>
		/// Список доменов, для которых найдено совпадение в стоп-листе
		/// </summary>
		[JsonPropertyName("domains")]
		public IEnumerable<string> Domains { get; set; }

		/// <summary>
		/// Список записей о возвратах писем (bounces) для проверяемого адреса
		/// </summary>
		[JsonPropertyName("bounces")]
		public IEnumerable<BounceDto> Bounces { get; set; }

		/// <summary>
		/// Список событий отписки пользователя (unsubscribe) от рассылок
		/// </summary>
		[JsonPropertyName("user_unsubscribe")]
		public IEnumerable<UserUnsubscribeDto> UserUnsubscribes { get; set; }

		/// <summary>
		/// Список отчетов обратной связи (feedback loop, FBL), сигнализирующих о жалобах пользователей на спам
		/// </summary>
		[JsonPropertyName("fbl_report")]
		public IEnumerable<FblReportDto> FblReports { get; set; }
	}
}
