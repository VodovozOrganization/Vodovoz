using System.Threading;
using System.Threading.Tasks;
using TransactionalOutbox.Contracts;
using TransactionalOutbox.Domain;

namespace TransactionalOutbox.Abstractions
{
	/// <summary>
	/// Строит сообщение для Outbox
	/// </summary>
	public interface IOutboxMessageBuilder<TDomainEvent>
	{
		Task<OutboxMessage> BuildAsync(
			TDomainEvent domainEvent,
			CancellationToken cancellationToken = default);
	}
}
