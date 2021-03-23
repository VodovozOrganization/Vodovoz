using System;
using EmailService;
using InstantSmsService;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Employees;
using Vodovoz.Parameters;
using Vodovoz.Tools;

namespace Vodovoz.Additions
{
    public class AuthorizationService : IAuthorizationService
    {
        public AuthorizationService(IPasswordGenerator passwordGenerator,
            MySQLUserRepository mySQLUserRepository,
            IEmailService emailService)
        {
            
            this.passwordGenerator = passwordGenerator ?? throw new ArgumentNullException(nameof(passwordGenerator));
            this.mySQLUserRepository =
                mySQLUserRepository ?? throw new ArgumentNullException(nameof(mySQLUserRepository));
            this.emailService = emailService;
        }

        private readonly IPasswordGenerator passwordGenerator;
        private readonly MySQLUserRepository mySQLUserRepository;
        private readonly IEmailService emailService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const int passwordLength = 5;

        public bool ResetPassword(Employee employee, string password)
        {
            if (emailService == null)
            {
                return false;
            }

            #region Смена пароля в БД

            string login = employee.User.Login;
            mySQLUserRepository.ChangePassword(login, password);

            #endregion

            #region Отправка почты

            string messageText = $"Логин: {login}\nПароль: {password}";

            var email = new Email()
            {
                Title = "Учетные данные для входа в программу Доставка Воды",
                Text = messageText,
                HtmlText = messageText,
                Recipient = new EmailContact("", employee.Email),
                Sender = new EmailContact("vodovoz-spb.ru", ParametersProvider.Instance.GetParameterValue("email_for_email_delivery")),
            };

            return emailService.SendEmail(email);

            #endregion
        }

        public bool ResetPasswordToGenerated(Employee employee) 
	        => ResetPassword(employee, passwordGenerator.GeneratePassword(passwordLength));

        public bool TryToSaveUser(Employee employee, IUnitOfWork uow)
        {
	        var user = new User {
            	Login = employee.LoginForNewUser,
            	Name = employee.FullName,
            	NeedPasswordChange = true
            };
            bool cont = MessageDialogHelper.RunQuestionDialog($"При сохранении работника будет создан \nпользователь с логином {user.Login} \nи на " +
            	$"указанный номер +7{employee.GetPhoneForSmsNotification()}\nбудет выслана SMS с временным паролем\n\t\t\tПродолжить?");
            if(!cont)
            	return false;

            var password = new Tools.PasswordGenerator().GeneratePassword(5);
            //Сразу пишет в базу
            var result = mySQLUserRepository.CreateLogin(user.Login, password);
            if(result) {
            	try {
            		mySQLUserRepository.UpdatePrivileges(user.Login, false);
            	} catch {
            		mySQLUserRepository.DropUser(user.Login);
            		throw;
            	}
                uow.Save(user);

            	logger.Info("Идёт отправка sms (до 10 секунд)...");
            	bool sendResult = false;
            	try {
            		sendResult = SendPasswordByPhone(employee, password);
            	} catch(TimeoutException) {
            		RemoveUserData(uow, user);
            		logger.Info("Ошибка при отправке sms");
            		MessageDialogHelper.RunErrorDialog("Сервис отправки Sms временно недоступен\n");
            		return false;
            	} catch {
            		RemoveUserData(uow, user);
            		logger.Info("Ошибка при отправке sms");
            		throw;
            	}
            	if(!sendResult) {
            		//Если не получилось отправить смс с паролем - удаляем пользователя
            		RemoveUserData(uow, user);
            		logger.Info("Ошибка при отправке sms");
            		return false;
            	}
            	logger.Info("Sms успешно отправлено");
                employee.User = user;
            } else {
            	MessageDialogHelper.RunErrorDialog("Не получилось создать нового пользователя");
            	return false;
            }
            
            return true;
        }
        
        private bool SendPasswordByPhone(Employee employee, string password)
        {
	        SmsSender sender = new SmsSender(new BaseParametersProvider(), InstantSmsServiceSetting.GetInstantSmsService());

	        #region ФормированиеТелефона
			
	        string stringPhoneNumber = employee.GetPhoneForSmsNotification();
	        if (stringPhoneNumber == null){
		        MessageDialogHelper.RunErrorDialog("Не найден подходящий телефон для отправки Sms", "Ошибка при отправке Sms");
		        return false;
	        }
	        string phoneNumber = $"+7{stringPhoneNumber}";

	        #endregion
			
	        var result = sender.SendPassword(phoneNumber, employee.LoginForNewUser, password);
			
	        if(result.MessageStatus == SmsMessageStatus.Ok) {
		        MessageDialogHelper.RunInfoDialog("Sms с паролем отправлена успешно");
		        return true;
	        } else {
		        MessageDialogHelper.RunErrorDialog(result.ErrorDescription, "Ошибка при отправке Sms");
		        return false;
	        }
        }
        
        private void RemoveUserData(IUnitOfWork uow, User user)
        {
	        uow.Delete(user);
	        uow.Session.Flush();
	        mySQLUserRepository.DropUser(user.Login);
        }
        
    }
}