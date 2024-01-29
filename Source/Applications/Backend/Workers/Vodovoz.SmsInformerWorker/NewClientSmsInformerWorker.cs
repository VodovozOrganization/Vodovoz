using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Sms.External.Interface;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Sms;
using Vodovoz.EntityRepositories.SmsNotifications;
using Vodovoz.SmsInformerWorker.Options;
using Vodovoz.SmsInformerWorker.Services;

namespace Vodovoz.SmsInformerWorker
{
	/// <summary>
	/// Отправляет смс уведомление при проведении первого заказа для нового клиента
	/// </summary>
	internal class NewClientSmsInformerWorker : SmsInformerWorkerBase
	{
		private readonly ISmsNotificationRepository _smsNotificationRepository;

		public NewClientSmsInformerWorker(
			IOptions<SmsInformerOptions> options,
			ILogger<NewClientSmsInformerWorker> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISmsNotificationRepository smsNotificationRepository,
			ISmsSender smsSender,
			ISmsBalanceNotifier smsBalanceNotifier,
			ILowBalanceNotificationService lowBalanceNotificationService)
			: base(options, logger, unitOfWorkFactory, smsSender, smsBalanceNotifier, lowBalanceNotificationService)
		{
			_smsNotificationRepository = smsNotificationRepository
				?? throw new ArgumentNullException(nameof(smsNotificationRepository));
		}

		public override IEnumerable<SmsNotification> GetNotifications(IUnitOfWork unitOfWork) =>
			_smsNotificationRepository.GetUnsendedNewClientSmsNotifications(unitOfWork);

		public override void SendNotification(SmsNotification notification)
		{
			try
			{
				SmsMessage smsMessage = new SmsMessage(notification.MobilePhone, notification.Id.ToString(), notification.MessageText);

				var result = _smsSender.SendSms(smsMessage);
				_logger.LogInformation("Отправлено уведомление новому клиенту. Тел.: {MobilePhoneNumber}, результат: {SendingResult}", smsMessage.MobilePhoneNumber, result.GetEnumTitle());

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
			catch(Exception ex)
			{
				notification.Status = SmsNotificationStatus.Error;
				notification.ErrorDescription = $"Ошибка при отправке смс сообщения. {ex.Message}";
				_logger.LogError(ex, notification.ErrorDescription);
			}
		}

		protected override void OnStopService()
		{
			_logger.LogInformation("Остановлена отправка смс уведомлений новых клиентов.");
			base.OnStopService();
		}
	}
}
