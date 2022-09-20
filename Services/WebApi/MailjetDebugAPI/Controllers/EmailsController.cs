using Mailjet.Api.Abstractions;
using Mailjet.Api.Abstractions.Events;
using MailjetDebugAPI.Endpoints;
using MailjetDebugAPI.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailjetDebugAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EmailsController : ControllerBase
	{
		private readonly ILogger<EmailsController> _logger;
		private readonly EventsRecieverEndpoint _eventsRecieverEndpoint;

		public EmailsController(ILogger<EmailsController> logger, EventsRecieverEndpoint eventsRecieverEndpoint)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_eventsRecieverEndpoint = eventsRecieverEndpoint ?? throw new ArgumentNullException(nameof(eventsRecieverEndpoint));
		}

		[HttpPost]
		[Route("/api/send")]
		public async Task<SendResponse> Send([FromBody] SendPayload sendPayload)
		{
			_logger.LogInformation($"Recieved sendPayload SandboxMode = { sendPayload.SandboxMode }, with { sendPayload.Messages.Count } messages");

			SendResponse result = new SendResponse { Messages = new List<MessageSendDetails>() };

			string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

			var hubUrl = baseUrl.TrimEnd('/') + EmailsHub.HubUrl;

			var hubConnection = new HubConnectionBuilder()
				.WithUrl(hubUrl)
				.Build();

			await hubConnection.StartAsync();

			foreach(var email in sendPayload.Messages)
			{
				_logger.LogInformation($"Recieved email: Subject: \"{ email.Subject }\", " +
					$"From { email.From.Email } ({ email.From.Name }), " +
					$"To: { string.Join(", ", email.To.Select(recipient => $"{ recipient.Email } ({ recipient.Name })")) }, " +
					$"Text: { email.TextPart }, " +
					$"HTML: { email.HTMLPart }, " +
					$"With { email.InlinedAttachments?.Count ?? 0 } inlined and { email.Attachments?.Count ?? 0 } not inlined attachments, " +
					$"CustomId: { email.CustomId }, " +
					$"Payload: { email.EventPayload }");

				await hubConnection.SendAsync("EmailRecieved", email);

				var messageDetail = new MessageSendDetails
				{
					Status = "success",
					To = new List<SendDetails>()
				};

				foreach(var recipient in email.To)
				{
					var emailGuid = Guid.NewGuid().ToString();
					var messageId = new Random().Next(int.MaxValue);

					var eventTime = DateTimeOffset.Now.ToUnixTimeSeconds();

					var sendDetails = new SendDetails
					{
						Email = recipient.Email,
						MessageID = messageId,
						MessageUUID = emailGuid,
						MessageHref = "https://some.url.must.be.here.at.production.that.contains/MessageUUID"
					};

					await _eventsRecieverEndpoint.SendEvent<MailSentEvent>(new MailSentEvent
					{
						Time = eventTime,
						EmailAddress = recipient.Email,
						MessageGuid = emailGuid,
						MessageId = messageId,
						CustomId = email.CustomId ?? "",
						Payload = email.EventPayload,
						MailjetMessageId = messageId.ToString(),
						CustomCampaign = "",
						SmtpReply = "sent (250 2.0.0 OK 4242424242 fa5si855896wjc.199 - gsmtp)"
					});

					messageDetail.To.Add(sendDetails);
				}

				result.Messages.Add(messageDetail);
			}

			return result;
		}
	}
}
