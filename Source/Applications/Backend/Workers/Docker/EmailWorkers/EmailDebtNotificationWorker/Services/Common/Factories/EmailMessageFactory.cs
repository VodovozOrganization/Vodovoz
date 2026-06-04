using EmailDebtNotificationWorker.Repositories;
using EmailDebtNotificationWorker.Services.Common.Generators;
using Mailjet.Api.Abstractions;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.StoredEmails;

namespace EmailDebtNotificationWorker.Services.Common.Factories
{
	public class EmailMessageFactory : IEmailMessageFactory
	{
		private const int _storedEmailSubjectMaxLength = 200;

		private readonly IDatabaseRepository _databaseRepository;
		private readonly IEmailLinkGenerator _emailLinkGenerator;

		public EmailMessageFactory(
			IDatabaseRepository databaseRepository,
			IEmailLinkGenerator emailLinkGenerator
			)
		{
			_databaseRepository = databaseRepository ?? throw new ArgumentNullException(nameof(databaseRepository));
			_emailLinkGenerator = emailLinkGenerator ??  throw new ArgumentNullException(nameof(emailLinkGenerator));
		}

		public SendEmailMessage CreateSendEmailMessage(
			IUnitOfWork uow,
			StoredEmail storedEmail,
			Counterparty client,
			string organizationFullName,
			string organizationEmailForMailing,
			IEnumerable<EmailAttachment> attachments,
			string emailAddress,
			string emailSubject,
			string messageText
			)
		{
			var instanceId = _databaseRepository.GetCurrentDatabaseId(uow);

			var unsubscribeUrl = storedEmail.Guid.HasValue
				? _emailLinkGenerator.GetUnsubscribeLink(storedEmail.Guid.Value)
				: string.Empty;

			var sendEmailMessage = new SendEmailMessage()
			{
				From = new EmailContact
				{
					Name = organizationFullName,
					Email = organizationEmailForMailing,
				},
				To = new List<EmailContact>
				{
					new()
					{
						Name = client.FullName,
						Email = emailAddress
					}
				},
				Subject = emailSubject,
				TextPart = messageText,
				HTMLPart = messageText,
				Payload = new EmailPayload
				{
					Id = storedEmail.Id,
					Trackable = true,
					InstanceId = instanceId
				},
				Attachments = attachments.ToList(),
				Headers = new Dictionary<string, string>
				{
					{ "List-Unsubscribe", unsubscribeUrl }
				}
			};

			return sendEmailMessage;
		}

		public StoredEmail CreateStoredEmail(string subject, string email, string? description)
		{
			var storedEmailSubject = subject.Length > _storedEmailSubjectMaxLength
				? subject[.._storedEmailSubjectMaxLength]
				: subject;

			var storedEmail = new StoredEmail
			{
				State = StoredEmailStates.WaitingToSend,
				Author = null,
				ManualSending = false,
				SendDate = DateTime.Now,
				StateChangeDate = DateTime.Now,
				Subject = storedEmailSubject,
				RecipientAddress = email,
				Guid = Guid.NewGuid(),
				Description = description
			};

			return storedEmail;
		}
	}
}
