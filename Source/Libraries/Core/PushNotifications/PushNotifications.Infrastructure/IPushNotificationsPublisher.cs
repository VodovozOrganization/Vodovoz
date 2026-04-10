using System.Threading;
using System.Threading.Tasks;

namespace PushNotifications.Infrastructure
{
	public interface IPushNotificationsPublisher<TDomainEvent>
	{
		Task PublishAsync(TDomainEvent notificationEvent, CancellationToken cancellationToken = default);
	}
}
