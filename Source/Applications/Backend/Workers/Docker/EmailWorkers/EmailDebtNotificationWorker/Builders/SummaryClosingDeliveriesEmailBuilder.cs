using EmailDebtNotificationWorker.Builders;
using EmailDebtNotificationWorker.DTO;
using Mailjet.Api.Abstractions;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Settings.Orders;

namespace EmailDebtNotificationWorker.Services
{
	public class SummaryClosingDeliveriesEmailBuilder : ISummaryClosingDeliveriesEmailBuilder
	{
		private readonly IClosingDeliveriesSettings _closingDeliveriesSettings;

		public SummaryClosingDeliveriesEmailBuilder(IClosingDeliveriesSettings closingDeliveriesSettings)
		{
			_closingDeliveriesSettings = closingDeliveriesSettings ?? throw new ArgumentNullException(nameof(closingDeliveriesSettings));
		}

		public async Task<IReadOnlyList<SendEmailMessage>> Build(
			IUnitOfWork uow,
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrEmpty(_closingDeliveriesSettings.ClosingDeliveriesNotificationEmailsTo))
			{
				throw new InvalidOperationException("Не настроены адреса для уведомлений о просрочке дебиторской задолженности клиентов");
			}

			var organization = notificationInfos.First(x => !string.IsNullOrWhiteSpace(x.OrderWithoutShipmentForDebt.Organization.ClosingDeliveriesNotificationEmailFrom)).OrderWithoutShipmentForDebt.Organization;

			if(organization is null)
			{
				throw new ArgumentException($"Не удалось подобрать орнанизацию с заполненным E-mail, с которого будет отправляться письмо о закрытии поставок");
			}

			var emailToSentContacts = _closingDeliveriesSettings.ClosingDeliveriesNotificationEmailsTo
					.Split(';')
					.Select(x => new EmailContact { Email = x.Trim(), Name = "Автоматика" })
					.ToList();

			var sendEmailMessages = new List<SendEmailMessage>(emailToSentContacts.Count);

			var body = BuildBody(notificationInfos);

			foreach(var emailToSentContact in emailToSentContacts)
			{
				var storedEmail = new StoredEmail
				{
					State = StoredEmailStates.WaitingToSend,
					SendDate = DateTime.Now,
					StateChangeDate = DateTime.Now,
					RecipientAddress = emailToSentContact.Email,
					Subject = "Просроченная задолженность клиентов",
					Guid = Guid.NewGuid(),
					Description = "Сводка задолженности"
				};

				await uow.SaveAsync(storedEmail, cancellationToken: cancellationToken);

				var sendEmailMessage = new SendEmailMessage
				{
					From = new EmailContact
					{
						Name = organization.FullName,
						Email = organization.ClosingDeliveriesNotificationEmailFrom,
					},
					To = new List<EmailContact>
					{
						emailToSentContact
					},

					Subject = storedEmail.Subject,
					TextPart = body,
					HTMLPart = body,

					Payload = new EmailPayload
					{
						Id = storedEmail.Id,
						Trackable = true
					}
				};

				sendEmailMessages.Add(sendEmailMessage);
			}

			return sendEmailMessages;
		}

		private string BuildBody(IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos)
		{
			var sb = new StringBuilder();

			sb.Append("<table border='1'>");
			sb.Append("<tr><th>№</th><th>Организация</th><th>ID</th><th>ИНН</th><th>Клиент</th><th>Дней просрочки</th><th>Сумма задолженности</th></tr>");

			var  index = 1;

			foreach(var info in notificationInfos)
			{
				var order = info.OrderWithoutShipmentForDebt;

				sb.Append($@"
					<tr>
						<td>{index}</td>
						<td>{order.Organization.Name}</td>
						<td>{order.Client.Id}</td>
						<td>{order.Client.INN}</td>
						<td>{order.Client.FullName}</td>
						<td>{info.OverdueDebtDays}</td>
						<td>{order.DebtSum}</td>
					</tr>");

				index++;
			}

			sb.Append("</table>");

			return sb.ToString();
		}
	}
}
