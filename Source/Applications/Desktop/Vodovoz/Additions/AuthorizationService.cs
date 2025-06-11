using Mailjet.Api.Abstractions;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Organizations;
using Vodovoz.Settings.User;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Infrastructure.Services;
using VodovozInfrastructure.Configuration;

namespace Vodovoz.Additions
{
	public class AuthorizationService : IAuthorizationService
	{
		private readonly ILogger<AuthorizationService> _logger;
		private readonly IPasswordGenerator _passwordGenerator;
		private readonly IUserRoleSettings _userRoleSettings;
		private readonly IUserRoleRepository _userRoleRepository;
		private readonly IUserRepository _userRepository;
		private readonly IEmailSettings _emailSettings;
		private readonly int _humanResourcesSubdivisionId;
		private readonly int _developersSubdivisionId;

		private const int _passwordLength = 8;

		public AuthorizationService(IPasswordGenerator passwordGenerator,
			IUserRoleSettings userRoleSettings,
			IUserRoleRepository userRoleRepository,
			IUserRepository userRepository,
			IEmailSettings emailSettings,
			ISubdivisionSettings subdivisionSettings,
			ILogger<AuthorizationService> logger)
		{
			_passwordGenerator = passwordGenerator ?? throw new ArgumentNullException(nameof(passwordGenerator));
			_userRoleSettings = userRoleSettings ?? throw new ArgumentNullException(nameof(userRoleSettings));
			_userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));

			if(subdivisionSettings is null)
			{
				throw new ArgumentNullException(nameof(subdivisionSettings));
			}
			_humanResourcesSubdivisionId = subdivisionSettings.GetHumanResourcesSubdivisionId;
			_developersSubdivisionId = subdivisionSettings.GetDevelopersSubdivisionId;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public bool ResetPassword(string userLogin, string password, string email, string fullName)
		{
			using (var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
			{
				_userRepository.ChangePasswordForUser(uow, userLogin, password);
				var user = _userRepository.GetUserByLogin(uow, userLogin);
				if(user != null)
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
			try
			{
				_userRepository.CreateUser(uow, user.Login, password);
			}
			catch
			{
				_logger.LogError("Не удалось создать пользователя");
				throw;
			}

			try
			{
				_logger.LogInformation("Выдаем права пользователю");
				
				var database = _userRoleSettings.GetDatabaseForNewUser;
				_userRepository.GrantPrivilegesToNewUser(uow, database, user.Login);
				_userRepository.GiveSelectPrivilegesToArchiveDataBase(uow, user.Login);
				var userRole = _userRoleSettings.GetDefaultUserRoleName;
				
				_logger.LogInformation("Выдаем роль пользователю");
				if(employee.Subdivision != null
					&& (employee.Subdivision.Id == _humanResourcesSubdivisionId || employee.Subdivision.Id == _developersSubdivisionId))
				{
					_userRoleRepository.GrantRoleToUser(uow, userRole, user.Login, true);
				}
				else
				{
					_userRoleRepository.GrantRoleToUser(uow, userRole, user.Login);
				}
				_logger.LogInformation("Назначаем ее по умолчанию для него");
				_userRoleRepository.SetDefaultRoleToUser(uow, userRole, user.Login);
				_logger.LogInformation("Сохраняем пользователя");
				uow.Save(user);
			}
			catch
			{
				RemoveUserData(uow, user);
				_logger.LogInformation("Ошибка при выдаче прав пользователю или его сохранении");
				throw;
			}
			
			_logger.LogInformation("Идёт отправка почты");
			bool sendResult = false;
			try
			{
				sendResult = SendCredentialsToEmail(user.Login, password, employee.Email, employee.FullName, uow);
			}
			catch(TimeoutException)
			{
				RemoveUserData(uow, user);
				_logger.LogInformation(emailSendErrorMessage);
				MessageDialogHelper.RunErrorDialog("Сервис отправки E-Mail временно недоступен\n");
				return false;
			}
			catch
			{
				RemoveUserData(uow, user);
				_logger.LogInformation(emailSendErrorMessage);
				throw;
			}
			if(!sendResult)
			{
				//Если не получилось отправить e-mail с паролем - удаляем пользователя
				RemoveUserData(uow, user);
				_logger.LogInformation(emailSendErrorMessage);
				return false;
			}
			_logger.LogInformation("Письмо успешно отправлено");
			employee.User = user;
			
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
					Name = _emailSettings.DocumentEmailSenderName,
					Email = _emailSettings.DocumentEmailSenderAddress
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
				var connection = connectionFactory.CreateConnection(configuration.MessageBrokerHost, configuration.MessageBrokerUsername, configuration.MessageBrokerPassword, configuration.MessageBrokerVirtualHost, configuration.Port, true);
				var channel = connection.CreateModel();

				var properties = channel.CreateBasicProperties();
				properties.Persistent = true;

				channel.BasicPublish(configuration.EmailSendExchange, configuration.EmailSendKey, false, properties, sendingBody);

				return true;
			}
			catch(Exception e)
			{
				_logger.LogError(e, e.Message);
				return false;
			}
		}
		
		private void RemoveUserData(IUnitOfWork uow, User user)
		{
			_userRepository.DropUser(uow, user.Login);
			uow.Delete(user);
			uow.Session.Flush();
		}
	}
}
