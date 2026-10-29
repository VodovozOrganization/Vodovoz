using MassTransit;
using MassTransit.Middleware.Outbox;
using MassTransit.Transports;
using QS.DomainModel.UoW;
using System;
using System.Runtime.CompilerServices;

namespace TransactionalOutbox.MassTransit
{
	/// <summary>
	/// Реализация <see cref="IUnitOfWorkOutbox{TBus}"/>: сообщение пишется в таблицы outbox шины
	/// через сессию NHibernate переданного UoW (<see cref="UnitOfWorkOutboxContext"/>).
	/// Штатное MassTransit: OutboxPublishEndpointProvider и OutboxSendEndpointProvider — те же провайдеры, что в штатном outbox MassTransit,
	/// только вместо EF Core-контекста им передан наш <see cref="UnitOfWorkOutboxContext"/>
	/// </summary>
	public sealed class UnitOfWorkOutbox<TBus> : IUnitOfWorkOutbox<TBus>
		where TBus : class, IBus
	{
		private readonly TBus _bus;
		private readonly string _busName;

		// Один контекст на живой UoW: все публикации одной транзакции в эту шину попадают в один OutboxId
		private readonly ConditionalWeakTable<IUnitOfWork, UnitOfWorkOutboxContext> _contexts =
			new ConditionalWeakTable<IUnitOfWork, UnitOfWorkOutboxContext>();

		/// <param name="busName">Логическое имя шины, по нему выбираются таблицы, см. <see cref="OutboxBusNames"/></param>
		public UnitOfWorkOutbox(TBus bus, string busName)
		{
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
			_busName = string.IsNullOrWhiteSpace(busName)
				? throw new ArgumentException("Не указано имя шины", nameof(busName))
				: busName;
		}

		public IPublishEndpoint GetPublishEndpoint(IUnitOfWork unitOfWork) =>
			new PublishEndpoint(new OutboxPublishEndpointProvider(GetContext(unitOfWork), _bus));

		public ISendEndpointProvider GetSendEndpointProvider(IUnitOfWork unitOfWork) =>
			new OutboxSendEndpointProvider(GetContext(unitOfWork), _bus);

		public bool TryRemove(IUnitOfWork unitOfWork, Guid messageId) =>
			GetContext(unitOfWork).TryRemove(messageId);

		private UnitOfWorkOutboxContext GetContext(IUnitOfWork unitOfWork) =>
			_contexts.GetValue(
				unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork)),
				x => new UnitOfWorkOutboxContext(x, _busName));
	}
}
