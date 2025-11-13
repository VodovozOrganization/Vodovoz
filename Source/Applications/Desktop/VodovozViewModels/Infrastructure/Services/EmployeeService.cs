using Mailjet.Api.Abstractions;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Attachments.Domain;
using QS.DomainModel.UoW;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Web;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;
using VodovozInfrastructure.Configuration;
using Vodovoz.Settings.Common;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Infrastructure.Services
{
	public class EmployeeService : IEmployeeService
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IUserService _userService;

		public EmployeeService(IUnitOfWorkFactory uowFactory, IUserService userService)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		public Employee GetEmployee(int employeeId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return GetEmployee(uow, employeeId);
			}
		}

		public Employee GetEmployee(IUnitOfWork uow, int employeeId)
		{
			return uow.GetById<Employee>(employeeId);
		}

		public Employee GetEmployeeForCurrentUser()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return GetEmployeeForCurrentUser(uow);
			}
		}

		public Employee GetEmployeeForCurrentUser(IUnitOfWork uow)
		{
			User userAlias = null;
			return uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.User, () => userAlias)
				.Where(() => userAlias.Id == _userService.CurrentUserId)
				.SingleOrDefault();
		}

		public Employee GetEmployeeForUser(IUnitOfWork uow, int userId)
		{
			User userAlias = null;
			return uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.User, () => userAlias)
				.Where(() => userAlias.Id == userId)
				.SingleOrDefault();
		}

		public void SendCounterpartyClassificationCalculationReportToEmail(
			IUnitOfWork uow,
			IEmailSettings emailSettings,
			string employeeName,
			IEnumerable<string> emailAddresses,
			byte[] attachmentData)
		{
			var instanceId = Convert.ToInt32(uow.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			string messageText = "Отчет об изменении категории клиентов";

			var attachment = new Attachment
			{
				FileName = $"Отчет об изменении категории клиентов от {DateTime.Now:dd.MM.yyyy}.xlsx",
				ByteFile = attachmentData
			};

			var emailContacts = emailAddresses
				.Select(e => new EmailContact { Name = employeeName, Email = e })
				.ToList();

			var message = new SendEmailMessage()
			{
				From = new EmailContact
				{
					Name = emailSettings.DocumentEmailSenderName,
					Email = emailSettings.DocumentEmailSenderAddress
				},

				To = emailContacts,

				Subject = $"Отчет об изменении категории клиентов от {DateTime.Now:dd.MM.yyyy}",

				TextPart = messageText,

				HTMLPart = messageText,

				Payload = new EmailPayload
				{
					Id = 0,
					Trackable = false,
					InstanceId = instanceId
				},

				Attachments = new List<EmailAttachment>
				{
					new EmailAttachment
					{
						ContentType = MimeMapping.GetMimeMapping(attachment.FileName),
						Filename = attachment.FileName,
						Base64Content = Convert.ToBase64String(attachment.ByteFile)
					}
				}
			};

			SendMessageToEmail(uow, message);
		}

		private void SendMessageToEmail(IUnitOfWork uow, SendEmailMessage message)
		{
			var configuration = uow.GetAll<InstanceMailingConfiguration>().FirstOrDefault();

			var serializedMessage = JsonSerializer.Serialize(message);
			var sendingBody = Encoding.UTF8.GetBytes(serializedMessage);

			var Logger = new Logger<RabbitMQConnectionFactory>(new NLogLoggerFactory());

			var connectionFactory = new RabbitMQConnectionFactory(Logger);

			var connection = connectionFactory.CreateConnection(
				configuration.MessageBrokerHost,
				configuration.MessageBrokerUsername,
				configuration.MessageBrokerPassword,
				configuration.MessageBrokerVirtualHost,
				configuration.Port,
				true);

			var channel = connection.CreateModel();

			var properties = channel.CreateBasicProperties();
			properties.Persistent = true;

			channel.BasicPublish(configuration.EmailSendExchange, configuration.EmailSendKey, false, properties, sendingBody);
		}
	}
}
