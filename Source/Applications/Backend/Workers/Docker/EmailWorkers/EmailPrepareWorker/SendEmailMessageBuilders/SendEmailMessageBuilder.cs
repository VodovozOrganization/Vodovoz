using EmailPrepareWorker.Prepares;
using Mailjet.Api.Abstractions;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Parameters;

namespace EmailPrepareWorker.SendEmailMessageBuilders
{
	public class SendEmailMessageBuilder
	{
		private readonly IEmailParametersProvider _emailParametersProvider;
		private readonly IEmailDocumentPreparer _emailDocumentPreparer;
		private readonly CounterpartyEmail _counterpartyEmail;
		private readonly int _instanceId;

		protected SendEmailMessage _sendEmailMessage = new();

		public SendEmailMessageBuilder(IEmailParametersProvider emailParametersProvider, IEmailDocumentPreparer emailDocumentPreparer,
			CounterpartyEmail counterpartyEmail, int instanceId)
		{
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));
			_emailDocumentPreparer = emailDocumentPreparer;
			_counterpartyEmail = counterpartyEmail ?? throw new ArgumentNullException(nameof(counterpartyEmail));
			_instanceId = instanceId;
		}

		public SendEmailMessage Build() => _sendEmailMessage;


		public static implicit operator SendEmailMessage(SendEmailMessageBuilder builder) => builder.Build();

		public virtual SendEmailMessageBuilder AddFromContact()
		{
			_sendEmailMessage.From = new EmailContact
			{
				Name = _emailParametersProvider.DocumentEmailSenderName,
				Email = _emailParametersProvider.DocumentEmailSenderAddress
			};

			return this;
		}

		public virtual SendEmailMessageBuilder AddToContact()
		{
			_sendEmailMessage.To = new List<EmailContact>
			{
				new()
				{
					Name = _counterpartyEmail.Counterparty.FullName,
					Email = _counterpartyEmail.StoredEmail.RecipientAddress
				}
			};

			return this;
		}

		public virtual SendEmailMessageBuilder AddTemplate()
		{
			var document = _counterpartyEmail.EmailableDocument;
			var template = document.GetEmailTemplate();

			_sendEmailMessage.Subject = $"{template.Title} {document.Title}";
			_sendEmailMessage.TextPart = template.Text;
			_sendEmailMessage.HTMLPart = template.TextHtml;

			return this;
		}

		public virtual SendEmailMessageBuilder AddAttachment()
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

			_sendEmailMessage.InlinedAttachments = inlinedAttachments;

			var attachments = new List<Mailjet.Api.Abstractions.EmailAttachment>
			{
				_emailDocumentPreparer.PrepareDocument(document, _counterpartyEmail.Type)
			};

			_sendEmailMessage.Attachments = attachments;

			return this;
		}

		public virtual SendEmailMessageBuilder AddPayload()
		{
			_sendEmailMessage.Payload = new EmailPayload
			{
				Id = _counterpartyEmail.StoredEmail.Id,
				Trackable = true,
				InstanceId = _instanceId
			};

			return this;
		}
	}
}
