using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sms;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using NLog;
using Vodovoz.EntityRepositories.SmsNotifications;
using Sms.External.Interface;
using Gamma.Utilities;

namespace VodovozSmsInformerService
{
	/// <summary>
	/// Отправляет смс уведомление при проведении первого заказа для нового клиента
	/// </summary>
	public class NewClientSmsInformer
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly ISmsSender smsSender;
		private readonly ISmsNotificationRepository smsNotificationRepository;
		private Timer timer;
		private bool sendingInProgress = false;
		private const int refreshInterval = 60000;

		public NewClientSmsInformer(ISmsSender smsSender, ISmsNotificationRepository smsNotificationRepository)
		{
			this.smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
			this.smsNotificationRepository = smsNotificationRepository ?? throw new ArgumentNullException(nameof(smsNotificationRepository));
		}

		public void Start()
		{
			timer = new Timer(refreshInterval);
			timer.Elapsed += Timer_Elapsed;
			timer.Start();
			logger.Info($"Запущена отправка смс уведомлений. Проверка новых уведомлений каждые {refreshInterval/1000} сек.");
		}

		public void Stop()
		{
			timer?.Stop();
			timer?.Dispose();
			timer = null;
			logger.Info($"Остановлена отправка смс уведомлений.");
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			SendNewNotifications();
		}

		private void CloseExpiredNotifications(IUnitOfWork uow, IEnumerable<NewClientSmsNotification> notifications)
		{
			var expiredNotifications = notifications.Where(x => x.Status == SmsNotificationStatus.New)
				//проверка даты без времени
				.Where(x => x.ExpiredTime < DateTime.Today);
			if(expiredNotifications.Any()) {
				logger.Info($"Были закрыты без отправки следующие просроченные уведомления: {string.Join(", ", expiredNotifications.Select(x => x.Id))}");
			}
			foreach(var expiredNotification in expiredNotifications) {
				expiredNotification.Status = SmsNotificationStatus.SendExpired;
				uow.Save(expiredNotification);
			}
		}

		private void SendNewNotifications()
		{
			logger.Debug($"Новый вызов отправки смс уведомлений");
			if(sendingInProgress) {
				logger.Debug($"Вывоз новой отправки до завершения предыдущей!");
				return;
			}
			sendingInProgress = true;
			try {
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
					var newNotifications = smsNotificationRepository.GetUnsendedNewClientSmsNotifications(uow);
					if(!newNotifications.Any()) {
						return;
					}
					//закрытие просроченных уведомлений
					CloseExpiredNotifications(uow, newNotifications);
					newNotifications = newNotifications.Where(x => x.Status == SmsNotificationStatus.New).ToList();

					foreach(var notification in newNotifications) {
						SendNotification(notification);
						uow.Save(notification);
					}
					uow.Commit();
				}
			}
			catch(Exception ex) {
				logger.Fatal(ex);
			}
			finally {
				sendingInProgress = false;
			}
		}

		private void SendNotification(NewClientSmsNotification notification)
		{
			try {
				SmsMessage smsMessage = new SmsMessage(notification.MobilePhone, notification.Id.ToString(), notification.MessageText);
				var result = smsSender.SendSms(smsMessage);
				logger.Info($"Отправлено уведомление. Тел.: {smsMessage.MobilePhoneNumber}, результат: {result.GetEnumTitle()}");

				if(result.IsSuccefullStatus())
				{
					notification.Status = SmsNotificationStatus.Accepted;
				}
				else
				{
					notification.ErrorDescription = result.GetEnumTitle();
					notification.Status = SmsNotificationStatus.Error;
				}
			}
			catch(Exception ex) {
				notification.Status = SmsNotificationStatus.Error;
				notification.ErrorDescription = $"Ошибка при отправке смс сообщения. {ex.Message}";
				logger.Error(ex);
			}
		}
	}
}
