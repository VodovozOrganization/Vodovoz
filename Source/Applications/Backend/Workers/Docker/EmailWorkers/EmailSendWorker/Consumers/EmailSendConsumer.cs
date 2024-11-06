using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Mailjet.Api.Abstractions;
using Mailjet.Api.Abstractions.Configs;
using Mailjet.Api.Abstractions.Endpoints;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.MailSending;

namespace EmailSendWorker.Consumers
{
	public class EmailSendConsumer : IConsumer<SendEmailMessage>
	{
		private readonly ILogger<EmailSendConsumer> _logger;
		private readonly SendEndpoint _sendEndpoint;
		private readonly MailjetOptions _mailjetOptions;

		public EmailSendConsumer(
			ILogger<EmailSendConsumer> logger,
			IOptions<MailjetOptions> mailjetOptions,
			SendEndpoint sendEndpoint)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_sendEndpoint = sendEndpoint ?? throw new ArgumentNullException(nameof(sendEndpoint));
			_mailjetOptions = (mailjetOptions ?? throw new ArgumentNullException(nameof(mailjetOptions))).Value;
		}
		
		public async Task Consume(ConsumeContext<SendEmailMessage> context)
		{
			var message = context.Message;

			var recipients = new StringBuilder();
			if(message.To != null)
            {
	            recipients.AppendJoin(',', message.To.Select(recipient => recipient?.Email));
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

            var payload = new SendPayload
            {
            	Messages = new[] { message },
            	SandboxMode = _mailjetOptions.Sandbox
            };
            
            _logger.LogInformation("Sending email {MessagePayloadId}", message.Payload.Id);

            try
            {
            	await _sendEndpoint.Send(payload);
            }
            catch(Exception e)
            {
            	_logger.LogError(e, e.Message);
	            throw;
            }
		}
	}
}
