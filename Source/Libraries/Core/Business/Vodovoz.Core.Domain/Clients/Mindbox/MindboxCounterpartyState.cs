using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Clients.Mindbox
{
	/// <summary>
	/// Состояние отправки контрагента в Mindbox.
	/// Одна запись на контрагента.
	/// </summary>
	public class MindboxCounterpartyState : IDomainObject
	{
		/// <summary>
		/// Идентификатор контрагента (он же первичный ключ)
		/// </summary>
		public virtual int CounterpartyId { get; set; }

		int IDomainObject.Id => CounterpartyId;

		/// <summary>
		/// Статус отправки
		/// </summary>
		public virtual MindboxCounterpartyOutboxStatus Status { get; set; } =
			MindboxCounterpartyOutboxStatus.Unknown;

		/// <summary>
		/// Ключ идемпотентности текущей операции
		/// </summary>
		public virtual Guid? TransactionId { get; set; }

		/// <summary>
		/// Количество выполненных попыток
		/// </summary>
		public virtual int Attempts { get; set; }

		/// <summary>
		/// Дата и время последней попытки (UTC)
		/// </summary>
		public virtual DateTime? LastAttemptAt { get; set; }

		/// <summary>
		/// Последняя ошибка: status, errorMessage/validationMessages, errorId
		/// </summary>
		public virtual string LastError { get; set; }

		/// <summary>
		/// HTTP-код последнего ответа
		/// </summary>
		public virtual int? LastHttpStatusCode { get; set; }

		/// <summary>
		/// Статус последнего ответа Mindbox
		/// </summary>
		public virtual string LastMindboxStatus { get; set; }

		/// <summary>
		/// Идентификатор ошибки Mindbox
		/// </summary>
		public virtual string LastErrorId { get; set; }

		/// <summary>
		/// Длительность последней попытки, мс
		/// </summary>
		public virtual int? LastAttemptDurationMs { get; set; }

		/// <summary>
		/// Дата и время успешной отправки (UTC)
		/// </summary>
		public virtual DateTime? SentAt { get; set; }

		/// <summary>
		/// Дата и время создания записи (UTC)
		/// </summary>
		public virtual DateTime? CreatedAt { get; set; }

		/// <summary>
		/// Дата и время последнего изменения (UTC)
		/// </summary>
		public virtual DateTime? UpdatedAt { get; set; }
	}
}
