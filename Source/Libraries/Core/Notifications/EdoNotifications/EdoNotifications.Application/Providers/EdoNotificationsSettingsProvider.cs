using EdoNotifications.Contracts;
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
				var setting = _edoNotificationSettingsRepository.Get(uow, x => x.EdoNotificationType == edoNotification.EdoNotificationType).SingleOrDefault();

				if(setting == null)
				{
					throw new InvalidOperationException(
						$"Не найдена настройка для типа события '{edoNotification.EdoNotificationType}'.");
				}

				return setting;
			}
		}

		public bool IsDuplicateAllowed(EdoNotificationMessage edoNotification)
			=> GetEdoNotificationSetting(edoNotification)?.AllowDuplicateNotifications ?? false;

		public bool IsDisabled(EdoNotificationMessage edoNotification)
			=> GetEdoNotificationSetting(edoNotification)?.NotificationDisabled ?? true;
	}
}
