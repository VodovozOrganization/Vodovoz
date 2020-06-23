using System;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.SmsNotifications;
using System.Linq;
using Vodovoz.Services;
using NLog;

namespace VodovozSmsInformerService
{
	public class SmsInformerService : ISmsInformerService
	{
		ILogger logger = LogManager.GetCurrentClassLogger();

		private readonly ISmsNotificationRepository smsNotificationRepository;
		private readonly ISmsNotificationServiceSettings smsNotificationServiceSettings;

		public SmsInformerService(ISmsNotificationRepository smsNotificationRepository, ISmsNotificationServiceSettings smsNotificationServiceSettings)
		{
			this.smsNotificationRepository = smsNotificationRepository ?? throw new ArgumentNullException(nameof(smsNotificationRepository));
			this.smsNotificationServiceSettings = smsNotificationServiceSettings ?? throw new ArgumentNullException(nameof(smsNotificationServiceSettings));
		}

		public bool ServiceStatus()
		{
			logger.Info("Запрос статуса службы смс уведомлений");
			try {
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
					var unsendedNotifications = smsNotificationRepository.GetUnsendedNewClientSmsNotifications(uow);
					int unsendedNotificationsCount = unsendedNotifications.Count();

					logger.Info($"Не отправленных смс уведомлений в очереди: {unsendedNotificationsCount}");
					if(unsendedNotificationsCount > smsNotificationServiceSettings.MaxUnsendedSmsNotificationsForWorkingService) {
						return false;
					}
					return true;
				}
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при проверке работоспособности службы смс уведомлений");
				return false;
			}
		}
	}
}
