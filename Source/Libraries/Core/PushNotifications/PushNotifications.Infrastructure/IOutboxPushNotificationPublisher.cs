using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using TransactionalOutbox.Contracts;

namespace PushNotifications.Infrastructure
{
	public interface IOutboxPushNotificationPublisher<in TDomainEvent>
	{
		Task PublishAsync(IUnitOfWork unitOfWork, TDomainEvent notificationEvent, CancellationToken cancellationToken = default);
		void Publish(IUnitOfWork unitOfWork, TDomainEvent notificationEvent);
	}
}
