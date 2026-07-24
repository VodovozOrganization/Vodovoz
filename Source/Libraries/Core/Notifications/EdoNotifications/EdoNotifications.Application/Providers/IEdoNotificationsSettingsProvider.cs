using EdoNotifications.Contracts;
using TransactionalOutbox.Abstractions;
using Vodovoz.Core.Domain.Edo;

namespace EdoNotifications.Application.Providers
{
	/// <summary>
	/// Провайдер настроек для ЭДО уведомлений
	/// </summary>
	public interface IEdoNotificationsSettingsProvider : IOutboxSettingsProvider<EdoNotificationMessage>
	{
		/// <summary>
		/// Получение настроек для ЭДО уведомлений
		/// </summary>
		/// <param name="notificationDomainEvent"></param>
		/// <returns></returns>
		EdoNotificationSetting GetEdoNotificationSetting(EdoNotificationMessage edoNotification);
	}
}
