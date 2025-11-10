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

namespace EmailSendWorker
{
	public class EmailSendWorker : BackgroundService
	{
		private const int _retryCount = 5;
		private const int _retryDelay = 5 * 1000; // sec => milisec

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
			if(message is null)
			{
				_logger.LogWarning("Received null message to send.");
				return;
			}

			if(message.To is null)
			{
				_logger.LogWarning("Message has no recipients. MessagePayloadId: {MessagePayloadId}", message.Payload?.Id);
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

		private async Task SendEmailMessage(int messagePayloadId, EmailMessage email)
		{
			if(email is null)
			{
				_logger.LogWarning("Received null email to send. MessagePayloadId: {MessagePayloadId}", messagePayloadId);
				return;
			}

			if(email.To is null)
			{
				_logger.LogWarning("Email has no recipient. MessagePayloadId: {MessagePayloadId}", messagePayloadId);
				return;
			}

			if(email.From is null)
			{
				_logger.LogWarning("Email has no sender. MessagePayloadId: {MessagePayloadId}", messagePayloadId);
				return;
			}

			for(var i = 0; i < _retryCount; i++)
			{
				_logger.LogInformation(
					"Sending email. Email address: {Email}. MessagePayloadId: {MessagePayloadId} ({RetryNumber}/{RetriesCount})",
					email.To,
					messagePayloadId,
					i + 1,
					_retryCount);

				try
				{
					await _mailganerClient.Send(email);

					_logger.LogInformation(
						"Email sent successfully. Email address: {Email}. MessagePayloadId: {MessagePayloadId}",
						email.To,
						messagePayloadId);

					PublishStoredEmailStatusUpdateMessage(messagePayloadId, MailEventType.sent, string.Empty);
					break;
				}
				catch(EmailInStopListException ex)
				{
					_logger.LogWarning(
						"Email is in stop list. Email address: {Email}. MessagePayloadId: {MessagePayloadId}. Bounce message: {BounceMessage}",
						email.To,
						messagePayloadId,
						ex.BounceMessage);

					PublishStoredEmailStatusUpdateMessage(
						messagePayloadId,
						MailEventType.bounce,
						$"Email is in stop list. Bounce message: {ex.BounceMessage}");
					break;
				}
				catch(Exception ex)
				{
					_logger.LogError(
						"Failed to send email. Email address: {Email}. MessagePayloadId: {MessagePayloadId}. Exception: {ExceptionMessage}",
						email.To,
						messagePayloadId,
						ex.Message);

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
		}

		private void PublishStoredEmailStatusUpdateMessage(
			int messagePayloadId,
			MailEventType mailEventType,
			string errorInfo)
		{
			var statusUpdateMessage = new UpdateStoredEmailStatusMessage
			{
				ErrorInfo = errorInfo,
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
