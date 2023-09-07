using EmailPrepareWorker.Prepares;
using Mailjet.Api.Abstractions;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Parameters;

namespace EmailPrepareWorker.SendEmailMessageBuilders
{
	public class SendEmailMessageBuilder
	{
		private protected SendEmailMessage SendingMessage { get; } = new();
		public SendEmailMessage ResultSendEmailMessage => SendingMessage;
		private readonly IEmailParametersProvider _emailParametersProvider;
		private readonly IEmailDocumentPreparer _emailDocumentPreparer;
		private readonly CounterpartyEmail _counterpartyEmail;
		private readonly int _instanceId;

		public SendEmailMessageBuilder(IEmailParametersProvider emailParametersProvider, IEmailDocumentPreparer emailDocumentPreparer,
			CounterpartyEmail counterpartyEmail, int instanceId)
		{
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));
			_emailDocumentPreparer = emailDocumentPreparer;
			_counterpartyEmail = counterpartyEmail ?? throw new ArgumentNullException(nameof(counterpartyEmail));
			_instanceId = instanceId;
		}

		public virtual void BuildFromContact()
		{
			SendingMessage.From = new EmailContact
			{
				Name = _emailParametersProvider.DocumentEmailSenderName,
				Email = _emailParametersProvider.DocumentEmailSenderAddress
			};
		}

		public virtual void BuildToContact()
		{
			SendingMessage.To = new List<EmailContact>
			{
				new()
				{
					Name = _counterpartyEmail.Counterparty.FullName,
					Email = _counterpartyEmail.StoredEmail.RecipientAddress
				}
			};
		}

		public virtual void BuildTemplate()
		{
			var document = _counterpartyEmail.EmailableDocument;
			var template = document.GetEmailTemplate();

			SendingMessage.Subject = $"{template.Title} {document.Title}";
			SendingMessage.TextPart = template.Text;
			SendingMessage.HTMLPart = template.TextHtml;
		}

		public virtual async Task BuildAttachment()
		{
			var inlinedAttachments = new List<InlinedEmailAttachment>();

			var document = _counterpartyEmail.EmailableDocument;

			var template = document.GetEmailTemplate();

			foreach(var item in template.Attachments)
			{
				inlinedAttachments.Add(new InlinedEmailAttachment
				{
					ContentID = item.Key,
					ContentType = item.Value.MIMEType,
					Filename = item.Value.FileName,
					Base64Content = item.Value.Base64Content
				});
			}

			SendingMessage.InlinedAttachments = inlinedAttachments;

			var attachments = new List<Mailjet.Api.Abstractions.EmailAttachment>
			{
				await _emailDocumentPreparer.PrepareDocument(document, _counterpartyEmail.Type)
			};

			SendingMessage.Attachments = attachments;
		}

		public virtual void BuildPayload()
		{
			SendingMessage.Payload = new EmailPayload
			{
				Id = _counterpartyEmail.StoredEmail.Id,
				Trackable = true,
				InstanceId = _instanceId
			};
		}
	}
}
