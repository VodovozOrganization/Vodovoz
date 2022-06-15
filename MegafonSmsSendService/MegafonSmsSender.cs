using MegafonSmsAPI;
using NLog;
using QS.Utilities.Numeric;
using SmsSendInterface;
using System;
using System.Threading.Tasks;
using Vodovoz.Services;

namespace MegafonSmsSendService
{
	public class MegafonSmsSender : ISmsSender, ISmsBalanceNotifier
	{
		private static Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly ISmsSettings _smsSettings;
		private MegafonSmsClient _client;
		private PhoneFormatter _phoneFormatter;

		public MegafonSmsSender(string login, string password, ISmsSettings smsSettings)
		{
			_client = new MegafonSmsClient(login, password);
			_smsSettings = smsSettings ?? throw new ArgumentNullException(nameof(smsSettings));
			_phoneFormatter = new PhoneFormatter(PhoneFormat.DigitsTen);
		}

		public BalanceResponse GetBalanceResponse => new BalanceResponse { Status = BalanceResponseStatus.Error };

		public event EventHandler<SmsBalanceEventArgs> OnBalanceChange;

		public ISmsSendResult SendSms(ISmsMessage message)
		{
			var task = SendSmsAsync(message);
			task.Wait();
			return task.Result;
		}

		public async Task<ISmsSendResult> SendSmsAsync(ISmsMessage message)
		{
			var formattedPhone = $"7{_phoneFormatter.FormatString(message.MobilePhoneNumber)}";
			if(!ulong.TryParse(formattedPhone, out ulong phone))
			{
				throw new ArgumentException($"Неудалось распарсить номер телефона: {message.MobilePhoneNumber}. Форматированный номер: {formattedPhone}");
			}

			var sms = new SmsMessage
			{
				MessageId = message.LocalId,
				Phone = phone,
				Text = message.MessageText,
				Sender = _smsSettings.MegafonSenderName,
				CallbackUrl = null
			};

			var sendResult = new SendResult();
			var result = await _client.SendSmsAsync(sms);
			if(result.Status.Code == 0)
			{
				sendResult.Status = SmsSentStatus.Accepted;
			}
			else
			{
				sendResult.Status = SmsSentStatus.UnknownError;
				var sendStatus = result.Status;
				if(sendStatus == null)
				{
					sendResult.Description = "sendStatus null";
				}
				else
				{
					sendResult.Description = result.Status.Status;
				}
				_logger.Info($"Не удалось отправить смс. " +
					$"Код ошибки: {sendStatus?.Code}. {result?.Status}. " +
					$"Подробнее: {sendStatus?.Payload}");
			}
			return sendResult;
		}
	}
}
