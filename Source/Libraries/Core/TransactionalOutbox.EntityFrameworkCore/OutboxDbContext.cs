using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using TransactionalOutbox.MassTransit;

namespace TransactionalOutbox.EntityFrameworkCore
{
	/// <summary>
	/// EF Core-контекст таблиц outbox MassTransit шины сервиса для штатной службы отправки MassTransit:
	/// она читает из них сообщения, отправляет и удаляет. Пишет сообщения в эти таблицы NHibernate
	/// (<see cref="UnitOfWorkOutboxContext"/>) в транзакции UoW.
	/// У каждой шины свои таблицы (префикс — имя шины, см. <see cref="OutboxBusNames"/>): служба отправки
	/// отправляет сообщения своей шиной и не должна забирать чужие. Шину задаёт
	/// <see cref="OutboxBusRegistrationExtensions.AddVodovozOutbox"/>, в процессе она одна:
	/// EF Core строит модель контекста один раз на тип.
	/// После перехода на один vhost RabbitMQ таблицы будут одни, префикс шины и <see cref="OutboxDeliveryBusName"/> не понадобятся
	/// </summary>
	public sealed class OutboxDbContext : DbContext
	{
		private readonly string _tablePrefix;

		public OutboxDbContext(DbContextOptions<OutboxDbContext> options, OutboxDeliveryBusName busName) : base(options)
		{
			_tablePrefix = (busName ?? throw new ArgumentNullException(nameof(busName))).Name;
		}

		/// <summary>
		/// Штатная EF Core-модель таблиц outbox MassTransit (OutboxState, OutboxMessage) с префиксом шины.
		/// Штатную сущность InboxState MassTransit не добавляем: inbox не используется
		/// </summary>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.AddOutboxStateEntity(x => x.ToTable($"{_tablePrefix}_outbox_state"));
			modelBuilder.AddOutboxMessageEntity(x => x.ToTable($"{_tablePrefix}_outbox_message"));
		}
	}
}
