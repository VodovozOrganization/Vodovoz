using EmailSend.Library.Handlers;
using MassTransit;
using Microsoft.Extensions.Logging;
using RabbitMQ.MailSending;
using System;
using System.Threading.Tasks;

namespace EmailSend.Library.Consumers
{
	public class SendEmailMessageConsumer : IConsumer<SendEmailMessage>
	{
		private readonly ILogger<SendEmailMessageConsumer> _logger;
		private readonly SendEmailMessageHandler _sendEmailMessageHandler;

		public SendEmailMessageConsumer(
			ILogger<SendEmailMessageConsumer> logger,
			SendEmailMessageHandler sendEmailMessageHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_sendEmailMessageHandler = sendEmailMessageHandler ?? throw new ArgumentNullException(nameof(sendEmailMessageHandler));
		}

		public async Task Consume(ConsumeContext<SendEmailMessage> context)
		{
			try
			{
				var message = context.Message;
				await _sendEmailMessageHandler.Handle(message);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing message from queue: {ErrorMessage}", ex.Message);
				throw;
			}
		}
	}
}
