using System;
using QS.DomainModel.UoW;
using Vodovoz.Services;
using Vodovoz.Domain.Sms;
using System.Linq;
using NLog;
using Sms.External.Interface;
using Gamma.Utilities;

namespace VodovozSmsInformerService
{
	public class LowBalanceNotifier
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly ISmsSender smsSender;
		private readonly ISmsBalanceNotifier balanceNotifier;
		private readonly ISmsNotifierParametersProvider smsNotifierParametersProvider;

		public LowBalanceNotifier(
			ISmsSender smsSender,
			ISmsBalanceNotifier balanceNotifier,
			ISmsNotifierParametersProvider smsNotifierParametersProvider)
		{
			this.smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
			this.balanceNotifier = balanceNotifier ?? throw new ArgumentNullException(nameof(balanceNotifier));
			this.smsNotifierParametersProvider = smsNotifierParametersProvider ?? throw new ArgumentNullException(nameof(smsNotifierParametersProvider));
		}

		public void Start()
		{
			this.balanceNotifier.OnBalanceChange -= BalanceNotifier_OnBalanceChange;
			this.balanceNotifier.OnBalanceChange += BalanceNotifier_OnBalanceChange;
			logger.Info("Запущена отправка уведомлений о низком балансе денежных средств");
		}

		public void Stop()
		{
			this.balanceNotifier.OnBalanceChange -= BalanceNotifier_OnBalanceChange;
			logger.Info("Остановлена отправка уведомлений о низком балансе денежных средств");
		}

		void BalanceNotifier_OnBalanceChange(object sender, SmsBalanceEventArgs e)
		{
			try {
				if(e.BalanceType != BalanceType.CurrencyBalance) {
					return;
				}
				decimal currentBalanceLevel = e.Balance;
				decimal minBalanceLevel = smsNotifierParametersProvider.GetLowBalanceLevel();
				var unformedPhone = smsNotifierParametersProvider.GetLowBalanceNotifiedPhone();
				if(string.IsNullOrWhiteSpace(unformedPhone)) {
					return;
				}
				string notifiedPhone = FormatPhone(unformedPhone) ?? throw new InvalidProgramException(
					$"Неверно заполнен номер телефона ({unformedPhone}) для уведомления о низком балансе денежных средств на счете");
				string notifyText = smsNotifierParametersProvider.GetLowBalanceNotifyText();
				notifyText = notifyText.Replace("$balance$", currentBalanceLevel.ToString("0.##"));
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
					var notification = uow.Session.QueryOver<LowBalanceSmsNotification>()
						.Where(x => x.Status == SmsNotificationStatus.New)
						.OrderBy(x => x.NotifyTime).Desc
						.Take(1)
						.SingleOrDefault();

					if(notification == null) {
						if(currentBalanceLevel < minBalanceLevel) {
							notification = new LowBalanceSmsNotification() {
								Balance = currentBalanceLevel,
								MessageText = notifyText,
								MobilePhone = notifiedPhone,
								NotifyTime = DateTime.Now,
								Status = SmsNotificationStatus.New
							};
							uow.Save(notification);
							uow.Commit();
							logger.Info("Создано новое уведомление о низком балансе на счете");
							var result = smsSender.SendSms(new SmsMessage(notification.MobilePhone, notification.Id.ToString(), notification.MessageText));

							if(result.IsSuccefullStatus())
							{
								notification.Status = SmsNotificationStatus.Accepted;
							}
							else
							{
								notification.ErrorDescription = result.GetEnumTitle();
								notification.Status = SmsNotificationStatus.Error;
							}

							uow.Save(notification);
							uow.Commit();
						}
					} else {
						if(currentBalanceLevel < minBalanceLevel) {
							//Ничего не отправляем, так как уже было уведомление о низком балансе
							return;
						}
						if(currentBalanceLevel > minBalanceLevel) {
							//Меняем статус уведомления на принятый, так как было поступление средств на счет
							notification.Status = SmsNotificationStatus.Accepted;
							uow.Save(notification);
							uow.Commit();
							logger.Info("Баланс был пополнен, уведомление о низком балансе закрыто");
						}
					}
				}
			}
			catch(Exception ex) {
				logger.Fatal(ex);
			}
		}

		private string FormatPhone(string phone)
		{
			string stringPhoneNumber = phone.TrimStart('+').TrimStart('7').TrimStart('8');
			if(stringPhoneNumber.Length == 0 || stringPhoneNumber.First() != '9' || stringPhoneNumber.Length != 10) {
				return null;
			}
			return $"+7{stringPhoneNumber}";
		}

	}
}
