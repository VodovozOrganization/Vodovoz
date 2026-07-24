using Email.Infrastructure.Factories;
using Email.Infrastructure.Repositories;
using EmailDebtNotificationWorker.Services.Common.Generators;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.StoredEmails;

namespace EmailDebtNotificationWorker.Services.ReminderToAcceptUpd
{
	public class ReminderToAcceptUpdEmailPreparer : IReminderToAcceptUpdEmailPreparer
	{
		private readonly IDatabaseRepository _databaseRepository;
		private readonly IEmailMessageFactory _emailMessageFactory;
		private readonly IEmailBodyGenerator _emailBodyGenerator;

		public ReminderToAcceptUpdEmailPreparer(
			IDatabaseRepository databaseRepository,
			IEmailMessageFactory emailMessageFactory,
			IEmailBodyGenerator emailBodyGenerator)
		{
			_databaseRepository = databaseRepository ?? throw new ArgumentNullException(nameof(databaseRepository));
			_emailMessageFactory = emailMessageFactory ?? throw new ArgumentNullException(nameof(emailMessageFactory));
			_emailBodyGenerator = emailBodyGenerator ?? throw new ArgumentNullException(nameof(emailBodyGenerator));
		}

		public async Task<IReadOnlyList<SendEmailMessage>> PrepareReminderToAcceptUpdEmails(
			IUnitOfWork uow,
			IReadOnlyCollection<TimedOutDocFlowGrouppedNode> timeOutDocFlowGrouppedNodes,
			CancellationToken cancellationToken)
		{
			var sendEmailMessages = new List<SendEmailMessage>();

			var instanceId = _databaseRepository.GetCurrentDatabaseId(uow);

			foreach(var node in timeOutDocFlowGrouppedNodes)
			{
				var clientEmails = node.Client.Emails.Where(x => x.EmailType?.EmailPurpose == EmailPurpose.ForBills).Select(x => x.Address).ToList();

				foreach(var emailToSent in clientEmails)
				{
					var emailSubject = $"Запрос на принятие и подтверждение УПД №({string.Join(", ", node.Documents.Select(d => d.UpdNum))})";

					var storedEmail = _emailMessageFactory.CreateStoredEmail(emailSubject, emailToSent, null);

					await uow.SaveAsync(storedEmail, cancellationToken: cancellationToken);

					var reminderToAcceptUpdEmail = new ReminderToAcceptUpdEmail
					{
						StoredEmail = storedEmail,
						Counterparty = node.Client,
						OrganizationId = node.Organization.Id
					};

					await uow.SaveAsync(reminderToAcceptUpdEmail, cancellationToken: cancellationToken);

					var messageText = _emailBodyGenerator.GenerateReminderToAcceptUpdEmailBody(node);

					var emailMessage = _emailMessageFactory.CreateSendEmailMessage(
						uow,
						storedEmail,
						node.Client.FullName,
						node.Organization.FullName,
						node.Organization.EmailForMailing,
						null,
						emailToSent,
						emailSubject,
						messageText,
						false);

					sendEmailMessages.Add(emailMessage);
				}

				foreach(var document in node.Documents)
				{
					var taxcomDocflow = document.TaxcomDocflow;
					taxcomDocflow.IsReminderToAcceptUpdEmailSent = true;
					await uow.SaveAsync(taxcomDocflow, cancellationToken: cancellationToken);
				}
			}

			return sendEmailMessages;
		}
	}
}
