using EmailPrepareWorker.Prepares;
using Mailjet.Api.Abstractions;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Parameters;
using EmailAttachment = Mailjet.Api.Abstractions.EmailAttachment;

namespace EmailPrepareWorker.SendEmailMessageBuilders
{
	public class UpdSendEmailMessageBuilder : SendEmailMessageBuilder
	{
		private readonly IEmailParametersProvider _emailParametersProvider;
		private readonly IEmailDocumentPreparer _emailDocumentPreparer;
		private readonly CounterpartyEmail _counterpartyEmail;

		public UpdSendEmailMessageBuilder(IEmailParametersProvider emailParametersProvider,
			IEmailDocumentPreparer emailDocumentPreparer, CounterpartyEmail counterpartyEmail, int instanceId) 
			: base(emailParametersProvider, emailDocumentPreparer, counterpartyEmail, instanceId)
		{
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));
			_emailDocumentPreparer = emailDocumentPreparer;
			_counterpartyEmail = counterpartyEmail ?? throw new ArgumentNullException(nameof(counterpartyEmail));
		}

		public override SendEmailMessageBuilder AddFromContact()
		{
			SendingMessage.From = new EmailContact
			{
				Name = _emailParametersProvider.DocumentEmailSenderName,
				Email = _emailParametersProvider.EmailSenderAddressForUpd
			};

			return this;
		}

		public override SendEmailMessageBuilder AddAttachment()
		{
			var document = _counterpartyEmail.EmailableDocument;

			var attachments = new List<EmailAttachment>
			{
				_emailDocumentPreparer.PrepareDocument(document, _counterpartyEmail.Type)
			};

			SendingMessage.Attachments = attachments;

			return this;
		}
	}
}
