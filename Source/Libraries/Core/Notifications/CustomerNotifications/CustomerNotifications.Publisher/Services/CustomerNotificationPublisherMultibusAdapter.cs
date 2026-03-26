using System;
using System.Threading.Tasks;
using CustomerNotifications.Contracts.Messages;
using CustomerNotifications.Contracts.Providers;
using CustomerNotifications.Publisher.Bus;
using CustomerNotifications.Publisher.Cache;

namespace CustomerNotifications.Publisher.Services
{
	/// <summary>
	/// Адаптер издателя уведомлений, использующий выделенную multibus‑шину
	/// для отправки сообщений о клиентских уведомлениях.
	/// </summary>
	public class CustomerNotificationPublisherMultibusAdapter 
		: BaseCustomerNotificationPublisher, ICustomerNotificationPublisher
	{
		private readonly ICustomerNotificationBus _bus;

		// <summary>
		/// Создаёт экземпляр адаптера издателя, работающего через отдельную шину MassTransit.
		/// </summary>
		/// <param name="bus">Выделенная шина для публикации уведомлений.</param>
		/// <param name="cache">Кэш для предотвращения повторной отправки уведомлений.</param>
		public CustomerNotificationPublisherMultibusAdapter(
			ICustomerNotificationBus bus,
			IOnlineOrderNotificationSettingsProvider notificationSettingsProvider,
			ICustomerNotificationCache cache)
			: base(notificationSettingsProvider, cache)
		{
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}

		/// <summary>
		/// Публикует уведомление через multibus‑шину.
		/// </summary>
		/// <param name="message">Отправляемое уведомление.</param>
		public Task PublishAsync(CustomerNotificationMessage message)
		{
			return PublishInternalAsync(
				message,
				m => _bus.Publish(m));
		}
	}
}
