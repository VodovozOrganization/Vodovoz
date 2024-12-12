﻿using EmailPrepareWorker.Prepares;
using Mailjet.Api.Abstractions;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Common;
using EmailAttachment = Mailjet.Api.Abstractions.EmailAttachment;

namespace EmailPrepareWorker.SendEmailMessageBuilders
{
	public class SendEmailMessageBuilder
	{
		private readonly IUnitOfWork _unitOfWork;
		protected readonly IEmailSettings _emailSettings;
		private readonly IEmailRepository _emailRepository;
		private readonly IEmailDocumentPreparer _emailDocumentPreparer;
		private readonly CounterpartyEmail _counterpartyEmail;
		private EmailTemplate _template;
		private readonly int _instanceId;

		protected SendEmailMessage _sendEmailMessage = new();

		public SendEmailMessageBuilder(
			IUnitOfWork unitOfWork,
			IEmailSettings emailSettings,
			IEmailRepository emailRepository,
			IEmailDocumentPreparer emailDocumentPreparer,
			CounterpartyEmail counterpartyEmail,
			int instanceId)
		{
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_emailSettings = emailSettings
				?? throw new ArgumentNullException(nameof(emailSettings));
			_emailRepository = emailRepository
				?? throw new ArgumentNullException(nameof(emailRepository));
			_emailDocumentPreparer = emailDocumentPreparer
				?? throw new ArgumentNullException(nameof(emailDocumentPreparer));
			_counterpartyEmail = counterpartyEmail
				?? throw new ArgumentNullException(nameof(counterpartyEmail));
			_instanceId = instanceId;
		}

		public SendEmailMessage Build() => _sendEmailMessage;


		public static implicit operator SendEmailMessage(SendEmailMessageBuilder builder) => builder.Build();

		public virtual SendEmailMessageBuilder AddFromContact()
		{
			_sendEmailMessage.From = new EmailContact
			{
				Name = _emailSettings.DocumentEmailSenderName,
				Email = _emailSettings.DocumentEmailSenderAddress
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

			var hasSendedEmailsForBill = false;
			
			if(document.Type == OrderDocumentType.Bill
			   || document.Type == OrderDocumentType.SpecialBill)
			{
				hasSendedEmailsForBill = _emailRepository.HasSendedEmailsForBillExceptOf(document.Order.Id, _counterpartyEmail.StoredEmail.Id);
			}

			if(hasSendedEmailsForBill
			   && document.Type == OrderDocumentType.Bill
			   && document is BillDocument billDocument)
			{
				_template = billDocument.GetResendEmailTemplate();
			}
			else if(hasSendedEmailsForBill
				&& document.Type == OrderDocumentType.SpecialBill
				&& document is SpecialBillDocument specialBillDocument)
			{
				_template = specialBillDocument.GetResendEmailTemplate();
			}
			else if(_counterpartyEmail.StoredEmail.ManualSending == true
				&& document is ICustomResendTemplateEmailableDocument resendableDocument)
			{
				_template = resendableDocument.GetResendDocumentEmailTemplate();
			}
			else
			{
				_template = document.GetEmailTemplate();
			}

			_sendEmailMessage.Subject = $"{_template.Title} {document.Title}";
			_sendEmailMessage.TextPart = _template.Text;
			_sendEmailMessage.HTMLPart = _template.TextHtml;

			return this;
		}

		public virtual SendEmailMessageBuilder AddAttachment(string connectionString)
		{
			var inlinedAttachments = new List<InlinedEmailAttachment>();

			var document = _counterpartyEmail.EmailableDocument;

			foreach(var item in _template.Attachments)
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

			var attachments = new List<EmailAttachment>
			{
				_emailDocumentPreparer.PrepareDocument(document, _counterpartyEmail.Type, connectionString)
			};

			if((document.Order?.IsFirstOrder ?? false)
				&& _counterpartyEmail.Type == CounterpartyEmailType.BillDocument
				&& _emailDocumentPreparer
					.PrepareOfferAgreementDocument(_unitOfWork, document.Order.Contract, connectionString) is EmailAttachment additionalAgreement)
			{
				attachments.Add(additionalAgreement);
			}

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
