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
using TransactionalOutbox.MassTransit;

namespace Vodovoz.Core.Application.Employees
{
	public class EmployeeService : IEmployeeService
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IUserService _userService;
		// На десктопе у диалогов свои UoW, поэтому IPublishEndpoint из DI запрещён: сообщение пишется
		// в таблицы outbox в транзакции нужного UoW (_outbox.GetPublishEndpoint(uow)), отправляет его бэкенд
		private readonly IUnitOfWorkOutbox<IBus> _outbox;

		public EmployeeService(IUnitOfWorkFactory uowFactory, IUserService userService, IUnitOfWorkOutbox<IBus> outbox)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
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

		/// <summary>
		/// Письмо сохраняется в outbox в отдельной транзакции, чтобы не коммитить чужой UoW
		/// </summary>
		private async Task SendMessageToEmail(SendEmailMessage message)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Отправка письма сотруднику"))
			{
				await _outbox.GetPublishEndpoint(uow).Publish(message);
				await uow.CommitAsync();
			}
		}
	}
}
