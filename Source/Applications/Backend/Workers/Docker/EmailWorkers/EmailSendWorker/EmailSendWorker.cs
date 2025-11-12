using CustomerAppsApi.Library.Configs;
using EmailSendWorker.Factoies;
using EmailSendWorker.Services;
using Mailganer.Api.Client.Dto;
using Mailjet.Api.Abstractions.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.MailSending;
using System;
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
		private readonly IEmailMessageFactory _emailMessageFactory;
		private readonly IEmailSendService _emailSendService;
		private readonly RabbitOptions _rabbitOptions;

		private readonly IModel _channel;
		private readonly object _publisherLock = new object();

		private readonly AsyncEventingBasicConsumer _consumer;

		public EmailSendWorker(
			ILogger<EmailSendWorker> logger,
			IModel channel,
			IOptions<RabbitOptions> rabbitOptions,
			IEmailMessageFactory emailMessageFactory,
			IEmailSendService emailSendService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_channel = channel ?? throw new ArgumentNullException(nameof(channel));
			_emailMessageFactory = emailMessageFactory ?? throw new ArgumentNullException(nameof(emailMessageFactory));
			_emailSendService = emailSendService ?? throw new ArgumentNullException(nameof(emailSendService));
			_rabbitOptions = (rabbitOptions ?? throw new ArgumentNullException(nameof(rabbitOptions))).Value;
			_channel.QueueDeclare(_rabbitOptions.EmailSendQueue, true, false, false, null);
			_consumer = new AsyncEventingBasicConsumer(_channel);
			_consumer.Received += MessageReceived;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_channel.BasicConsume(_rabbitOptions.EmailSendQueue, false, _consumer);
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
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing message from queue: {ErrorMessage}", ex.Message);
			}
			finally
			{
				_logger.LogInformation("Free message from queue");
				_channel.BasicAck(e.DeliveryTag, false);
			}
		}

		private async Task SendEmails(SendEmailMessage message)
		{
			var emailsCreateResult = _emailMessageFactory.CreateEmailMessages(message);

			if(emailsCreateResult.IsFailure)
			{
				_logger.LogError(
					"Failed to create email messages. Errors: {Errors}",
					emailsCreateResult.GetErrorsString());
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

			foreach(var email in emailsCreateResult.Value)
			{
				var sendMessageResult = await SendEmailMessage(message.Payload.Id, email);

				if(sendMessageResult.IsSuccess)
				{
					_logger.LogInformation(
						"Email sent successfully to {Email}. MessagePayloadId: {MessagePayloadId}",
						email.To,
						message.Payload.Id);

					PublishStoredEmailStatusUpdateMessage(
						message.Payload.Id,
						MailEventType.sent,
						string.Empty);
				}
				else
				{
					var errors = sendMessageResult.GetErrorsString();

					_logger.LogError(
						"Failed to send email to {Email}. MessagePayloadId: {MessagePayloadId}. Errors: {Errors}",
						email.To,
						message.Payload.Id,
						sendMessageResult.GetErrorsString());

					PublishStoredEmailStatusUpdateMessage(
						message.Payload.Id,
						MailEventType.bounce,
						errors);
				}
			}
		}

		private async Task<Result> SendEmailMessage(int messagePayloadId, EmailMessage emailMessage)
		{
			for(var i = 0; i < _retryCount; i++)
			{
				_logger.LogInformation(
					"Sending email. Email address: {Email}. MessagePayloadId: {MessagePayloadId} ({RetryNumber}/{RetriesCount})",
					emailMessage.To,
					messagePayloadId,
					i + 1,
					_retryCount);

				var sendEmailResult = await _emailSendService.SendEmail(emailMessage);

				if(sendEmailResult.IsSuccess)
				{
					return Result.Success();
				}

				if(sendEmailResult.IsFailure && sendEmailResult.Errors.Any(e => e.Code == _emailSendService.EmaiInStopListErrorCodeString))
				{
					var removeEmailFromStopListResult =
						await _emailSendService.CheckAndRemoveSpamEmailFromStopList(emailMessage.To, emailMessage.From);

					if(removeEmailFromStopListResult.IsFailure)
					{
						return removeEmailFromStopListResult;
					}

					var reSendEmailResult = await _emailSendService.SendEmail(emailMessage);

					if(reSendEmailResult.IsSuccess)
					{
						return Result.Success();
					}
				}

				await Task.Delay(_retryDelay);
			}

			return Result.Failure(new Error(string.Empty, $"SendWorker unable to send message to MailGaner. MessagePayloadId: {messagePayloadId}"));
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
					_channel.QueueDeclare(_rabbitOptions.StatusUpdateQueue, true, false, false, null);
					var properties = _channel.CreateBasicProperties();
					properties.Persistent = true;
					_channel.BasicPublish("", _rabbitOptions.StatusUpdateQueue, false, properties, statusUpdateBody);
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
	}
}
