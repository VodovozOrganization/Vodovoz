using System.Data;
using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using TransactionalOutbox.MassTransit;

namespace TransactionalOutbox.EntityFrameworkCore
{
	public static class OutboxBusRegistrationExtensions
	{
		/// <summary>
		/// Подключает outbox к основной шине сервиса бэкенда. После вызова:
		/// <list type="bullet">
		/// <item>публикация через IPublishEndpoint / ISendEndpointProvider (MassTransit) из DI сохраняет сообщение в транзакции IUnitOfWork
		/// того же DI-scope (HTTP-запроса или обрабатываемого сообщения); сообщение уходит после Commit этого UoW
		/// (<see cref="UnitOfWorkScopedBusContextProvider"/>);</item>
		/// <item>если сервис не зарегистрировал свой IUnitOfWork на DI-scope, регистрируется UoW из фабрики;</item>
		/// <item>для UoW, созданных вручную через IUnitOfWorkFactory (воркеры, рутины, общие библиотеки), публикуют через
		/// <see cref="IUnitOfWorkOutbox{TBus}"/>: GetPublishEndpoint(uow) сохраняет сообщение в транзакции этого UoW;</item>
		/// <item>в сервисе работает служба отправки MassTransit: она читает сохранённые сообщения из таблиц шины
		/// <paramref name="busName"/> и отправляет их в RabbitMQ.</item>
		/// </list>
		/// Что здесь из штатного outbox MassTransit, а что наше:
		/// <list type="bullet">
		/// <item>штатное MassTransit (пакет MassTransit.EntityFrameworkCore): таблицы outbox и служба отправки, которая читает их через EF Core;</item>
		/// <item>наше: запись сообщений в эти таблицы. В штатном outbox MassTransit её делает EF Core в транзакции DbContext,
		/// у нас — NHibernate в транзакции UoW, потому что бизнес-данные в NHibernate.</item>
		/// </list>
		/// Inbox (защиту потребителя от повторной доставки) не включаем: потребители должны переживать повтор сообщения.
		/// Сервис отправляет сообщения одной шины: повторный вызов с другой шиной — ошибка при старте
		/// </summary>
		/// <param name="busName">Шина сервиса (vhost), см. <see cref="OutboxBusNames"/>, например <see cref="OutboxBusNames.Pacs"/></param>
		public static IBusRegistrationConfigurator AddVodovozOutbox(this IBusRegistrationConfigurator busConfigurator, string busName)
		{
			busConfigurator.AddOutboxDbContext(busName);

			// Штатный outbox MassTransit на EF Core. Из него используем только таблицы и службу отправки
			busConfigurator.AddEntityFrameworkOutbox<OutboxDbContext>(outbox =>
			{
				outbox.UseMySql();
				// Штатная служба отправки MassTransit: читает таблицы через EF Core (OutboxDbContext), отправляет и удаляет строки
				outbox.UseBusOutbox();
				// Штатная очистка inbox MassTransit не нужна: inbox не используем
				outbox.DisableInboxCleanupService();
				// Уровень изоляции транзакций службы отправки, требует binlog_format ROW или MIXED
				outbox.IsolationLevel = IsolationLevel.ReadCommitted;
			});

			// UseBusOutbox регистрирует штатный контекст шины MassTransit: IPublishEndpoint из DI писал бы сообщения через EF Core
			// в транзакцию DbContext. Заменяем его своим: запись через NHibernate в транзакцию IUnitOfWork
			busConfigurator.Replace(ServiceDescriptor.Scoped<IScopedBusContextProvider<IBus>, UnitOfWorkScopedBusContextProvider>());

			// UoW DI-scope (запроса, сообщения), если сервис не зарегистрировал свой
			busConfigurator.TryAddScoped<IUnitOfWork>(serviceProvider =>
				serviceProvider.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot());

			busConfigurator.AddExplicitUnitOfWorkOutbox<IBus>(busName);

			return busConfigurator;
		}
	}
}
