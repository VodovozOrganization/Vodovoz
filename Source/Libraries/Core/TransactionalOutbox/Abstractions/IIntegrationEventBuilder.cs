using System.Threading;
using System.Threading.Tasks;

namespace TransactionalOutbox.Abstractions
{
	/// <summary>
	/// Интерфейс для построения интеграционных событий на основе доменных событий.
	/// </summary>
	/// <typeparam name="TDomainEvent"></typeparam>
	/// <typeparam name="TIntegrationEvent"></typeparam>
	public interface IIntegrationEventBuilder<TDomainEvent, TIntegrationEvent>
	{
		/// <summary>
		/// Строит интеграционное событие на основе доменного события.
		/// </summary>
		
		Task<TIntegrationEvent> BuildAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
	}
}
