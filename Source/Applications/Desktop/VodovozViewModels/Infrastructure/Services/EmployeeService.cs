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

		public bool SendCounterpartyClassificationCalculationReportToEmail(
			IUnitOfWork unitOfWork,
			IEmailParametersProvider emailParametersProvider,
			string employeeName,
			string emailAddress,
			byte[] attachmentData)
		{
			var instanceId = Convert.ToInt32(unitOfWork.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			var configuration = unitOfWork.GetAll<InstanceMailingConfiguration>().FirstOrDefault();

			string messageText = "Отчет об изменении категории клиентов";

			var attachment = new Attachment
			{
				FileName = $"Отчет об изменении категории клиентов от {DateTime.Now:dd.MM.yyyy}",
				ByteFile = attachmentData
			};

			var sendEmailMessage = new SendEmailMessage()
			{
				From = new EmailContact
				{
					Name = emailParametersProvider.DocumentEmailSenderName,
					Email = emailParametersProvider.DocumentEmailSenderAddress
				},

				To = new List<EmailContact>
				{
					new EmailContact
					{
						Name = employeeName,
						Email = emailAddress
					}
				},

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

			var serializedMessage = JsonSerializer.Serialize(sendEmailMessage);
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

			return true;
		}
	}
}
