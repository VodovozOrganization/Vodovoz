using System;

namespace TransactionalOutbox.MassTransit.Domain
{
	/// <summary>
	/// Строка штатной таблицы OutboxState MassTransit (bus outbox): одна транзакция, сообщения которой ждут отправки.
	/// Схема та же, что у штатной EF Core-модели MassTransit (OutboxDbContext), но здесь запись идёт через NHibernate, не EF Core.
	/// Пишется через NHibernate (<see cref="UnitOfWorkOutboxContext"/>) на бэкенде и десктопе.
	/// Читает и удаляет служба отправки MassTransit.
	/// У каждой шины своя таблица и свой класс-наследник (папка Buses): NHibernate сопоставляет таблицу с классом.
	/// После перехода на один vhost RabbitMQ таблица останется одна, наследники по шинам не понадобятся
	/// </summary>
	public abstract class OutboxStateRecord
	{
		public virtual Guid OutboxId { get; set; }

		/// <summary>
		/// Заполняет служба отправки при захвате записи
		/// </summary>
		public virtual Guid LockId { get; set; }

		public virtual DateTime Created { get; set; }
	}
}
