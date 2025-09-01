using Mailganer.Api.Client.Dto;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mailganer.Api.Client
{
	public class EmailDirectSender
	{
		private readonly MailganerClientV2 _mailganerClient;

		public EmailDirectSender(MailganerClientV2 mailganerClient)
		{
			_mailganerClient = mailganerClient ?? throw new ArgumentNullException(nameof(mailganerClient));
		}

		public async Task SendAsync(SendEmailMessage message)
		{
			if(message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			var emails = CreateEmailMessages(message);

			foreach(var email in emails)
			{
				await _mailganerClient.Send(email);
			}
		}

		private IEnumerable<EmailMessage> CreateEmailMessages(SendEmailMessage message)
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
