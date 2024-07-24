using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Client
{
	/// <summary>
	/// Уведомление об удалении пользователя ИПЗ
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "Уведомления об удалении пользователей ИПЗ",
		Nominative = "Уведомление об удалении пользователя ИПЗ",
		Prepositional = "Уведомлении об удалении пользователя ИПЗ",
		PrepositionalPlural = "Уведомлениях об удалении пользователей ИПЗ"
	)]
	public class DeletedExternalCounterpartyNotification : IDomainObject, INotification
	{
		public DeletedExternalCounterpartyNotification() { }

		protected DeletedExternalCounterpartyNotification(
			Guid externalCounterpartyId, int erpCounterpartyId, CounterpartyFrom counterpartyFrom)
		{
			ErpCounterpartyId = erpCounterpartyId;
			CounterpartyFrom = counterpartyFrom;
			ExternalCounterpartyId = externalCounterpartyId;
			CreationDate = DateTime.Now;
		}
		
		/// <summary>
		/// Код уведомления
		/// </summary>
		public virtual int Id { get; set; }
		/// <summary>
		/// Дата создания
		/// </summary>
		public virtual DateTime CreationDate { get; set; }
		/// <summary>
		/// Код клиента
		/// </summary>
		public virtual int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Код пользователя из ИПЗ
		/// </summary>
		public virtual Guid ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Откуда регистрировался клиент
		/// </summary>
		public virtual CounterpartyFrom CounterpartyFrom { get; set; }
		/// <summary>
		/// Код отправки
		/// </summary>
		public virtual int? HttpCode { get; set; }
		/// <summary>
		/// Дата отправки
		/// </summary>
		public virtual DateTime? SentDate { get; set; }

		public static DeletedExternalCounterpartyNotification Create(
			Guid externalCounterpartyId, int erpCounterpartyId, CounterpartyFrom counterpartyFrom)
		{
			return new DeletedExternalCounterpartyNotification(externalCounterpartyId, erpCounterpartyId, counterpartyFrom);
		}
	}
}
