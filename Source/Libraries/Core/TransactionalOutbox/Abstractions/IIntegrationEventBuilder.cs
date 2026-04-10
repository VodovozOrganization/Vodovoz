using System.Threading;
using System.Threading.Tasks;

namespace TransactionalOutbox.Abstractions
{
	public interface IIntegrationEventBuilder<TDomainEvent, TIntegrationEvent>
	{
		Task<TIntegrationEvent> BuildAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
	}
}
