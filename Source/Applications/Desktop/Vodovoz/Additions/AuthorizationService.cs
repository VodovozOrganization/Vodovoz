using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using NLog;
using NLog.Extensions.Logging;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using RabbitMQ.MailSending;
using RabbitMQ.Infrastructure;
using Vodovoz.Domain.Employees;
using Vodovoz.Parameters;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using VodovozInfrastructure.Configuration;
using System.Collections.Generic;
using Mailjet.Api.Abstractions;
using Vodovoz.EntityRepositories;

namespace Vodovoz.Additions
{
	public class AuthorizationService : IAuthorizationService
	{
		private readonly IPasswordGenerator _passwordGenerator;
		private readonly MySQLUserRepository _mySQLUserRepository;
		private readonly IUserRepository _userRepository;
		private readonly IEmailParametersProvider _emailParametersProvider;
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private const int _passwordLength = 8;

		public AuthorizationService(IPasswordGenerator passwordGenerator,
			MySQLUserRepository mySQLUserRepository,
			IUserRepository userRepository,
			IEmailParametersProvider emailParametersProvider)
		{
			_passwordGenerator = passwordGenerator ?? throw new ArgumentNullException(nameof(passwordGenerator));
			_mySQLUserRepository =
				mySQLUserRepository ?? throw new ArgumentNullException(nameof(mySQLUserRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));
		}

		public bool ResetPassword(string userLogin, string password, string email, string fullName)
		{
			_mySQLUserRepository.ChangePassword(userLogin, password);

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var user = uow.Session.QueryOver<User>().Where(u => u.Login == userLogin).SingleOrDefault();
				if (user != null)
				{
					user.NeedPasswordChange = true;
					uow.Save(user);
					uow.Commit();
				}

				return SendCredentialsToEmail(userLogin, password, email, fullName, uow);
			}
		}

		public bool ResetPasswordToGenerated(string userLogin, string email, string fullName) 
			=> ResetPassword(userLogin, _passwordGenerator.GeneratePassword(_passwordLength), email, fullName);

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
			{
				return false;
			}

			var password = _passwordGenerator.GeneratePassword(_passwordLength);

			//Сразу пишет в базу
			var result = _mySQLUserRepository.CreateLogin(user.Login, password);
			if(result)
			{
				try {
					_mySQLUserRepository.UpdatePrivileges(user.Login, false);
					_userRepository.GiveSelectPrivelegesToArchiveDataBase(uow, user.Login);
				} catch {
					_mySQLUserRepository.DropUser(user.Login);
					throw;
				}
				uow.Save(user);

				_logger.Info("Идёт отправка почты");
				bool sendResult = false;
				try {
					sendResult = SendCredentialsToEmail(user.Login, password, employee.Email, employee.FullName, uow);
				} catch(TimeoutException) {
					RemoveUserData(uow, user);
					_logger.Info(emailSendErrorMessage);
					MessageDialogHelper.RunErrorDialog("Сервис отправки E-Mail временно недоступен\n");
					return false;
				} catch {
					RemoveUserData(uow, user);
					_logger.Info(emailSendErrorMessage);
					throw;
				}
				if(!sendResult) {
					//Если не получилось отправить e-mail с паролем - удаляем пользователя
					RemoveUserData(uow, user);
					_logger.Info(emailSendErrorMessage);
					return false;
				}
				_logger.Info("Письмо успешно отправлено");
				employee.User = user;
			} else {
				MessageDialogHelper.RunErrorDialog("Не получилось создать нового пользователя");
				return false;
			}
			return true;
		}

		private bool SendCredentialsToEmail(string login, string password, string mailAddress, string fullName, IUnitOfWork unitOfWork)
		{
			var instanceId = Convert.ToInt32(unitOfWork.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			var configuration = unitOfWork.GetAll<InstanceMailingConfiguration>().FirstOrDefault();

			string messageText =
				$"Логин: { login }\n" +
				$"Пароль: { password }";

			var sendEmailMessage = new SendEmailMessage()
			{
				From = new EmailContact
				{
					Name = _emailParametersProvider.DocumentEmailSenderName,
					Email = _emailParametersProvider.DocumentEmailSenderAddress
				},

				To = new List<EmailContact>
				{
					new EmailContact
					{
						Name = fullName,
						Email = mailAddress
					}
				},

				Subject = "Учетные данные для входа в программу Доставка Воды",

				TextPart = messageText,
				HTMLPart = messageText,
				Payload = new EmailPayload
				{
					Id = 0,
					Trackable = false,
					InstanceId = instanceId
				}
			};

			try
			{
				var serializedMessage = JsonSerializer.Serialize(sendEmailMessage);
				var sendingBody = Encoding.UTF8.GetBytes(serializedMessage);

				var Logger = new Logger<RabbitMQConnectionFactory>(new NLogLoggerFactory());

				var connectionFactory = new RabbitMQConnectionFactory(Logger);
				var connection = connectionFactory.CreateConnection(configuration.MessageBrokerHost, configuration.MessageBrokerUsername, configuration.MessageBrokerPassword, configuration.MessageBrokerVirtualHost);
				var channel = connection.CreateModel();

				var properties = channel.CreateBasicProperties();
				properties.Persistent = true;

				channel.BasicPublish(configuration.EmailSendExchange, configuration.EmailSendKey, false, properties, sendingBody);

				return true;
			}
			catch(Exception e)
			{
				_logger.Error(e, e.Message);
				return false;
			}
		}
		
		private void RemoveUserData(IUnitOfWork uow, User user)
		{
			uow.Delete(user);
			uow.Session.Flush();
			_mySQLUserRepository.DropUser(user.Login);
		}
	}
}
