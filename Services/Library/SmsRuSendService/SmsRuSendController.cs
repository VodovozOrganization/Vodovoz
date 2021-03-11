using SmsRu;
using SmsRu.Enumerations;
using SmsSendInterface;
using System;
using System.Threading.Tasks;
using System.Linq;
using NLog;
using System.Globalization;

namespace SmsRuSendService
{
    public class SmsRuSendController : ISmsSender, ISmsBalanceNotifier
    {
        private readonly SmsRuProvider smsRuProvider;
        private readonly ISmsRuConfiguration configuration;
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();
        private const string balanceStringPrefix = "balance=";

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

                var lines = balanceResponse.Split('\n');

                var culture = CultureInfo.CreateSpecificCulture("ru-RU");
                culture.NumberFormat.NumberDecimalSeparator = ".";

                BalanceResponse balance = new BalanceResponse()
                {
                    BalanceType = BalanceType.CurrencyBalance,
                    BalanceValue = decimal.Parse(lines[1], NumberStyles.AllowDecimalPoint, culture.NumberFormat)
                };

                return balance;
            }
        }

        public event EventHandler<SmsBalanceEventArgs> OnBalanceChange;

        public ISmsSendResult SendSms(ISmsMessage message)
        {

            var response = smsRuProvider.Send(configuration.SmsNumberFrom, message.MobilePhoneNumber, message.MessageText, message.ScheduleTime);

            if (!string.IsNullOrEmpty(response))
            {
                var lines = response.Split('\n');

                var enumStatus = Enum.Parse(typeof(ResponseOnSendRequest), lines[0]);

                SmsSendResult smsSendResponse;

                switch (enumStatus)
                {
                    case ResponseOnSendRequest.MessageAccepted:
                        smsSendResponse = new SmsSendResult(SmsSentStatus.Accepted);

                        var balanceLine = lines.FirstOrDefault(x => x.StartsWith(balanceStringPrefix));

                        var culture = CultureInfo.CreateSpecificCulture("ru-RU");
                        culture.NumberFormat.NumberDecimalSeparator = ".";

                        if (balanceLine != null && decimal.TryParse(balanceLine.Substring(balanceStringPrefix.Length), NumberStyles.AllowDecimalPoint, culture.NumberFormat, out decimal newBalance))
                        {
                            OnBalanceChange?.Invoke(this, new SmsBalanceEventArgs(BalanceType.CurrencyBalance, newBalance));
                        }
                        else
                        {
                            logger.Warn("Не удалось получить баланс в ответном сообщении");
                        }

                        break;
                    case ResponseOnSendRequest.BadRecipient:
                    case ResponseOnSendRequest.BlacklistedRecepient:
                    case ResponseOnSendRequest.CantSendToThisNumber:
                    case ResponseOnSendRequest.DayMessageLimitToNumber:
                        smsSendResponse = new SmsSendResult(SmsSentStatus.InvalidMobilePhone);
                        break;
                    case ResponseOnSendRequest.MessageTextNotSpecified:
                        smsSendResponse = new SmsSendResult(SmsSentStatus.TextIsEmpty);
                        break;
                    case ResponseOnSendRequest.BadSender:
                        smsSendResponse = new SmsSendResult(SmsSentStatus.SenderAddressInvalid);
                        break;
                    case ResponseOnSendRequest.OutOfMoney:
                        smsSendResponse = new SmsSendResult(SmsSentStatus.NotEnoughBalance);
                        break;
                    default:
                        smsSendResponse = new SmsSendResult(SmsSentStatus.UnknownError);
                        break;
                }

                return smsSendResponse;
            }
            else
            {
                throw new Exception("Не получен ответ от сервера");
            }
        }

        public Task<ISmsSendResult> SendSmsAsync(ISmsMessage message)
        {
            throw new NotSupportedException(); // Нет использований в нашем проекте TODO: дописать при рефакторинге библиотеки
        }
    }
}
