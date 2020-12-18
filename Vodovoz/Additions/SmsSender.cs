using System;
using InstantSmsService;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Additions
{
    public class SmsSender
    {
        private readonly ISmsNotifierParametersProvider smsNotifierParametersProvider;
        private readonly IInstantSmsService service;
        public SmsSender(ISmsNotifierParametersProvider smsNotifierParametersProvider, IInstantSmsService service)
        {
            this.smsNotifierParametersProvider = smsNotifierParametersProvider ??
                                                 throw new ArgumentNullException(nameof(smsNotifierParametersProvider));
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }
        
        public ResultMessage SendPassword( string phone, string login, string password)
        {
            #region Формирование
            
            if(!smsNotifierParametersProvider.IsSmsNotificationsEnabled) {
                return new ResultMessage { ErrorDescription = "Sms уведомления выключены" };
            }
            if(String.IsNullOrWhiteSpace(password)) {
                return new ResultMessage { ErrorDescription = "Был передан неверный пароль" };
            }

            string messageText = $"Логин: {login}\nПароль: {password}";
            
            if(service == null) {
                return new ResultMessage { ErrorDescription = "Сервис отправки Sms не работает, обратитесь в РПО." };
            }
            
            var smsNotification = new InstantSmsMessage {
                MessageText = messageText,
                MobilePhone = phone,
                ExpiredTime = DateTime.Now.AddMinutes(10)
            };

            #endregion
            
            return service.SendSms(smsNotification);
        }
        
        
    }
}