using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Sms.External.Interface;
using System;
using System.Threading.Tasks;
using ExternalSmsMessage = Sms.External.Interface.SmsMessage;
using InternalSmsMessage = Sms.Internal.SmsMessage;

namespace Sms.Internal.Service
{
	[Authorize]
	public class SmsService : SmsSender.SmsSenderBase
    {
        private readonly ILogger<SmsService> _logger;
		private readonly ISmsSender _smsSender;

		public SmsService(ILogger<SmsService> logger, ISmsSender smsSender)
		{
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
		}

		public override Task<ResultMessage> Send(InternalSmsMessage smsMessage, ServerCallContext context)
		{
			ResultMessage smsResult;

			try
			{
				smsResult = SendSms(smsMessage);
				_logger.LogInformation("Смс отправлена на номер {0}", smsMessage.MobilePhone);
			}
			catch(Exception ex)
			{
				smsResult = new ResultMessage
				{
					Status = ResultStatus.Error, 
					ErrorDescription = $"Ошибка при отправке смс сообщения на номер {smsMessage.MobilePhone}. {ex.Message}" 
				};
				_logger.LogError(ex, "Ошибка при отправке смс сообщения на номер {0}.", smsMessage.MobilePhone);
			}

			return Task.FromResult(smsResult);
		}

		private ResultMessage SendSms(InternalSmsMessage smsMessage)
		{
			ResultMessage smsResult = new ResultMessage();

			var externalSms = new ExternalSmsMessage(smsMessage.MobilePhone, smsMessage.ServerMessageId, smsMessage.MessageText);

			if(smsMessage.ExpiredTime != null && DateTime.Now > smsMessage.ExpiredTime.ToDateTime().ToLocalTime())
			{
				smsResult.ErrorDescription = "Время отправки Sms сообщения вышло";
				return smsResult;
			}

			var result = _smsSender.SendSms(externalSms);

			_logger.LogInformation("Отправлено уведомление. Тел.: {0}, результат: {1}", externalSms.MobilePhoneNumber, result.Status);

			switch(result.Status)
			{
				case SmsSentStatus.InvalidMobilePhone:
					smsResult.ErrorDescription = $"Неверно заполнен номер мобильного телефона. ({smsMessage.MobilePhone})";
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

			return smsResult;
		}
	}
}
