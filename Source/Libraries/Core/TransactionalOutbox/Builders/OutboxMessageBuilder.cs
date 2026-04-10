using System;
using System.Threading;
using System.Threading.Tasks;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Contracts;
using TransactionalOutbox.Domain;

namespace TransactionalOutbox.Builders
{
	public class OutboxMessageBuilder<TDomainEvent, TIntegrationEvent> : IOutboxMessageBuilder<TDomainEvent>
		where TDomainEvent : IOutboxDomainEvent
	{
		private readonly IIntegrationEventBuilder<TDomainEvent, TIntegrationEvent> _integrationEventBuilder;

		public OutboxMessageBuilder(IIntegrationEventBuilder<TDomainEvent, TIntegrationEvent> integrationEventBuilder)
		{
			_integrationEventBuilder = integrationEventBuilder ?? throw new ArgumentNullException(nameof(integrationEventBuilder));
		}

		public async Task<OutboxMessage> BuildAsync(
			TDomainEvent domainEvent,
			CancellationToken cancellationToken = default)
		{
			var integrationEvent = await _integrationEventBuilder.BuildAsync(domainEvent, cancellationToken);

			return new OutboxMessage(domainEvent, integrationEvent);
		}
	}
}
