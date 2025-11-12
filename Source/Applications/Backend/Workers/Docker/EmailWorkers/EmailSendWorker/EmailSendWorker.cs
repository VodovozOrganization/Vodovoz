using CustomerAppsApi.Library.Configs;
using Mailganer.Api.Client;
using Mailganer.Api.Client.Dto;
using Mailganer.Api.Client.Exceptions;
using Mailjet.Api.Abstractions.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace EmailSendWorker
{
	public class EmailSendWorker : BackgroundService
	{
		private const int _retryCount = 5;
		private const int _retryDelay = 5 * 1000; // sec => milisec
		private const int _errorInfoMaxLength = 1000;

		private readonly ILogger<EmailSendWorker> _logger;
		private readonly MailganerClientV2 _mailganerClient;
		private readonly RabbitOptions _rabbitOptions;

		private readonly IModel _consumerChannel;
		private readonly IModel _publisherChannel;
		private readonly object _publisherLock = new object();

		private readonly AsyncEventingBasicConsumer _consumer;

		public EmailSendWorker(
			ILogger<EmailSendWorker> logger,
			IConnection connection,
			IOptions<RabbitOptions> rabbitOptions,
			MailganerClientV2 mailganerClient
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mailganerClient = mailganerClient ?? throw new ArgumentNullException(nameof(mailganerClient));
			_rabbitOptions = (rabbitOptions ?? throw new ArgumentNullException(nameof(rabbitOptions))).Value;

			_consumerChannel = connection.CreateModel();
			_publisherChannel = connection.CreateModel();

			_consumerChannel.QueueDeclare(_rabbitOptions.EmailSendQueue, true, false, false, null);
			_publisherChannel.QueueDeclare(_rabbitOptions.StatusUpdateQueue, true, false, false, null);

			_consumer = new AsyncEventingBasicConsumer(_consumerChannel);
			_consumer.Received += MessageReceived;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_consumerChannel.BasicConsume(_rabbitOptions.EmailSendQueue, false, _consumer);
			await Task.Delay(0, stoppingToken);
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting Email Send Worker...");
			return base.StartAsync(cancellationToken);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Stopping Email Send Worker...");

			_consumer.Received -= MessageReceived;

			_consumerChannel?.Close();
			_consumerChannel?.Dispose();

			_publisherChannel?.Close();
			_publisherChannel?.Dispose();

			await base.StopAsync(cancellationToken);
		}

		private async Task MessageReceived(object sender, BasicDeliverEventArgs e)
		{
			try
			{
				var body = e.Body;
				var message = JsonSerializer.Deserialize<SendEmailMessage>(body.Span);
				await SendEmails(message);
			}
			finally
			{
				_logger.LogInformation("Free message from queue");
				_consumerChannel.BasicAck(e.DeliveryTag, false);
			}
		}

		private async Task SendEmails(SendEmailMessage message)
		{
			if(!IsSendEmailMessageValid(message))
			{
				return;
			}

			var recipients = message.To.Select(recipient => recipient?.Email).ToArray();

			_logger.LogInformation(
				"Recieved message to send to recipients: {Recipients} with subject: \"{Subject}\", with {AttachmentsCount} attachments",
				string.Join(", ", recipients),
				message.Subject,
				message.Attachments?.Count ?? 0);


			if(message.EventPayload == null)
			{
				message.Payload = new EmailPayload();
			}

			var emails = CreateEmailMessages(message);

			foreach(var email in emails)
			{
				await SendEmailMessage(message.Payload.Id, email);
			}
		}

		private async Task SendEmailMessage(int messagePayloadId, EmailMessage emailMessage)
		{
			if(!IsEmailMessageValid(emailMessage, messagePayloadId))
			{
				return;
			}

			for(var i = 0; i < _retryCount; i++)
			{
				_logger.LogInformation(
					"Sending email. Email address: {Email}. MessagePayloadId: {MessagePayloadId} ({RetryNumber}/{RetriesCount})",
					emailMessage.To,
					messagePayloadId,
					i + 1,
					_retryCount);

				var sendEmailResult = await TrySendEmail(emailMessage);

				if(sendEmailResult.IsSuccess)
				{
					PublishStoredEmailStatusUpdateMessage(messagePayloadId, MailEventType.sent, string.Empty);
					break;
				}

				if(sendEmailResult.IsFailure && sendEmailResult.Errors.Any(e => e.Code == "500"))
				{
					var removeEmailFromStopListResult = await TryRemoveEmailFromStopList(emailMessage);

					if(removeEmailFromStopListResult.IsFailure)
					{
						PublishStoredEmailStatusUpdateMessage(
							messagePayloadId,
							MailEventType.bounce,
							"Email is in stop list and could not be removed.");
						break;
					}

					var reSendEmailResult = await TrySendEmail(emailMessage);

					if(reSendEmailResult.IsSuccess)
					{
						PublishStoredEmailStatusUpdateMessage(messagePayloadId, MailEventType.sent, string.Empty);
						break;
					}
				}

				await Task.Delay(_retryDelay);

				if(i >= _retryCount - 1)
				{
					PublishStoredEmailStatusUpdateMessage(
						messagePayloadId,
						MailEventType.bounce,
						$"SendWorker unable to send message to MailGaner. MessagePayloadId: {messagePayloadId}");
				}
			}
		}

		private async Task<Result> TrySendEmail(EmailMessage email)
		{
			_logger.LogInformation("Trying to send email to: {Email}", email.To);
			try
			{
				await _mailganerClient.Send(email);
			}
			catch(EmailInStopListException ex)
			{
				_logger.LogError(ex, "Email is in stop list: {Email}", email.To);
				return Result.Failure(new Error("500", "Email is in stop list"));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Failed to send email: {Email}", email.To);
				return Result.Failure(new Error(string.Empty, $"Failed to send email: {ex.Message}"));
			}
			_logger.LogInformation("Email sent successfully to: {Email}", email.To);
			return Result.Success();
		}

		private async Task<Result> TryRemoveEmailFromStopList(EmailMessage email)
		{
			try
			{
				_logger.LogInformation(
					"Trying to remove email from stop list. Email address: {Email}",
					email.To);

				var bounceInfo = await _mailganerClient.GetEmailBounseMessageInStopList(email.To);

				if(string.IsNullOrEmpty(bounceInfo))
				{
					_logger.LogWarning(
						"Bounce info is empty. Cannot determine if email should be removed from stop list. Email address: {Email}",
						email.To);
					return Result.Failure(new Error(string.Empty, "Bounce info is empty"));
				}

				if(!bounceInfo.ToLower().Contains("spam"))
				{
					_logger.LogWarning(
						"Bounce info does not indicate spam. Email will not be removed from stop list. Email address: {Email}. Bounce info: {BounceInfo}",
						email.To,
						bounceInfo);
					return Result.Failure(new Error(string.Empty, "Bounce info does not indicate spam"));
				}

				await _mailganerClient.RemoveEmailFromStopList(email.From, email.To);

				_logger.LogInformation(
					"Email removed from stop list successfully. Email address: {Email}",
					email.To);

				return Result.Success();
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Failed to remove email from stop list. Email address: {Email}",
					email.To);

				return Result.Failure(new Error(string.Empty, $"Failed to remove email from stop list: {ex.Message}"));
			}
		}

		private void PublishStoredEmailStatusUpdateMessage(
			int messagePayloadId,
			MailEventType mailEventType,
			string errorInfo)
		{
			var statusUpdateMessage = new UpdateStoredEmailStatusMessage
			{
				ErrorInfo = errorInfo.Length > _errorInfoMaxLength ? errorInfo[.._errorInfoMaxLength] : errorInfo,
				EventPayload = new EmailPayload { Id = messagePayloadId, Trackable = true },
				Status = mailEventType,
				RecievedAt = DateTime.Now
			};

			PublishStoredEmailStatusUpdateMessage(statusUpdateMessage);
		}

		private void PublishStoredEmailStatusUpdateMessage(UpdateStoredEmailStatusMessage message)
		{
			try
			{
				_logger.LogInformation(
					"Publishing email status update message. MessagePayloadId: {MessagePayloadId}, Status: {Status}",
					message.EventPayload.Id,
					message.Status);

				var serializedMessage = JsonSerializer.Serialize(message);
				var statusUpdateBody = Encoding.UTF8.GetBytes(serializedMessage);

				lock(_publisherLock)
				{
					var properties = _publisherChannel.CreateBasicProperties();
					properties.Persistent = true;
					_publisherChannel.BasicPublish("", _rabbitOptions.StatusUpdateQueue, false, properties, statusUpdateBody);
				}

				_logger.LogInformation(
					"Email status update message published successfully. MessagePayloadId: {MessagePayloadId}, Status: {Status}",
					message.EventPayload.Id,
					message.Status);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Failed to publish email status update message. MessagePayloadId: {MessagePayloadId}, Status: {Status}",
					message.EventPayload.Id,
					message.Status);
			}
		}

		private bool IsSendEmailMessageValid(SendEmailMessage message)
		{
			if(message is null)
			{
				_logger.LogWarning("Received null message to send.");
				return false;
			}

			if(message.To is null)
			{
				_logger.LogWarning("Message has no recipients. MessagePayloadId: {MessagePayloadId}", message.Payload?.Id);
				return false;
			}

			return true;
		}

		public bool IsEmailMessageValid(EmailMessage emailMessage, int messagePayloadId)
		{
			if(emailMessage is null)
			{
				_logger.LogWarning("Received null email to send. MessagePayloadId: {MessagePayloadId}", messagePayloadId);
				return false;
			}

			if(emailMessage.To is null)
			{
				_logger.LogWarning("Email has no recipient. MessagePayloadId: {MessagePayloadId}", messagePayloadId);
				return false;
			}

			if(emailMessage.From is null)
			{
				_logger.LogWarning("Email has no sender. MessagePayloadId: {MessagePayloadId}", messagePayloadId);
				return false;
			}

			return true;
		}

		private static IEnumerable<EmailMessage> CreateEmailMessages(SendEmailMessage message)
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
