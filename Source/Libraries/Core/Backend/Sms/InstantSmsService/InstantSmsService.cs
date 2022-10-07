using System;
using NLog;
using SmsRuSendService;
using SmsSendInterface;

namespace InstantSmsService
{
	public class InstantSmsService : IInstantSmsService
	{
		public InstantSmsService(ISmsSender smsSender)
		{
			this.smsSender = smsSender;
		}

		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISmsSender smsSender;
		decimal minBalanceValue = 5;

		public ResultMessage SendSms(InstantSmsMessage smsNotification)
		{
			ResultMessage smsResult = new ResultMessage { MessageStatus = SmsMessageStatus.Ok};
			try {
				SmsMessage smsMessage = new SmsMessage(smsNotification.MobilePhone, smsNotification.ServerMessageId, smsNotification.MessageText);

				if(DateTime.Now > smsNotification.ExpiredTime) {
					smsResult.ErrorDescription = "Время отправки Sms сообщения вышло";
					return smsResult;
				}
				var result = smsSender.SendSms(smsMessage);

				logger.Info($"Отправлено уведомление. Тел.: {smsMessage.MobilePhoneNumber}, результат: {result.Status}");

				switch(result.Status) {
				case SmsSentStatus.InvalidMobilePhone:
					smsResult.ErrorDescription = $"Неверно заполнен номер мобильного телефона. ({smsNotification.MobilePhone})";
					break;
				case SmsSentStatus.TextIsEmpty:
					smsResult.ErrorDescription = $"Не заполнен текст сообщения";
					break;
				case SmsSentStatus.SenderAddressInvalid:
					smsResult.ErrorDescription = $"Неверное имя отправителя";
					break;
				case SmsSentStatus.NotEnoughBalance:
					smsResult.ErrorDescription = $"Недостаточно средств на счете";
					break;
				case SmsSentStatus.UnknownError:
					smsResult.ErrorDescription = $"{result.Description}";
					break;
				}
			}
			catch(Exception ex) {
				smsResult.ErrorDescription = $"Ошибка при отправке смс сообщения. {ex.Message}";
				logger.Error(ex);
			}
			return smsResult;
		}

		public bool ServiceStatus()
		{
			logger.Info("Запрос статуса службы моментальных смс уведомлений");
			try {
				BalanceResponse balanceResponse = smsSender.GetBalanceResponse;
				if(balanceResponse.Status == BalanceResponseStatus.Error) {
					logger.Info($"Ошибка запроса баланса");
					return false;
				}
				if(balanceResponse.BalanceValue < minBalanceValue) {
					logger.Info($"Баланс на счёте менее {minBalanceValue} рублей");
					return false;
				}
				logger.Info($"Баланс на счёте: {balanceResponse.BalanceValue}р.");
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при проверке работоспособности службы смс уведомлений");
				return false;
			}
			return true;
		}
	}
}
