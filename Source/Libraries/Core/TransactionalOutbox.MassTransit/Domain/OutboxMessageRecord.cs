using System;

namespace TransactionalOutbox.MassTransit.Domain
{
	/// <summary>
	/// Строка штатной таблицы OutboxMessage MassTransit: сериализованное сообщение с заголовками и адресом назначения.
	/// Схема та же, что у штатной EF Core-модели MassTransit (OutboxDbContext), но здесь запись идёт через NHibernate, не EF Core.
	/// Пишется через NHibernate (<see cref="UnitOfWorkOutboxContext"/>), отправляет служба отправки MassTransit.
	/// У каждой шины своя таблица и свой класс-наследник (папка Buses).
	/// После перехода на один vhost RabbitMQ таблица останется одна, наследники по шинам не понадобятся
	/// </summary>
	public abstract class OutboxMessageRecord
	{
		public virtual long SequenceNumber { get; set; }
		public virtual Guid OutboxId { get; set; }
		public virtual Guid MessageId { get; set; }
		public virtual DateTime? EnqueueTime { get; set; }
		public virtual DateTime SentTime { get; set; }
		public virtual DateTime? ExpirationTime { get; set; }
		public virtual string Headers { get; set; }
		public virtual string Properties { get; set; }
		public virtual string ContentType { get; set; }
		public virtual string Body { get; set; }
		public virtual Guid? ConversationId { get; set; }
		public virtual Guid? CorrelationId { get; set; }
		public virtual Guid? InitiatorId { get; set; }
		public virtual Guid? RequestId { get; set; }
		public virtual Uri SourceAddress { get; set; }
		public virtual Uri DestinationAddress { get; set; }
		public virtual Uri ResponseAddress { get; set; }
		public virtual Uri FaultAddress { get; set; }
	}
}
