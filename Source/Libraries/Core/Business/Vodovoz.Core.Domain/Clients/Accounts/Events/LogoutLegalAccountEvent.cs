using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Clients.Accounts.Events
{
	/// <summary>
	/// Событие для ИПЗ, чтобы разлогинить аккаунт юр лица
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "События для выхода из аккаунтов юр лиц в ИПЗ",
		Nominative = "Событие для выхода из аккаунта юр лица в ИПЗ",
		Prepositional = "Событии для выхода из аккаунта юр лица в ИПЗ",
		PrepositionalPlural = "Событиях для выхода из аккаунтов юр лиц в ИПЗ"
	)]
	[HistoryTrace]
	public class LogoutLegalAccountEvent : IDomainObject
	{
		protected LogoutLegalAccountEvent() { }

		private LogoutLegalAccountEvent(int counterpartyId, string email)
		{
			ErpCounterpartyId = counterpartyId;
			Email = email;	
		}
		
		/// <summary>
		/// Идентификатор
		/// </summary>
		[JsonIgnore]
		public virtual int Id { get; set; }
		/// <summary>
		/// Идентификатор юр лица
		/// </summary>
		[Display(Name = "Код клиента")]
		public virtual int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Электронная почта
		/// </summary>
		[Display(Name = "Электронная почта")]
		public virtual string Email { get; set; }
		/// <summary>
		/// Доставлено
		/// </summary>
		[JsonIgnore]
		[Display(Name = "Доставлено")]
		public virtual bool Delivered { get; set; }
		/// <summary>
		/// Дата и время последней отправки
		/// </summary>
		[JsonIgnore]
		[Display(Name = "Дата и время последней отправки")]
		public virtual DateTime? LastSentDateTime { get; set; }
		/// <summary>
		/// Количество совершенных попыток
		/// </summary>
		[JsonIgnore]
		[Display(Name = "Количество отправленных уведомлений")]
		public virtual int SentEventsCount { get; set; }

		public static LogoutLegalAccountEvent Create(int counterpartyId, string email) =>
			new LogoutLegalAccountEvent(counterpartyId, email);
	}
}
