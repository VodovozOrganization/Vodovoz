using Mailjet.Api.Abstractions;
using MassTransit;
using MimeMapping;
using QS.Attachments.Domain;
using QS.DomainModel.UoW;
using QS.Services;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;
using Vodovoz.Settings.Common;

namespace Vodovoz.Core.Application.Employees
{
	public class EmployeeService : IEmployeeService
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IUserService _userService;
		private readonly IBus _messageBus;

		public EmployeeService(IUnitOfWorkFactory uowFactory, IUserService userService, IBus messageBus)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
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
						ContentType = MimeUtility.GetMimeMapping(attachment.FileName),
						Filename = attachment.FileName,
						Base64Content = Convert.ToBase64String(attachment.ByteFile)
					}
				}
			};

			SendMessageToEmail(message).GetAwaiter().GetResult();
		}

		private async Task SendMessageToEmail(SendEmailMessage message)
		{
			await _messageBus.Publish(message);
		}
	}
}
