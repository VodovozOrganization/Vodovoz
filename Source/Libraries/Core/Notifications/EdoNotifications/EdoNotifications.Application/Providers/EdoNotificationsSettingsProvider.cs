using EdoNotifications.Contracts;
using Notifications.Core;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;

namespace EdoNotifications.Application.Providers
{
	public class EdoNotificationsSettingsProvider : IEdoNotificationsSettingsProvider
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<EdoNotificationSetting> _edoNotificationSettingsRepository;

		public EdoNotificationsSettingsProvider(IUnitOfWorkFactory uowFactory, IGenericRepository<EdoNotificationSetting> edoNotificationSettingsRepository)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_edoNotificationSettingsRepository = edoNotificationSettingsRepository ?? throw new ArgumentNullException(nameof(edoNotificationSettingsRepository));
		}

		public EdoNotificationSetting GetEdoNotificationSetting(EdoNotificationMessage edoNotification)
		{
			if(edoNotification == null)
			{
				throw new ArgumentNullException(nameof(edoNotification));
			}

			using(var uow = _uowFactory.CreateWithoutRoot("Получение настройки уведомлений об ЭДО"))
			{
				var setting = _edoNotificationSettingsRepository
					.Get(uow, x => x.EdoNotificationType == edoNotification.EdoNotificationType)
					.SingleOrDefault();

				if(setting == null)
				{
					throw new InvalidOperationException(
						$"Не найдена настройка для типа уведомления '{edoNotification.EdoNotificationType}' (edo_notification_settings).");
				}

				return setting;
			}
		}

		public NotificationSendingSettings GetSendingSettings(EdoNotificationMessage edoNotification)
		{
			var setting = GetEdoNotificationSetting(edoNotification);
			return new NotificationSendingSettings(setting.NotificationDisabled, setting.AllowDuplicateNotifications);
		}
	}
}
