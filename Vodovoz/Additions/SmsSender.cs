using System;
using InstantSmsService;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Additions
{
    public class SmsSender
    {
        public ResultMessage SendPasswordToEmployee(ISmsNotifierParametersProvider smsNotifierParametersProvider, Employee employee, string password)
        {
            if(!smsNotifierParametersProvider.IsSmsNotificationsEnabled) {
                return new ResultMessage { ErrorDescription = "Sms уведомления выключены" };
            }
            if(String.IsNullOrWhiteSpace(password)) {
                return new ResultMessage { ErrorDescription = "Был передан неверный пароль" };
            }

            //формирование номера мобильного телефона
            string stringPhoneNumber = employee.GetPhoneForSmsNotification();
            if(stringPhoneNumber == null)
                return new ResultMessage { ErrorDescription = "Не найден подходящий телефон для отправки Sms" };

            string phoneNumber = $"+7{stringPhoneNumber}";

            string messageText = $"Логин: {employee.LoginForNewUser}\nПароль: {password}";

            var smsNotification = new InstantSmsMessage {
                MessageText = messageText,
                MobilePhone = phoneNumber,
                ExpiredTime = DateTime.Now.AddMinutes(10)
            };

            IInstantSmsService service = InstantSmsServiceSetting.GetInstantSmsService();
            if(service == null) {
                return new ResultMessage { ErrorDescription = "Сервис отправки Sms не работает" };
            }
            return service.SendSms(smsNotification);
        }
    }
}