using Mailjet.Api.Abstractions;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Attachments.Domain;
using QS.DomainModel.UoW;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Web;
using Vodovoz.Domain.Employees;
using Vodovoz.Parameters;
using Vodovoz.Services;
using VodovozInfrastructure.Configuration;

namespace Vodovoz.Infrastructure.Services
{
	public class EmployeeService : IEmployeeService
	{
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
			IEmailParametersProvider emailParametersProvider,
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
					Name = emailParametersProvider.DocumentEmailSenderName,
					Email = emailParametersProvider.DocumentEmailSenderAddress
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
				configuration.MessageBrokerVirtualHost);

			var channel = connection.CreateModel();

			var properties = channel.CreateBasicProperties();
			properties.Persistent = true;

			channel.BasicPublish(configuration.EmailSendExchange, configuration.EmailSendKey, false, properties, sendingBody);
		}
	}
}
