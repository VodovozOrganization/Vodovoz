using CustomerAppsApi.Library.Configs;
using EmailSend.Library.Handlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.MailSending;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EmailSendWorker
{
	public class EmailSendWorker : BackgroundService
	{
		private readonly ILogger<EmailSendWorker> _logger;
		private readonly RabbitOptions _rabbitOptions;

		private readonly IModel _channel;
		private readonly SendEmailMessageHandler _sendEmailMessageHandler;
		private readonly AsyncEventingBasicConsumer _consumer;

		public EmailSendWorker(
			ILogger<EmailSendWorker> logger,
			IModel channel,
			IOptions<RabbitOptions> rabbitOptions,
			SendEmailMessageHandler sendEmailMessageHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_channel = channel ?? throw new ArgumentNullException(nameof(channel));
			_sendEmailMessageHandler = sendEmailMessageHandler ?? throw new ArgumentNullException(nameof(sendEmailMessageHandler));
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
				await _sendEmailMessageHandler.Handle(message);
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
	}
}
