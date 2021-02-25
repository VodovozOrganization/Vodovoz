using System;
using System.Threading.Tasks;
using SmsSendInterface;
using SmsBlissAPI;
using SmsBlissAPI.Model;
using System.Linq;
using NLog;

namespace SmsBlissSendService
{
	public class SmsBlissSendController : ISmsSender, ISmsBalanceNotifier
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly SmsBlissClient smsBlissClient;
		private readonly SmsBlissAPI.Model.BalanceType balanceType;

		public SmsSendInterface.BalanceResponse GetBalanceResponse {
			get {
				SmsBlissAPI.Model.BalanceResponse balance = smsBlissClient.GetBalance();
				SmsSendInterface.BalanceResponse balanceResponse = new SmsSendInterface.BalanceResponse();

				switch(balance.Status) {
				case ResponseStatus.Ok:
					balanceResponse.Status = BalanceResponseStatus.Ok;
					break;
				case ResponseStatus.Error:
					balanceResponse.Status = BalanceResponseStatus.Error;
					break;
				}
				foreach(var item in balance.Balances) {
					if(balanceType == item.Type) {

						item.BalanceValue = item.BalanceValue.Replace('.', ',');
						if(!decimal.TryParse(item.BalanceValue, out decimal balanceValue)) {
							logger.Warn($"Невозможно преобразовать значение баланса из \"{item.BalanceValue}\" в число");
							continue;
						}
						balanceResponse.BalanceValue += balanceValue;
					}
				}
				return balanceResponse;
			}
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="T:SmsBlissSendService.SmsBlissSendController"/> class.
		/// </summary>
		/// <param name="login">Login.</param>
		/// <param name="password">Password.</param>
		/// <param name="responseBalanceType">Тип возвращаемого баланса в <see cref="GetBalanceResponse"/></param>
		public SmsBlissSendController(string login, string password, SmsSendInterface.BalanceType responseBalanceType)
		{
			smsBlissClient = new SmsBlissClient(login, password);

			balanceType = ConvertBalanceType(responseBalanceType);
		}

		#region Converters

		private SmsSendInterface.BalanceType ConvertBalanceType(SmsBlissAPI.Model.BalanceType bType)
		{
			switch(bType) {
			case SmsBlissAPI.Model.BalanceType.RUB:
				return SmsSendInterface.BalanceType.CurrencyBalance;
			case SmsBlissAPI.Model.BalanceType.SMS:
				return SmsSendInterface.BalanceType.SmsCounts;
			default: throw new ArgumentException("Unrecognized balance type");
			}
		}

		private SmsBlissAPI.Model.BalanceType ConvertBalanceType(SmsSendInterface.BalanceType bType)
		{
			switch(bType) {
			case SmsSendInterface.BalanceType.CurrencyBalance:
				return SmsBlissAPI.Model.BalanceType.RUB;
			case SmsSendInterface.BalanceType.SmsCounts:
				return SmsBlissAPI.Model.BalanceType.SMS;
			default: throw new ArgumentException("Unrecognized balance type");
			}
		}

		#endregion

		#region ISmsSender implementation

		public ISmsSendResult SendSms(ISmsMessage smsMessage)
		{
			if(smsMessage == null) {
				throw new ArgumentNullException(nameof(smsMessage));
			}

			if(!ValidateMobilePhoneNumberLength(smsMessage.MobilePhoneNumber)) {
				return new SmsBlissSentResult(SmsSentStatus.InvalidMobilePhone) { Description = "Сообщение не было отправлено, не корректная длина номера телефона" };
			}

			string phone = smsMessage.MobilePhoneNumber;
			Message message = new Message(smsMessage.LocalId, phone, smsMessage.MessageText);

			var response = smsBlissClient.SendMessages(new[] { message }, showBillingDetails: true);

			NotifyBalanceChangeOnSendSuccessfully(response);
			return ConvertToISmsSendResult(response);
		}

		public async Task<ISmsSendResult> SendSmsAsync(ISmsMessage smsMessage)
		{
			if(smsMessage == null) {
				throw new ArgumentNullException(nameof(smsMessage));
			}

			if(!ValidateMobilePhoneNumberLength(smsMessage.MobilePhoneNumber)) {
				return new SmsBlissSentResult(SmsSentStatus.InvalidMobilePhone) { Description = "Сообщение не было отправлено, не корректная длина номера телефона" };
			}

			string phone = smsMessage.MobilePhoneNumber;
			Message message = new Message(smsMessage.LocalId, phone, smsMessage.MessageText);

			var response = await smsBlissClient.SendMessagesAsync(new[] { message }, showBillingDetails: true);

			NotifyBalanceChangeOnSendSuccessfully(response);
			return ConvertToISmsSendResult(response);
		}

		#endregion ISmsSender implementation

		#region ISmsBalanceNotifier implementation

		public event EventHandler<SmsBalanceEventArgs> OnBalanceChange;

		#endregion ISmsBalanceNotifier implementation

		protected virtual bool ValidateMobilePhoneNumberLength(string phone)
		{
			int phoneLegth = phone.Length;

			if(phoneLegth != 12) {
				return false;
			}

			return true;
		}

		private ISmsSendResult ConvertToISmsSendResult(MessagesResponse response)
		{
			if(response == null || !response.Messages.Any()) {
				return new SmsBlissSentResult(SmsSentStatus.UnknownError) { Description = "От сервера получен пустой ответ" };
			}
			MessageResponse messageResponse = response.Messages.First();
			return new SmsBlissSentResult(messageResponse);
		}

		private void NotifyBalanceChangeOnSendSuccessfully(MessagesResponse response)
		{
			if(response == null || !response.Messages.Any()) {
				return;
			}
			MessageResponse messageResponse = response.Messages.First();
			if(messageResponse.Status == MessageResponseStatus.Accepted) {
				foreach(var item in response.Balances) {

					if(!decimal.TryParse(item.BalanceValue, out decimal balance)) {
						logger.Warn($"Невозможно преобразовать значение баланса из \"{item.BalanceValue}\" в число");
						continue;
					}
					OnBalanceChange?.Invoke(this, new SmsBalanceEventArgs(ConvertBalanceType(item.Type), balance));
				}
			}
		}
	}
}
