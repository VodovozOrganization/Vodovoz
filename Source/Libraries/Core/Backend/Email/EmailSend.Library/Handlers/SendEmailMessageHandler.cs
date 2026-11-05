using EmailSend.Library.Factories;
using EmailSend.Library.Services;
using Mailganer.Api.Client.Dto;
using Mailjet.Api.Abstractions.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using RabbitMQ.MailSending;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace EmailSend.Library.Handlers
{
	public class SendEmailMessageHandler
	{
		private const int _retryCount = 5;
		private const int _retryDelay = 5 * 1000; // sec => milisec
		private const int _errorInfoMaxLength = 1000;

		private readonly ILogger<SendEmailMessageHandler> _logger;
		private readonly IEmailMessageFactory _emailMessageFactory;
		private readonly IEmailSendService _emailSendService;
		// IBus напрямую, без outbox: обработчик не работает с БД, он только отправляет письмо через Mailganer
		// и публикует его статус. Если брокер недоступен, статус не дойдёт (ошибка пишется в лог),
		// а повторная доставка сообщения может отправить письмо дважды.
		// Outbox и защиту от повторов удобнее сделать после перехода на один vhost: исчезнет вторая шина IEmailSendBus,
		// а старый обработчик без MassTransit (класс EmailSendWorker) удаляется после релиза — см. пометку в EmailSendWorker/Program.cs.
		// Тогда останутся один потребитель и одна шина: AddVodovozOutbox(OutboxBusNames.Email) и IPublishEndpoint вместо IBus
		private readonly IBus _messageBus;

		public SendEmailMessageHandler(
			ILogger<SendEmailMessageHandler> logger,
			IEmailMessageFactory emailMessageFactory,
			IEmailSendService emailSendService,
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_emailMessageFactory = emailMessageFactory ?? throw new ArgumentNullException(nameof(emailMessageFactory));
			_emailSendService = emailSendService ?? throw new ArgumentNullException(nameof(emailSendService));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task Handle(SendEmailMessage message)
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

					await PublishStoredEmailStatusUpdateMessage(
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

					await PublishStoredEmailStatusUpdateMessage(
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
						await _emailSendService.CheckAndRemoveSpamEmailFromStopList(emailMessage.To, emailMessage.FromAddress);

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

		private Task PublishStoredEmailStatusUpdateMessage(
			int messagePayloadId,
			MailEventType mailEventType,
			string errorInfo)
		{
			var statusUpdateMessage = new UpdateStoredEmailStatusMessage
			{
				ErrorInfo = errorInfo.Length > _errorInfoMaxLength ? errorInfo.Substring(0, _errorInfoMaxLength) : errorInfo,
				EventPayload = new EmailPayload { Id = messagePayloadId, Trackable = true },
				Status = mailEventType,
				RecievedAt = DateTime.Now
			};

			return PublishStoredEmailStatusUpdateMessage(statusUpdateMessage);
		}

		/// <remarks>
		/// Ошибка публикации не пробрасывается: письмо уже отправлено, повтор обработки отправил бы его ещё раз
		/// </remarks>
		private async Task PublishStoredEmailStatusUpdateMessage(UpdateStoredEmailStatusMessage message)
		{
			try
			{
				_logger.LogInformation(
					"Publishing email status update message. MessagePayloadId: {MessagePayloadId}, Status: {Status}",
					message.EventPayload.Id,
					message.Status);

				await _messageBus.Publish(message);

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
