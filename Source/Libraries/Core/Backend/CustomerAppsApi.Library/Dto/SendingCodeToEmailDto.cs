using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto
{
	/// <summary>
	/// Данные для формирования письма с кодом авторизации
	/// </summary>
	public class SendingCodeToEmailDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Id пользователя из ИПЗ
		/// </summary>
		public Guid ExternalUserId { get; set; }
		/// <summary>
		/// Id клиента из ДВ
		/// </summary>
		public int CounterpartyId { get; set; }
		/// <summary>
		/// Адрес электронной почты
		/// </summary>
		public string EmailAddress { get; set; }
		/// <summary>
		/// Сообщение
		/// </summary>
		public string Message { get; set; }
	}
}
