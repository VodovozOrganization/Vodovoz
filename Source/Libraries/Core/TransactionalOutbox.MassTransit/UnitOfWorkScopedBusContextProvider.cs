using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using System;

namespace TransactionalOutbox.MassTransit
{
	/// <summary>
	/// Связывает IPublishEndpoint и ISendEndpointProvider из DI с outbox на бэкенде: сообщение пишется
	/// в транзакцию <see cref="IUnitOfWork"/> того же DI-scope и сохраняется при его Commit.
	///
	/// Где используется. Во всех сервисах бэкенда с AddVodovozOutbox (регистрируется там вместо штатного провайдера MassTransit,
	/// который регистрирует UseBusOutbox и который пишет через EF Core). Работает одинаково:
	/// <list type="bullet">
	/// <item>в HTTP-запросах WebAPI — DI-scope запроса, UoW запроса;</item>
	/// <item>в потребителях — DI-scope, который MassTransit создаёт на каждое полученное сообщение, UoW этого сообщения;</item>
	/// <item>в фоновых задачах, которые сами создают DI-scope и берут из него IUnitOfWork.</item>
	/// </list>
	/// На десктопе не используется: там IUnitOfWork нет в DI, вместо него <see cref="ExplicitOutboxOnlyScopedBusContextProvider"/>.
	///
	/// Кто использует. Код сервисов этот класс не вызывает. Его использует MassTransit: IPublishEndpoint и
	/// ISendEndpointProvider из DI MassTransit берёт из контекста, который отдаёт <see cref="IScopedBusContextProvider{TBus}"/>.
	/// EF Core здесь не используется: сообщение пишет NHibernate
	/// </summary>
	public sealed class UnitOfWorkScopedBusContextProvider : IScopedBusContextProvider<IBus>
	{
		public UnitOfWorkScopedBusContextProvider(
			Bind<IBus, IClientFactory> clientFactory,
			IUnitOfWorkOutbox<IBus> outbox,
			IServiceProvider serviceProvider)
		{
			var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

			Context = new OutboxScopedBusContext(
				clientFactory.Value,
				serviceProvider,
				outbox.GetPublishEndpoint(unitOfWork),
				outbox.GetSendEndpointProvider(unitOfWork));
		}

		/// <summary>
		/// Переопределение штатного свойства MassTransit <see cref="IScopedBusContextProvider{TBus}.Context"/>:
		/// из этого контекста MassTransit берёт IPublishEndpoint, ISendEndpointProvider и клиентов запросов для DI.
		/// IPublishEndpoint и ISendEndpointProvider здесь пишут в outbox транзакции IUnitOfWork этого DI-scope
		/// </summary>
		public ScopedBusContext Context { get; }
	}
}
