using Mailjet.Api.Abstractions;
using Mailjet.Api.Abstractions.Endpoints;
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
using CustomerAppsApi.Library.Configs;
using Mailjet.Api.Abstractions.Configs;
using Mailjet.Api.Abstractions.Events;
using Microsoft.Extensions.Options;

namespace EmailSendWorker
{
	public class EmailSendWorker : BackgroundService
	{
		private const int _retriesCount = 5;
		private const int _retryDelay = 5 * 1000; // sec => milisec

		private readonly ILogger<EmailSendWorker> _logger;
		private readonly IModel _channel;
		private readonly RabbitOptions _rabbitOptions;
		private readonly MailjetOptions _mailjetOptions;
		private readonly SendEndpoint _sendEndpoint;
		private readonly AsyncEventingBasicConsumer _consumer;

		public EmailSendWorker(
			ILogger<EmailSendWorker> logger,
			IModel channel,
			IOptions<MailjetOptions> mailjetOptions,
			IOptions<RabbitOptions> rabbitOptions,
			SendEndpoint sendEndpoint)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_channel = channel ?? throw new ArgumentNullException(nameof(channel));
			_mailjetOptions = (mailjetOptions ?? throw new ArgumentNullException(nameof(mailjetOptions))).Value;
			_rabbitOptions = (rabbitOptions ?? throw new ArgumentNullException(nameof(rabbitOptions))).Value;
			_sendEndpoint = sendEndpoint ?? throw new ArgumentNullException(nameof(sendEndpoint));
			_channel.QueueDeclare(_rabbitOptions.EmailSendQueue, true, false, false, null);
			_consumer = new AsyncEventingBasicConsumer(_channel);
			_consumer.Received += MessageRecieved;
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
					SandboxMode = _mailjetOptions.Sandbox
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

							_channel.QueueDeclare(_rabbitOptions.StatusUpdateQueue, true, false, false, null);
							var serializedMessage = JsonSerializer.Serialize(statusUpdateMessage);
							var statusUpdateBody = Encoding.UTF8.GetBytes(serializedMessage);
							var properties = _channel.CreateBasicProperties();
							properties.Persistent = true;
							_channel.BasicPublish("", _rabbitOptions.StatusUpdateQueue, false, properties, statusUpdateBody);
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
