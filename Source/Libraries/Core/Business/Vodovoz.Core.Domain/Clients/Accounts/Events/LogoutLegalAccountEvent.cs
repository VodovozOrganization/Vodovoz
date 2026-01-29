using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
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

		private LogoutLegalAccountEvent(int counterpartyId, string email, IEnumerable<Source> sources)
		{
			ErpCounterpartyId = counterpartyId;
			Email = email;

			foreach(var source in sources)
			{
				var sentData = LogoutLegalAccountEventSourceSentData.Create(this, source);
				SentData.Add(sentData);
			}
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
		/// Данные по отправкам
		/// </summary>
		[Display(Name = "Данные по отправкам")]
		[JsonIgnore]
		public virtual IObservableList<LogoutLegalAccountEventSourceSentData> SentData { get; set; }
			= new ObservableList<LogoutLegalAccountEventSourceSentData>();

		public static LogoutLegalAccountEvent Create(int counterpartyId, string email, IEnumerable<Source> sources) =>
			new LogoutLegalAccountEvent(counterpartyId, email, sources);
	}
}
