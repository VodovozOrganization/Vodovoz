using MassTransit;
using MassTransit.Clients;
using MassTransit.DependencyInjection;
using System;

namespace TransactionalOutbox.MassTransit
{
	/// <summary>
	/// Контекст шины для одного DI-scope: из него MassTransit берёт то, что код получает из DI, —
	/// IPublishEndpoint, ISendEndpointProvider (MassTransit) и клиентов запросов (IRequestClient&lt;T&gt; (MassTransit)).
	/// Создают его <see cref="UnitOfWorkScopedBusContextProvider"/> (бэкенд) и
	/// <see cref="ExplicitOutboxOnlyScopedBusContextProvider"/> (десктоп вне потребителя).
	/// <list type="bullet">
	/// <item>PublishEndpoint и SendEndpointProvider (свойства ScopedBusContext MassTransit) передаёт создатель: на бэкенде они пишут сообщение в outbox
	/// транзакции UoW, на десктопе бросают исключение;</item>
	/// <item>ClientFactory — штатная фабрика клиентов запросов MassTransit. Запрос с ожиданием ответа
	/// (IRequestClient&lt;T&gt;.GetResponse) отправляется сразу в RabbitMQ, без outbox: ответ нужен сразу,
	/// а через outbox запрос ушёл бы только после коммита.</item>
	/// </list>
	/// </summary>
	internal sealed class OutboxScopedBusContext : ScopedBusContext
	{
		private readonly IClientFactory _clientFactory;
		private readonly IServiceProvider _serviceProvider;
		private IScopedClientFactory _scopedClientFactory;

		public OutboxScopedBusContext(
			IClientFactory clientFactory,
			IServiceProvider serviceProvider,
			IPublishEndpoint publishEndpoint,
			ISendEndpointProvider sendEndpointProvider)
		{
			_clientFactory = clientFactory;
			_serviceProvider = serviceProvider;
			PublishEndpoint = publishEndpoint;
			SendEndpointProvider = sendEndpointProvider;
		}

		/// <summary>
		/// Переопределение штатного свойства MassTransit <see cref="ScopedBusContext.PublishEndpoint"/>:
		/// отсюда MassTransit берёт IPublishEndpoint для DI. Значение передаёт создатель контекста
		/// </summary>
		public IPublishEndpoint PublishEndpoint { get; }

		/// <summary>
		/// Переопределение штатного свойства MassTransit <see cref="ScopedBusContext.SendEndpointProvider"/>:
		/// отсюда MassTransit берёт ISendEndpointProvider для DI. Значение передаёт создатель контекста
		/// </summary>
		public ISendEndpointProvider SendEndpointProvider { get; }

		/// <summary>
		/// Переопределение штатного свойства MassTransit <see cref="ScopedBusContext.ClientFactory"/>:
		/// отсюда MassTransit берёт клиентов запросов (IRequestClient&lt;T&gt;) для DI. Собрано из штатных классов MassTransit
		/// поверх обычной шины, поэтому запросы уходят сразу в RabbitMQ, без outbox
		/// </summary>
		public IScopedClientFactory ClientFactory =>
			_scopedClientFactory ??= new ScopedClientFactory(
				new ClientFactory(new ScopedClientFactoryContext(_clientFactory, _serviceProvider)), null);
	}
}
