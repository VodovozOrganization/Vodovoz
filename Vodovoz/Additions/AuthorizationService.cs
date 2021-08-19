using System;
using EmailService;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using Vodovoz.Domain.Employees;
using Vodovoz.Parameters;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Infrastructure.Services;

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

        private const int passwordLength = 8;

        public bool ResetPassword(string userLogin, string password, string email)
        {
            if (emailService == null)
            {
                return false;
            }
            mySQLUserRepository.ChangePassword(userLogin, password);

            using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
            {
                var user = uow.Session.QueryOver<User>().Where(u => u.Login == userLogin).SingleOrDefault();
                if (user != null)
                {
                    user.NeedPasswordChange = true;
                    uow.Save(user);
                    uow.Commit();
                }
            }

            return SendCredentialsToEmail(userLogin, password, email);
        }

        public bool ResetPasswordToGenerated(string userLogin, string email) 
	        => ResetPassword(userLogin, passwordGenerator.GeneratePassword(passwordLength), email);

        public bool TryToSaveUser(Employee employee, IUnitOfWork uow)
        {
            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                MessageDialogHelper.RunQuestionDialog("Нельзя сбросить пароль.\n У сотрудника не заполнено поле Email");
                return false;
            }

            const string emailSendErrorMessage = "Ошибка при отправке E-Mail";

            var user = new User {
            	Login = employee.LoginForNewUser,
            	Name = employee.FullName,
            	NeedPasswordChange = true
            };

            bool cont = MessageDialogHelper.RunQuestionDialog($"При сохранении работника будет создан \nпользователь с логином {user.Login} \nи на " +
            	$"указанный E-Mail {employee.Email}\nбудет отправлено письмо с временным паролем\n\t\t\tПродолжить?");
            if(!cont)
            	return false;

            var password = passwordGenerator.GeneratePassword(passwordLength);

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

            	logger.Info("Идёт отправка почты");
            	bool sendResult = false;
            	try {
            		sendResult = SendCredentialsToEmail(user.Login, password, employee.Email);
            	} catch(TimeoutException) {
            		RemoveUserData(uow, user);
            		logger.Info(emailSendErrorMessage);
            		MessageDialogHelper.RunErrorDialog("Сервис отправки E-Mail временно недоступен\n");
            		return false;
            	} catch {
            		RemoveUserData(uow, user);
            		logger.Info(emailSendErrorMessage);
            		throw;
            	}
            	if(!sendResult) {
            		//Если не получилось отправить e-mail с паролем - удаляем пользователя
            		RemoveUserData(uow, user);
            		logger.Info(emailSendErrorMessage);
            		return false;
            	}
            	logger.Info("Письмо успешно отправлено");
                employee.User = user;
            } else {
            	MessageDialogHelper.RunErrorDialog("Не получилось создать нового пользователя");
            	return false;
            }
            return true;
        }

        private bool SendCredentialsToEmail(string login, string password, string mailAddress)
        {
            string messageText = $"Логин: {login}\nПароль: {password}";

            var email = new Email()
            {
                Title = "Учетные данные для входа в программу Доставка Воды",
                Text = messageText,
                HtmlText = messageText,
                Recipient = new EmailContact("", mailAddress),
                Sender = new EmailContact("vodovoz-spb.ru", new ParametersProvider().GetParameterValue("email_for_email_delivery")),
            };

            return emailService.SendEmail(email);
        }
        
        private void RemoveUserData(IUnitOfWork uow, User user)
        {
	        uow.Delete(user);
	        uow.Session.Flush();
	        mySQLUserRepository.DropUser(user.Login);
        }
    }
}
