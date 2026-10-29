using MassTransit;
using MassTransit.Clients;
using MassTransit.DependencyInjection;
using MassTransit.Transports;
using System;

namespace TransactionalOutbox.MassTransit
{
	/// <summary>
	/// Запрещает на десктопе публиковать сообщения через IPublishEndpoint и ISendEndpointProvider (MassTransit) из DI.
	///
	/// Зачем. На десктопе сообщение должно сохраниться в outbox в той же транзакции, что и данные диалога.
	/// На бэкенде IUnitOfWork берётся из DI — один на DI-scope (HTTP-запрос, сообщение), и IPublishEndpoint из того же scope
	/// пишет в его транзакцию. На десктопе IUnitOfWork в DI не регистрируется: диалог создаёт свой UoW сам через
	/// IUnitOfWorkFactory (часто не один, вложенные диалоги работают в UoW родителя) и передаёт его в сервисы параметром.
	/// Привязать IPublishEndpoint из DI не к чему — он отправил бы сообщение сразу в RabbitMQ, мимо outbox.
	/// Поэтому на десктопе публикуют только через
	/// IUnitOfWorkOutbox&lt;IBus&gt;.GetPublishEndpoint(uow), а публикация через IPublishEndpoint из DI бросает
	/// исключение с этой подсказкой.
	///
	/// Где используется. Только на десктопе (Applications/Desktop/Vodovoz, DependencyInjection:
	/// AddExplicitUnitOfWorkOutbox&lt;IBus&gt;(OutboxBusNames.Pacs)) — и в диалогах, и в потребителях телефонии Pacs.
	/// На бэкенде вместо него <see cref="UnitOfWorkScopedBusContextProvider"/> (AddVodovozOutbox).
	///
	/// Кто использует. Код десктопа этот класс не вызывает. Его использует MassTransit: IPublishEndpoint и
	/// ISendEndpointProvider из DI MassTransit берёт из контекста, который отдаёт <see cref="IScopedBusContextProvider{TBus}"/>.
	/// Штатную реализацию этого интерфейса (MassTransit) AddExplicitUnitOfWorkOutbox на десктопе заменяет этим классом.
	///
	/// Как работает. DI создаёт класс заново в каждом DI-scope, и класс выбирает контекст по тому, зачем создан scope:
	/// <list type="bullet">
	/// <item>Scope обработки сообщения из RabbitMQ. Десктоп тоже получает сообщения: телефония Pacs подписана на события
	/// (OperatorStateConsumer, PacsCallEventConsumer и др.). На каждое полученное сообщение MassTransit создаёт DI-scope
	/// и кладёт в <see cref="ScopedConsumeContextProvider"/> (MassTransit) контекст этого сообщения — так класс и понимает, что он
	/// внутри потребителя (HasContext = true). Тогда отдаётся штатный контекст MassTransit: публикация из потребителя
	/// идёт сразу в RabbitMQ, как без outbox. Телефония в outbox не входит.</item>
	/// <item>Любой другой scope (диалоги, модели представлений, сервисы десктопа): HasContext = false. Тогда отдаётся
	/// контекст, где публикация и отправка бросают исключение (ForbiddenPublishEndpointProvider,
	/// ForbiddenSendEndpointProvider). Запросы с ожиданием ответа (IRequestClient&lt;TRequest&gt; (MassTransit)) в нём
	/// работают как обычно, сразу в RabbitMQ: ответ нужен сразу, а через outbox запрос ушёл бы только после коммита.</item>
	/// </list>
	/// </summary>
	public sealed partial class ExplicitOutboxOnlyScopedBusContextProvider : IScopedBusContextProvider<IBus>
	{
		internal const string Message =
			"Здесь IPublishEndpoint и ISendEndpointProvider из DI не связаны с UoW и отправили бы "
			+ "сообщение мимо outbox. Публикуйте в UoW, в которой меняете данные: "
			+ "IUnitOfWorkOutbox<IBus>.GetPublishEndpoint(uow).Publish(...), затем uow.Commit()";

		public ExplicitOutboxOnlyScopedBusContextProvider(
			Bind<IBus, IClientFactory> clientFactory,
			ScopedConsumeContextProvider consumeContextProvider,
			IServiceProvider serviceProvider)
		{
			Context = consumeContextProvider.HasContext
				? new ConsumeContextScopedBusContext(consumeContextProvider.GetContext(), clientFactory.Value)
				: (ScopedBusContext)new OutboxScopedBusContext(
					clientFactory.Value,
					serviceProvider,
					new PublishEndpoint(new ForbiddenPublishEndpointProvider()),
					new ForbiddenSendEndpointProvider());
		}

		/// <summary>
		/// Переопределение штатного свойства MassTransit <see cref="IScopedBusContextProvider{TBus}.Context"/>:
		/// из этого контекста MassTransit берёт IPublishEndpoint, ISendEndpointProvider и клиентов запросов для DI.
		/// IPublishEndpoint и ISendEndpointProvider здесь на десктопе вне потребителя бросают исключение, в потребителе работают штатно
		/// </summary>
		public ScopedBusContext Context { get; }
	}
}
