using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Sms.External.Interface;
using System;
using System.Threading.Tasks;
using ExternalSmsMessage = Sms.External.Interface.SmsMessage;
using InternalSmsMessage = Sms.Internal.SmsMessage;
using Gamma.Utilities;

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

				if(smsResult.Status == ResultStatus.Ok)
				{
					_logger.LogInformation("Смс отправлена на номер {MobilePhone}", smsMessage.MobilePhone);
				}
				else
				{
					_logger.LogError("Ошибка отправки смс {ErrorDescription}", smsResult.ErrorDescription);
				}
			}
			catch(Exception ex)
			{
				smsResult = new ResultMessage
				{
					Status = ResultStatus.Error, 
					ErrorDescription = $"Ошибка при отправке смс сообщения на номер {smsMessage.MobilePhone}. {ex.Message}" 
				};
				_logger.LogError(ex, "Ошибка при отправке смс сообщения на номер {MobilePhone}.", smsMessage.MobilePhone);
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
				smsResult.Status = ResultStatus.Error;

				_logger.LogError(smsResult.ErrorDescription);

				return smsResult;
			}

			var result = _smsSender.SendSms(externalSms);

			if(result.IsSuccefullStatus())
			{
				smsResult.Status = ResultStatus.Ok;
			}
			else
			{
				smsResult.ErrorDescription = result.GetEnumTitle();
				smsResult.Status = ResultStatus.Error;
			}

			_logger.LogInformation("Отправлено уведомление. Тел.: {MobilePhoneNumber}, результат: {Result}", externalSms.MobilePhoneNumber, result.GetEnumTitle());

			return smsResult;
		}
	}
}
