using Mailjet.Api.Abstractions.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.MailSending;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;

namespace EmailStatusUpdateWorker
{
	public class EmailStatusUpdateWorker : BackgroundService
	{
		private const string _queuesConfigurationSection = "Queues";
		private const string _emailStatusUpdateQueueParameter = "EmailStatusUpdateQueue";

		private readonly string _storedEmailStatusUpdatingQueueId;

		private readonly ILogger<EmailStatusUpdateWorker> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IModel _channel;
		private readonly IEmailRepository _emailRepository;
		private readonly AsyncEventingBasicConsumer _consumer;

		public EmailStatusUpdateWorker(ILogger<EmailStatusUpdateWorker> logger, IUnitOfWorkFactory uowFactory, IConfiguration configuration, IModel channel, IEmailRepository emailRepository)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_channel = channel ?? throw new ArgumentNullException(nameof(channel));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_storedEmailStatusUpdatingQueueId = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailStatusUpdateQueueParameter);
			_channel.QueueDeclare(_storedEmailStatusUpdatingQueueId, true, false, false, null);
			_consumer = new AsyncEventingBasicConsumer(_channel);
			_consumer.Received += MessageRecieved;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_channel.BasicConsume(_storedEmailStatusUpdatingQueueId, false, _consumer);
			await Task.Delay(0, stoppingToken);
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting Stored Email Status Update Worker...");
			return base.StartAsync(cancellationToken);
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Stopping Stored Email Status Update Worker...");
			return base.StopAsync(cancellationToken);
		}

		private async Task MessageRecieved(object sender, BasicDeliverEventArgs e)
		{
			try
			{
				var body = e.Body;

				var message = JsonSerializer.Deserialize<UpdateStoredEmailStatusMessage>(body.Span);

				_logger.LogInformation($"Recieved message to update status for stored email with id: { message.EventPayload.Id }" +
					$" to status: { message.Status }, request recieved at: { message.RecievedAt }");

				if(message.EventPayload.Trackable)
				{
					using(var unitOfWork = _uowFactory.CreateWithoutRoot("Status update worker"))
					{
						var storedEmail = _emailRepository.GetById(unitOfWork, message.EventPayload.Id);

						if(storedEmail != null)
						{
							_logger.LogInformation($"Found Email: { storedEmail.Id }, externalId { storedEmail.ExternalId }, status { storedEmail.State }");

							if(storedEmail.StateChangeDate < message.RecievedAt)
							{
								try
								{
									var newStatus = ConvertFromMailjetStatus(message.Status);

									if(newStatus == StoredEmailStates.Undelivered || newStatus == StoredEmailStates.SendingError)
									{
										storedEmail.AddDescription(message.ErrorInfo);
									}

									storedEmail.State = newStatus;
									storedEmail.StateChangeDate = message.RecievedAt;
									storedEmail.ExternalId = message.MailjetMessageId;

									unitOfWork.Save(storedEmail);
									unitOfWork.Commit();

									_logger.LogInformation($"Email: { storedEmail.Id }, externalId { storedEmail.ExternalId }, status changed to { storedEmail.State }");
								}
								catch(ArgumentOutOfRangeException)
								{
									_logger.LogInformation($"Skipped event for emaid with id: { storedEmail.Id }, externalId { storedEmail.ExternalId } for status change to { message.Status }");
								}
							}
						}
						else
						{
							_logger.LogWarning($"Stored Email with id: { message.EventPayload.Id } not found");
						}
					}
				}

				_channel.BasicAck(e.DeliveryTag, false);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				throw;
			}
		}

		private StoredEmailStates ConvertFromMailjetStatus(MailEventType mailEventType)
		{
			switch(mailEventType)
			{
				case MailEventType.sent:
					return StoredEmailStates.Delivered;
				case MailEventType.open:
					return StoredEmailStates.Opened;
				case MailEventType.spam:
					return StoredEmailStates.MarkedAsSpam;
				case MailEventType.bounce:
					return StoredEmailStates.SendingError;
				case MailEventType.blocked:
					return StoredEmailStates.Undelivered;
			}

			throw new ArgumentOutOfRangeException(nameof(mailEventType), $"Тип события { mailEventType } не поддерживается");
		}
	}
}
