using System.Threading;
using System.Threading.Tasks;

namespace Notifications.Core
{
	/// <summary>
	/// Строит сообщение для шины (интеграционное событие) по доменному событию уведомления
	/// </summary>
	/// <typeparam name="TDomainEvent">Доменное событие</typeparam>
	/// <typeparam name="TIntegrationEvent">Сообщение, которое публикуется в шину уведомлений</typeparam>
	public interface IIntegrationEventBuilder<TDomainEvent, TIntegrationEvent>
	{
		/// <summary>
		/// Строит интеграционное событие на основе доменного события
		/// </summary>
		Task<TIntegrationEvent> BuildAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
	}
}
