﻿using Gamma.Utilities;
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
using Vodovoz.Zabbix.Sender;

namespace Vodovoz.SmsInformerWorker
{
	/// <summary>
	/// Отправляет смс уведомление при переносе недовоза, если клиенту не дозвонились
	/// </summary>
	internal class UndeliveryNotApprovedSmsInformerWorker : SmsInformerWorkerBase
	{
		private readonly ISmsNotificationRepository _smsNotificationRepository;

		public UndeliveryNotApprovedSmsInformerWorker(
			IOptions<SmsInformerOptions> options,
			ILogger<UndeliveryNotApprovedSmsInformerWorker> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISmsSender smsSender,
			ISmsNotificationRepository smsNotificationRepository,
			ISmsBalanceNotifier smsBalanceNotifier,
			ILowBalanceNotificationService lowBalanceNotificationService,
			IZabbixSender zabbixSender)
			: base(options, logger, unitOfWorkFactory, smsSender, smsBalanceNotifier, lowBalanceNotificationService, zabbixSender)
		{
			_smsNotificationRepository = smsNotificationRepository
				?? throw new ArgumentNullException(nameof(smsNotificationRepository));
		}

		public override void SendNotification(SmsNotification notification)
		{
			try
			{
				SmsMessage smsMessage = new SmsMessage(notification.MobilePhone, notification.Id.ToString(), notification.MessageText);

				var result = _smsSender.SendSms(smsMessage);

				_logger.LogInformation($"Отправлено уведомление о переносе недовоза. Тел.: {smsMessage.MobilePhoneNumber}, результат: {result.GetEnumTitle()}");

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
			_logger.LogInformation("Остановлена отправка смс уведомлений о переносе недовоза.");
			base.OnStopService();
		}

		public override IEnumerable<SmsNotification> GetNotifications(IUnitOfWork unitOfWork) =>
			_smsNotificationRepository.GetUnsendedUndeliveryNotApprovedSmsNotifications(unitOfWork);
	}
}
