using Notifications.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Notifications.Infrastructure
{
	/// <summary>
	/// Для уведомлений, которые публикуются в шину как есть (например, уведомления ЭДО): сообщение — само событие
	/// </summary>
	public sealed class SameEventBuilder<TEvent> : IIntegrationEventBuilder<TEvent, TEvent>
	{
		public Task<TEvent> BuildAsync(TEvent domainEvent, CancellationToken cancellationToken = default) =>
			Task.FromResult(domainEvent);
	}
}
