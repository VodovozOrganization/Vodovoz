using Mailganer.Api.Client.Dto;
using RabbitMQ.MailSending;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Results;

namespace EmailSendWorker.Factoies
{
	public class EmailMessageFactory : IEmailMessageFactory
	{
		///<inheritdoc/>
		public Result<IEnumerable<EmailMessage>> CreateEmailMessages(SendEmailMessage message)
		{
			if(message is null)
			{
				return Result.Failure<IEnumerable<EmailMessage>>(new Error(string.Empty, "Received null message to send email"));
			}

			if(message.To is null)
			{
				return Result.Failure<IEnumerable<EmailMessage>>(new Error(string.Empty, "Message has no recipients"));
			}

			var emailMessages = new List<EmailMessage>();

			foreach(var to in message.To)
			{
				if(string.IsNullOrWhiteSpace(to.Email))
				{
					return Result.Failure<IEnumerable<EmailMessage>>(new Error(string.Empty, "Email has no recipient"));
				}

				if(message.From is null)
				{
					return Result.Failure<IEnumerable<EmailMessage>>(new Error(string.Empty, "Email has no sender"));
				}

				var email = new EmailMessage
				{
					From = $"{message.From.Name} <{message.From.Email}>",
					FromAddress = message.From.Email,
					To = to.Email,
					Subject = message.Subject,
					MessageText = message.HTMLPart,
					TrackOpen = true,
					TrackClick = true,
					TrackId = $"{message.Payload.InstanceId}-{message.Payload.Id}",
				};

				if(message.Attachments != null && message.Attachments.Any())
				{
					email.Attachments = message.Attachments.Select(x => new EmailAttachment
					{
						Filename = x.Filename,
						Base64Content = x.Base64Content,
					}).ToList();
				}

				emailMessages.Add(email);
			}
			return emailMessages;
		}
	}
}
