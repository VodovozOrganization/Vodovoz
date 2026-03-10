using EmailPrepareWorker.Prepares;
using Mailjet.Api.Abstractions;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Controllers;
using EmailAttachment = Mailjet.Api.Abstractions.EmailAttachment;

namespace EmailPrepareWorker.SendEmailMessageBuilders
{
	public class UpdSendEmailMessageBuilder : SendEmailMessageBuilder
	{
		private readonly IEmailDocumentPreparer _emailDocumentPreparer;
		private readonly CounterpartyEmail _counterpartyEmail;

		public UpdSendEmailMessageBuilder(
			IEmailSettings emailSettings,
			IEmailRepository emailRepository,
			IUnitOfWork unitOfWork,
			IEmailDocumentPreparer emailDocumentPreparer,
			ICounterpartyEdoAccountController edoAccountController,
			CounterpartyEmail counterpartyEmail,
			IOrganizationSettings  organizationSettings,
			int instanceId) 
			: base(unitOfWork, emailSettings, emailRepository, emailDocumentPreparer, edoAccountController, counterpartyEmail, organizationSettings, instanceId)
		{
			_emailDocumentPreparer = emailDocumentPreparer ?? throw new ArgumentNullException(nameof(emailDocumentPreparer));
			_counterpartyEmail = counterpartyEmail ?? throw new ArgumentNullException(nameof(counterpartyEmail));
		}

		public override SendEmailMessageBuilder AddFromContact()
		{
			_sendEmailMessage.From = new EmailContact
			{
				Name = _emailSettings.DocumentEmailSenderName,
				Email = _emailSettings.EmailSenderAddressForUpd
			};

			return this;
		}

		public override SendEmailMessageBuilder AddAttachment(string connectionString)
		{
			var document = _counterpartyEmail.EmailableDocument;

			var attachments = new List<EmailAttachment>
			{
				_emailDocumentPreparer.PrepareDocument(document, _counterpartyEmail.Type, connectionString)
			};

			_sendEmailMessage.Attachments = attachments;

			return this;
		}
	}
}
