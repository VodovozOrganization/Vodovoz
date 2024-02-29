using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Sms.External.Interface;
using System;
using System.Linq;
using Vodovoz.Domain.Sms;
using Vodovoz.Services;

namespace Vodovoz.SmsInformerWorker.Services
{
	internal class LowBalanceNotificationService : ILowBalanceNotificationService
	{
		private readonly ILogger<LowBalanceNotificationService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISmsSender _smsSender;
		private readonly ISmsNotifierSettings _smsNotifierSettings;

		public LowBalanceNotificationService(
			ILogger<LowBalanceNotificationService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISmsSender smsSender,
			ISmsNotifierSettings smsNotifierSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
			_smsNotifierSettings = smsNotifierSettings ?? throw new ArgumentNullException(nameof(smsNotifierSettings));
		}

		public void BalanceNotifierOnBalanceChange(object sender, SmsBalanceEventArgs e)
		{
			try
			{
				if(e.BalanceType != BalanceType.CurrencyBalance)
				{
					return;
				}

				decimal currentBalanceLevel = e.Balance;
				decimal minBalanceLevel = _smsNotifierSettings.LowBalanceLevel;

				var unformedPhone = _smsNotifierSettings.LowBalanceNotifiedPhone;

				if(string.IsNullOrWhiteSpace(unformedPhone))
				{
					return;
				}

				string notifiedPhone = FormatPhone(unformedPhone)
					?? throw new InvalidProgramException(
					$"Неверно заполнен номер телефона ({unformedPhone}) для уведомления о низком балансе денежных средств на счете");

				string notifyText = _smsNotifierSettings
					.LowBalanceNotifyText
					.Replace("$balance$", currentBalanceLevel.ToString("0.##"));

				using var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot();

				var notification = unitOfWork.Session.QueryOver<LowBalanceSmsNotification>()
					.Where(x => x.Status == SmsNotificationStatus.New)
					.OrderBy(x => x.NotifyTime).Desc
					.Take(1)
					.SingleOrDefault();

				if(notification == null)
				{
					if(currentBalanceLevel < minBalanceLevel)
					{
						notification = new LowBalanceSmsNotification()
						{
							Balance = currentBalanceLevel,
							MessageText = notifyText,
							MobilePhone = notifiedPhone,
							NotifyTime = DateTime.Now,
							Status = SmsNotificationStatus.New
						};

						unitOfWork.Save(notification);
						unitOfWork.Commit();

						_logger.LogInformation("Создано новое уведомление о низком балансе на счете");
						var result = _smsSender.SendSms(new SmsMessage(notification.MobilePhone, notification.Id.ToString(), notification.MessageText));

						if(result.IsSuccefullStatus())
						{
							notification.Status = SmsNotificationStatus.Accepted;
						}
						else
						{
							notification.ErrorDescription = result.GetEnumTitle();
							notification.Status = SmsNotificationStatus.Error;
						}

						unitOfWork.Save(notification);
						unitOfWork.Commit();
					}
				}
				else
				{
					if(currentBalanceLevel < minBalanceLevel)
					{
						//Ничего не отправляем, так как уже было уведомление о низком балансе
						return;
					}
					if(currentBalanceLevel > minBalanceLevel)
					{
						//Меняем статус уведомления на принятый, так как было поступление средств на счет
						notification.Status = SmsNotificationStatus.Accepted;
						unitOfWork.Save(notification);
						unitOfWork.Commit();
						_logger.LogInformation("Баланс был пополнен, уведомление о низком балансе закрыто");
					}
				}
			}
			catch(Exception ex)
			{
				_logger.LogCritical(ex, ex.Message);
			}
		}

		private string FormatPhone(string phone)
		{
			string stringPhoneNumber = phone
				.TrimStart('+')
				.TrimStart('7')
				.TrimStart('8');

			if(stringPhoneNumber.Length == 0 || stringPhoneNumber.First() != '9' || stringPhoneNumber.Length != 10)
			{
				return null;
			}

			return $"+7{stringPhoneNumber}";
		}
	}
}
