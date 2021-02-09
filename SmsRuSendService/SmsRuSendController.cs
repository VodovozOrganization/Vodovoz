using SmsRu;
using SmsRu.Enumerations;
using SmsSendInterface;
using System;
using System.Threading.Tasks;

namespace SmsRuSendService
{
    public class SmsRuSendController : ISmsSender, ISmsBalanceNotifier
    {
        private readonly SmsRuProvider smsRuProvider;
        private readonly ISmsRuConfiguration configuration;

        public SmsRuSendController(ISmsRuConfiguration configuration)
        {
            this.configuration = configuration;
            smsRuProvider = new SmsRuProvider(configuration);
        }

        public BalanceResponse GetBalanceResponse
        {
            get
            {
                var balanceResponse = smsRuProvider.CheckBalance(EnumAuthenticationTypes.StrongApi);

                BalanceResponse balance = new BalanceResponse()
                {
                    BalanceType = BalanceType.CurrencyBalance,
                    BalanceValue = decimal.Parse(balanceResponse)
                };

                return balance;
            }
        }

        public event EventHandler<SmsBalanceEventArgs> OnBalanceChange;

        public ISmsSendResult SendSms(ISmsMessage message)
        {
            try
            {
                var response = smsRuProvider.Send(configuration.SmsNumberFrom, message.MobilePhoneNumber, message.MessageText, message.ScheduleTime);

                if (!string.IsNullOrEmpty(response))
                {
                    var lines = response.Split('\n');

                    var enumStatus = Enum.Parse(typeof(ResponseOnSendRequest), lines[0]);
                    switch (enumStatus)
                    {
                        case ResponseOnSendRequest.MessageAccepted:
                            return new SmsSendResult(SmsSentStatus.Accepted);
                        case ResponseOnSendRequest.BadRecipient:
                        case ResponseOnSendRequest.BlacklistedRecepient:
                        case ResponseOnSendRequest.CantSendToThisNumber:
                        case ResponseOnSendRequest.DayMessageLimitToNumber:
                            return new SmsSendResult(SmsSentStatus.InvalidMobilePhone);
                        case ResponseOnSendRequest.MessageTextNotSpecified:
                            return new SmsSendResult(SmsSentStatus.TextIsEmpty);
                        case ResponseOnSendRequest.BadSender:
                            return new SmsSendResult(SmsSentStatus.SenderAddressInvalid);
                        case ResponseOnSendRequest.OutOfMoney:
                            return new SmsSendResult(SmsSentStatus.NotEnoughBalance);
                        default:
                            return new SmsSendResult(SmsSentStatus.UnknownError);
                    }
                }
                else
                {
                    throw new Exception("Не получен ответ от сервера");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task<ISmsSendResult> SendSmsAsync(ISmsMessage message)
        {
            throw new NotSupportedException(); // Нет использований в нашем проекте TODO: дописать при рефакторинге библиотеки
        }
    }
}
