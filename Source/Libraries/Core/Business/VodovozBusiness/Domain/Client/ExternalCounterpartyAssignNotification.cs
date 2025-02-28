using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Client
{
	/// <summary>
	/// Уведомление о сопоставление клиента из внешнего источника
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "Уведомления о сопоставлениях клиента из внешнего источника",
		Nominative = "Уведомление о сопоставление клиента из внешнего источника",
		Prepositional = "Уведомлении о сопоставлении клиента из внешнего источника",
		PrepositionalPlural = "Уведомлениях о сопоставлениях клиента из внешнего источника"
	)]
	public class ExternalCounterpartyAssignNotification : IDomainObject
	{
		public virtual int Id { get; set; }
		/// <summary>
		/// Дата создания
		/// </summary>
		public virtual DateTime CreationDate { get; set; }
		/// <summary>
		/// Пользователь ИПЗ
		/// </summary>
		public virtual ExternalCounterparty ExternalCounterparty { get; set; }
		/// <summary>
		/// Код ответа сервера
		/// </summary>
		public virtual int? HttpCode { get; set; }
		/// <summary>
		/// Дата отправки
		/// </summary>
		public virtual DateTime? SentDate { get; set; }
	}
}
