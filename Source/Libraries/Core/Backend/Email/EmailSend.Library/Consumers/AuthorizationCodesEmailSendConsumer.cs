using Mailganer.Api.Client;
using Mailganer.Api.Client.Dto;
using MassTransit;
using Microsoft.Extensions.Logging;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSend.Library.Consumers
{
	public class AuthorizationCodesEmailSendConsumer : IConsumer<AuthorizationCodesSendEmailMessage>
	{
		private readonly ILogger<AuthorizationCodesEmailSendConsumer> _logger;
		private readonly MailganerClientV2 _mailganerClient;

		public AuthorizationCodesEmailSendConsumer(
			ILogger<AuthorizationCodesEmailSendConsumer> logger,
			MailganerClientV2 mailganerClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mailganerClient = mailganerClient ?? throw new ArgumentNullException(nameof(mailganerClient));
		}

		public async Task Consume(ConsumeContext<AuthorizationCodesSendEmailMessage> context)
		{
			var message = context.Message;

			var recipients = new StringBuilder();
			if(message.To != null)
			{
				recipients.Append(string.Join(", ", message.To.Select(recipient => recipient?.Email)));
			}

			_logger.LogInformation(
				"Recieved message to send to recipients: {Recipients} with subject: \"{Subject}\", with {Attachmetscount} attachments",
				recipients.ToString(),
				message.Subject,
				message.Attachments?.Count ?? 0);

			if(message.EventPayload == null)
			{
				message.Payload = new EmailPayload();
			}

			var emails = CreateEmailMessages(message);
			_logger.LogInformation("Sending email {MessagePayloadId}", message.Payload.Id);

			try
			{
				foreach(var email in emails)
				{
					await _mailganerClient.Send(email);
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, e.Message);
				throw;
			}
		}

		private IEnumerable<EmailMessage> CreateEmailMessages(AuthorizationCodesSendEmailMessage message)
		{
			var emailMessages = new List<EmailMessage>();
			foreach(var to in message.To)
			{
				var email = new EmailMessage
				{
					From = $"{message.From.Name} <{message.From.Email}>",
					To = to.Email,
					Subject = message.Subject,
					MessageText = message.HTMLPart,
					Attachments = message.Attachments.Select(x => new EmailAttachment
					{
						Filename = x.Filename,
						Base64Content = x.Base64Content,
					}).ToList(),
					TrackOpen = true,
					TrackClick = true,
					TrackId = $"{message.Payload.InstanceId}-{message.Payload.Id}",
				};
				emailMessages.Add(email);
			}
			return emailMessages;
		}
	}
}
