using System.Threading;
using System.Threading.Tasks;

namespace Notifications.Infrastructure
{
	/// <summary>
	/// Интерфейс для публикации нотификаций, которые не требуют оборачивания в транзакцию.
	/// </summary>
	/// <typeparam name="TDomainEvent"></typeparam>
	public interface INotificationPublisher<TDomainEvent>
	{
		/// <summary>
		/// Публикация уведомления для клиента
		/// </summary>			
		Task PublishAsync(TDomainEvent notificationEvent, CancellationToken cancellationToken = default);
	}
}
