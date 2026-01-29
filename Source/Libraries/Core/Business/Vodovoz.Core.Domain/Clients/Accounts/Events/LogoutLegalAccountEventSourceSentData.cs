using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Clients.Accounts.Events
{
	/// <summary>
	/// Данные по отправке события разлогинивания аккаунта юр лица в ИПЗ
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Данные по отправкам событий разлогинивания аккаунта юр лица в ИПЗ",
		Nominative = "Данные по отправке события разлогинивания аккаунта юр лица в ИПЗ",
		Prepositional = "Данных по отправке события разлогинивания аккаунта юр лица в ИПЗ",
		PrepositionalPlural = "Данных по отправкам событий разлогинивания аккаунта юр лица в ИПЗ"
	)]
	public class LogoutLegalAccountEventSourceSentData : IDomainObject
	{
		protected LogoutLegalAccountEventSourceSentData() { }

		private LogoutLegalAccountEventSourceSentData(LogoutLegalAccountEvent @event, Source source)
		{
			Event = @event;
			Source = source;
		}
		
		public virtual int Id { get; set; }
		
		/// <summary>
		/// ИПЗ
		/// </summary>
		[JsonIgnore]
		[Display(Name = "ИПЗ")]
		public virtual Source Source { get; set; }
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
		/// <summary>
		/// Событие
		/// </summary>
		[JsonIgnore]
		[Display(Name = "Событие")]
		public virtual LogoutLegalAccountEvent Event { get; set; }

		internal static LogoutLegalAccountEventSourceSentData Create(LogoutLegalAccountEvent @event, Source source) =>
			new LogoutLegalAccountEventSourceSentData(@event, source);
	}
}
