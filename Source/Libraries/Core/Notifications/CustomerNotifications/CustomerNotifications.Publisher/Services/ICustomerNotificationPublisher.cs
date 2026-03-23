using System.Threading.Tasks;
using CustomerNotifications.Contracts.Messages;

namespace CustomerNotifications.Publisher.Services
{
	/// <summary>
	/// Интерфейс издателя уведомлений для отправки сообщений клиенту.
	/// </summary>
	public interface ICustomerNotificationPublisher
	{
		/// <summary>
		/// Публикует уведомление с возможностью запрета повторной отправки.
		/// </summary>
		/// <param name="message">Отправляемое уведомление.</param>
		Task PublishAsync(CustomerNotificationMessage message);
	}
}
