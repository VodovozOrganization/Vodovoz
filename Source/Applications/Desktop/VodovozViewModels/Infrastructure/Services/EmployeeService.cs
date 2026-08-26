using Mailjet.Api.Abstractions;
using QS.Attachments.Domain;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;
using Vodovoz.Settings.Common;
using Vodovoz.Core.Domain.Users;
using MassTransit;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Infrastructure.Services
{
	public class EmployeeService : IEmployeeService
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IBus _messageBus;

		public EmployeeService(
			IUnitOfWorkFactory uowFactory,
			IEmployeeRepository employeeRepository,
			IBus messageBus)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
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
				return _employeeRepository.GetEmployeeForCurrentUser(uow);
			}
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

			SendMessageToEmail(message).GetAwaiter().GetResult();
		}

		public IEmployeeInnerPhone GetEmployeeInnerPhone()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return _employeeRepository.GetEmployeeInnerPhone(uow);
			}
		}

		private async Task SendMessageToEmail(SendEmailMessage message)
		{
			await _messageBus.Publish(message);
		}
	}
}
