using System;
using System.Threading.Tasks;
using CustomerNotifications.Contracts.Messages;
using CustomerNotifications.Contracts.Providers;
using CustomerNotifications.Publisher.Cache;
using MassTransit;

namespace CustomerNotifications.Publisher.Services
{
	/// <summary>
	/// Адаптер издателя уведомлений для десктопного приложения,
	/// использующий стандартный <see cref="IPublishEndpoint"/> MassTransit.
	/// </summary>
	public class CustomerNotificationPublisherDesktopAdapter 
		: BaseCustomerNotificationPublisher, ICustomerNotificationPublisher
	{
		private readonly IPublishEndpoint _publishEndpoint;

		/// <summary>
		/// Создаёт экземпляр адаптера издателя уведомлений для десктопа.
		/// </summary>
		/// <param name="publishEndpoint">Точка публикации сообщений MassTransit.</param>
		/// <param name="cache">Кэш для предотвращения повторной отправки уведомлений.</param>
		public CustomerNotificationPublisherDesktopAdapter(
			IPublishEndpoint publishEndpoint,
			IOnlineOrderNotificationSettingsProvider notificationSettingsProvider,
			ICustomerNotificationCache cache)
			: base(notificationSettingsProvider, cache)
		{
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		/// <summary>
		/// Публикует уведомление через десктопный адаптер.
		/// </summary>
		/// <param name="message">Отправляемое уведомление.</param>
		public Task PublishAsync(CustomerNotificationMessage message)
		{
			return PublishInternalAsync(
				message,
				m => _publishEndpoint.Publish(m));
		}
	}
}
