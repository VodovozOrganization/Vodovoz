using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Client
{
	/// <summary>
	/// Уведомление о привязке контрагента к пользователю ИПЗ
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "Уведомления о привязках контрагентов к пользователям ИПЗ",
		Nominative = "Уведомление о привязке контрагента к пользователю ИПЗ",
		Prepositional = "Уведомлении о привязке контрагента к пользователю ИПЗ",
		PrepositionalPlural = "Уведомлениях о привязках контрагентов к пользователям ИПЗ"
	)]
	public class ExternalCounterpartyAssignNotification : IDomainObject, INotification
	{
		public const string TableName = "external_counterparties_assign_notifications";
		public const string IdColumn = "id";
		public const string ExternalCounterartyIdColumn = "external_counterparty_id";
		/// <summary>
		/// Код уведомления
		/// </summary>
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
		/// Код отправки
		/// </summary>
		public virtual int? HttpCode { get; set; }
		/// <summary>
		/// Дата отправки
		/// </summary>
		public virtual DateTime? SentDate { get; set; }
	}
}
