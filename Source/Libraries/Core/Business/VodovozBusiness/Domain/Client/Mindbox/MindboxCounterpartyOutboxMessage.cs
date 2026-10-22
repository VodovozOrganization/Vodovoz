using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Client.Mindbox
{
	/// <summary>
	/// Сообщение очереди отправки контрагентов в Mindbox
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Neuter,
		NominativePlural = "очередь отправки контрагентов в Mindbox",
		Nominative = "сообщение очереди Mindbox")]
	public class MindboxCounterpartyOutboxMessage : IDomainObject
	{
		/// <summary>
		/// Идентификатор сообщения
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Контрагент, которого нужно отправить в Mindbox
		/// </summary>
		public virtual Counterparty Counterparty { get; set; }

		/// <summary>
		/// Ключ идемпотентности операции
		/// </summary>
		public virtual Guid TransactionId { get; set; }

		/// <summary>
		/// Статус сообщения
		/// </summary>
		public virtual MindboxCounterpartyOutboxStatus Status { get; set; }

		/// <summary>
		/// Количество выполненных попыток отправки
		/// </summary>
		public virtual int Attempts { get; set; }

		/// <summary>
		/// Текст последней ошибки
		/// </summary>
		public virtual string LastError { get; set; }

		/// <summary>
		/// Дата и время постановки в очередь (UTC)
		/// </summary>
		public virtual DateTime CreatedAt { get; set; }

		/// <summary>
		/// Дата и время следующей попытки (UTC)
		/// </summary>
		public virtual DateTime? NextAttemptAt { get; set; }

		/// <summary>
		/// Дата и время успешной отправки (UTC)
		/// </summary>
		public virtual DateTime? SentAt { get; set; }
	}
}
