using System;
using System.Threading.Tasks;
using CustomerNotifications.Contracts.Messages;
using CustomerNotifications.Contracts.Providers;
using CustomerNotifications.Publisher.Cache;

namespace CustomerNotifications.Publisher.Services
{
	/// <summary>
	/// Базовый класс для издателей уведомлений, обеспечивающий проверку повторной отправки.
	/// </summary>
	public abstract class BaseCustomerNotificationPublisher
	{
		private readonly IOnlineOrderNotificationSettingsProvider _notificationSettingsProvider;
		private readonly ICustomerNotificationCache _cache;

		/// <summary>
		/// Создаёт экземпляр базового издателя уведомлений.
		/// </summary>
		/// <param name="notificationSettingsProvider">Настройки уведомлений</param>
		/// <param name="cache">Кэш, используемый для предотвращения повторной отправки.</param>
		protected BaseCustomerNotificationPublisher(
			IOnlineOrderNotificationSettingsProvider notificationSettingsProvider,
			ICustomerNotificationCache cache)
		{
			_notificationSettingsProvider = notificationSettingsProvider ?? throw new ArgumentNullException(nameof(notificationSettingsProvider));
			_cache = cache ?? throw new ArgumentNullException(nameof(cache));
		}

		/// <summary>
		/// Выполняет отправку уведомления с учётом ограничения на повторную отправку.
		/// </summary>
		/// <param name="message">Отправляемое уведомление.</param>
		/// <param name="publishAction">Функция, выполняющая фактическую отправку.</param>

		protected async Task PublishInternalAsync(
			CustomerNotificationMessage message,
			Func<CustomerNotificationMessage, Task> publishAction)
		{
			if(message.OnlineOrderId == 0)
			{
				throw new ArgumentOutOfRangeException("Отсутствует id онлайн заказа");
			}

			if(_notificationSettingsProvider.IsDisabled(message.CustomerNotificationEventType))
			{
				return;
			}

			if(!_notificationSettingsProvider.IsDuplicateAllowed(message.CustomerNotificationEventType))
			{
				var isFirstSent = await _cache.TryMarkAsFirstSentAsync(
					message.OnlineOrderId,
					message.CustomerNotificationEventType);

				if(!isFirstSent)
				{
					return;
				}
			}

			await publishAction(message);
		}
	}
}
