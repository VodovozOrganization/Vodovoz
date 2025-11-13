using CustomerAppsApi.Library.Configs;
using Mailganer.Api.Client;
using Mailganer.Api.Client.Dto;
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
		private const int _retriesCount = 5;
		private const int _retryDelay = 5 * 1000; // sec => milisec

		private readonly ILogger<EmailSendWorker> _logger;
		private readonly IModel _channel;
		private readonly MailganerClientV2 _mailganerClient;
		private readonly RabbitOptions _rabbitOptions;
		private readonly AsyncEventingBasicConsumer _consumer;

		public EmailSendWorker(
			ILogger<EmailSendWorker> logger,
			IModel channel,
			IOptions<RabbitOptions> rabbitOptions,
			MailganerClientV2 mailganerClient
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_channel = channel ?? throw new ArgumentNullException(nameof(channel));
			_mailganerClient = mailganerClient ?? throw new ArgumentNullException(nameof(mailganerClient));
			_rabbitOptions = (rabbitOptions ?? throw new ArgumentNullException(nameof(rabbitOptions))).Value;
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

				var emails = CreateEmailMessages(message);

				foreach(var email in emails)
				{
					for(var i = 0; i < _retriesCount; i++)
					{
						_logger.LogInformation($"Sending email {message.Payload.Id} {i + 1}/{_retriesCount}");

						try
						{
							await _mailganerClient.Send(email);

							var semaphore = new object();

							try
							{
								lock(semaphore)
								{
									var statusUpdateMessage = new UpdateStoredEmailStatusMessage
									{
										EventPayload = new EmailPayload { Id = message.Payload.Id, Trackable = true },
										Status = Mailjet.Api.Abstractions.Events.MailEventType.sent,
										RecievedAt = DateTime.Now
									};
									_channel.QueueDeclare(_rabbitOptions.StatusUpdateQueue, true, false, false, null);
									var serializedMessage = JsonSerializer.Serialize(statusUpdateMessage);
									var statusUpdateBody = Encoding.UTF8.GetBytes(serializedMessage);
									var properties = _channel.CreateBasicProperties();
									properties.Persistent = true;
									_channel.BasicPublish("", _rabbitOptions.StatusUpdateQueue, false, properties, statusUpdateBody);
								}
							}
							catch(Exception ex)
							{
								_logger.LogError(ex, "Произошла ошибка при попытке отправки сообщения об изменении статуса в очередь");
							}
							
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
									Status = Mailjet.Api.Abstractions.Events.MailEventType.bounce,
									RecievedAt = DateTime.Now
								};

								_channel.QueueDeclare(_rabbitOptions.StatusUpdateQueue, true, false, false, null);
								var serializedMessage = JsonSerializer.Serialize(statusUpdateMessage);
								var statusUpdateBody = Encoding.UTF8.GetBytes(serializedMessage);
								var properties = _channel.CreateBasicProperties();
								properties.Persistent = true;
								_channel.BasicPublish("", _rabbitOptions.StatusUpdateQueue, false, properties, statusUpdateBody);
							}
						}
					}
				}
			}
			finally
			{
				_logger.LogInformation("Free message from queue");
				_channel.BasicAck(e.DeliveryTag, false);
			}
		}

		private IEnumerable<EmailMessage> CreateEmailMessages(SendEmailMessage message)
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
