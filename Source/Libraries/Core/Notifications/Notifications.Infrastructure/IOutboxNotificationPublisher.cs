using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace Notifications.Infrastructure
{
	/// <summary>
	/// Интерфейс для публикации уведомлений в рамках транзакционного аутбокса. Позволяет создавать и сохранять сообщения в аутбоксе,
	/// которые будут опубликованы после успешного коммита транзакции.
	/// </summary>
	/// <typeparam name="TDomainEvent"></typeparam>
	public interface IOutboxNotificationPublisher<in TDomainEvent>
	{
		Task PublishAsync(IUnitOfWork unitOfWork, TDomainEvent notificationEvent, CancellationToken cancellationToken = default);
		void Publish(IUnitOfWork unitOfWork, TDomainEvent notificationEvent);
	}
}
