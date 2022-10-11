using Mailjet.Api.Abstractions;
using Mailjet.Api.Abstractions.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.MailSending;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mailjet.Api.Abstractions.Events;

namespace EmailSendWorker
{
	public class EmailSendWorker : BackgroundService
	{
		private const string _mailjetConfigurationSection = "Mailjet";
		private const string _queuesConfigurationSection = "Queues";
		private const string _emailSendQueueParameter = "EmailSendQueue";
		private const string _emailStatusUpdateParameter = "EmailStatusUpdateQueue";
		private const string _sandboxModeParameter = "Sandbox";

		private const int _retriesCount = 5;
		private const int _retryDelay = 5 * 1000; // sec => milisec

		private readonly string _mailSendingQueueId;
		private readonly string _statusUpdateQueueId;
		private readonly bool _sandboxMode;

		private readonly ILogger<EmailSendWorker> _logger;
		private readonly IModel _channel;
		private readonly SendEndpoint _sendEndpoint;
		private readonly AsyncEventingBasicConsumer _consumer;

		public EmailSendWorker(ILogger<EmailSendWorker> logger, IConfiguration configuration, IModel channel, SendEndpoint sendEndpoint)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_channel = channel ?? throw new ArgumentNullException(nameof(channel));
			_sendEndpoint = sendEndpoint ?? throw new ArgumentNullException(nameof(sendEndpoint));
			_mailSendingQueueId = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailSendQueueParameter);
			_statusUpdateQueueId = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailStatusUpdateParameter);
			_channel.QueueDeclare(_mailSendingQueueId, true, false, false, null);
			_consumer = new AsyncEventingBasicConsumer(_channel);
			_consumer.Received += MessageRecieved;
			_sandboxMode = configuration.GetSection(_mailjetConfigurationSection).GetValue(_sandboxModeParameter, true);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_channel.BasicConsume(_mailSendingQueueId, false, _consumer);
			await Task.Delay(0, stoppingToken);
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting Email Send Worker...");
			return base.StartAsync(cancellationToken);
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Stopping Email Send Worker...");
			return base.StopAsync(cancellationToken);
		}

		private async Task MessageRecieved(object sender, BasicDeliverEventArgs e)
		{
			ReadOnlyMemory<byte> body = null;

			try
			{
				body = e.Body;

				var message = JsonSerializer.Deserialize<SendEmailMessage>(body.Span);

				string[] recipients = new string[] { };
				if(message.To != null)
				{
					recipients = message.To.Select(recipient => recipient?.Email).ToArray();
				}

				_logger.LogInformation(
					$"Recieved message to send to recipients: { string.Join(", ", recipients) }" +
					$" with subject: \"{ message.Subject }\", with { message.Attachments?.Count ?? 0 } attachments");


				if(message.EventPayload == null)
				{
					message.Payload = new EmailPayload();
				}

				var payload = new SendPayload
				{
					Messages = new[] { message },
					SandboxMode = _sandboxMode
				};

				for(var i = 0; i < _retriesCount; i++)
				{
					_logger.LogInformation($"Sending email { message.Payload.Id } { i + 1 }/{ _retriesCount }");

					try
					{
						var response = await _sendEndpoint.Send(payload);
						_logger.LogInformation("Response:\n" + JsonSerializer.Serialize(response));
						break;
					}
					catch(Exception exc)
					{
						_logger.LogError(exc, exc.Message);
						await Task.Delay(_retryDelay);

						if(i >= _retriesCount - 1)
						{
							var statusUpdateMessage = new UpdateStoredEmailStatusMessage
							{
								ErrorInfo = "SendWorker unable to send message to MailJet",
								EventPayload = new EmailPayload { Id = message.Payload.Id, Trackable = true },
								Status = MailEventType.bounce,
								RecievedAt = DateTime.Now
							};

							_channel.QueueDeclare(_statusUpdateQueueId, true, false, false, null);
							var serializedMessage = JsonSerializer.Serialize(statusUpdateMessage);
							var statusUpdateBody = Encoding.UTF8.GetBytes(serializedMessage);
							var properties = _channel.CreateBasicProperties();
							properties.Persistent = true;
							_channel.BasicPublish("", _statusUpdateQueueId, false, properties, statusUpdateBody);
						}

						continue;
					}
				}
			}
			finally
			{
				_logger.LogInformation("Free message from queue");
				_channel.BasicAck(e.DeliveryTag, false);
			}
		}
	}
}
