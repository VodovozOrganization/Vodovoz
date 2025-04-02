using EmailStatusUpdateWorker.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Services;
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

		public EmailStatusUpdateWorker(
			IUserService userService,
			ILogger<EmailStatusUpdateWorker> logger,
			IUnitOfWorkFactory uowFactory,
			IConfiguration configuration,
			IModel channel,
			IEmailRepository emailRepository)
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
			_channel.BasicConsume(
				_storedEmailStatusUpdatingQueueId,
				false,
				_consumer);

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
			var body = e.Body;

			UpdateStoredEmailStatusMessage message;

			try
			{
				message = JsonSerializer
					.Deserialize<UpdateStoredEmailStatusMessage>(body.Span);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error occured while deserialization of UpdateStoredEmailStatusMessage: {ExceptionMessage}", ex.Message);
				
				_channel.BasicAck(e.DeliveryTag, false);
				return;
			}

			_logger.LogInformation(
				"Recieved message to update status for stored email with id: {EmailId}" +
				" to status: {NewStatus}, request recieved at: {RecievedDateTime}",
				message.EventPayload.Id,
				message.Status,
				message.RecievedAt);

			if(!message.EventPayload.Trackable)
			{
				_channel.BasicAck(e.DeliveryTag, false);
				return;
			}

			using var unitOfWork = _uowFactory.CreateWithoutRoot("Status update worker");

			var storedEmail = _emailRepository.GetById(
				unitOfWork,
				message.EventPayload.Id);

			if(storedEmail is null)
			{
				_logger.LogWarning(
					"Stored Email with id: {EmailId} not found",
					message.EventPayload.Id);

				_channel.BasicAck(e.DeliveryTag, false);
				return;
			}

			_logger.LogInformation("Found Email: {EmailId}," +
				" externalId {ExternalEmailId}, status {OldStatus}",
				storedEmail.Id,
				storedEmail.ExternalId,
				storedEmail.State);

			if(storedEmail.StateChangeDate >= message.RecievedAt)
			{
				_logger.LogInformation("Skipped event for emaid with id: {EmailId}," +
					" externalId {ExternalEmailId} for status change to {NewStatus}. StateChangeDate: {StateChangeDate}. RecievedAt: {RecievedAt}",
					storedEmail.Id,
					storedEmail.ExternalId,
					message.Status,
					storedEmail.StateChangeDate,
					message.RecievedAt);

				_channel.BasicAck(e.DeliveryTag, false);
				return;
			}

			StoredEmailStates newStatus;

			try
			{
				newStatus = message.Status.MapToStoredEmailStates();
			}
			catch(ArgumentOutOfRangeException)
			{
				_logger.LogInformation("Skipped event for emaid with id: {EmailId}," +
					" externalId {ExternalEmailId} for status change to {NewStatus}",
					storedEmail.Id,
					storedEmail.ExternalId,
					message.Status);

				_channel.BasicAck(e.DeliveryTag, false);
				return;
			}

			if(newStatus == StoredEmailStates.Undelivered
				|| newStatus == StoredEmailStates.SendingError)
			{
				storedEmail.AddDescription(message.ErrorInfo);
			}

			storedEmail.State = newStatus;
			storedEmail.StateChangeDate = message.RecievedAt;
			storedEmail.ExternalId = message.MailjetMessageId;

			try
			{
				unitOfWork.Save(storedEmail);
				unitOfWork.Commit();

				_logger.LogInformation("Email: {EmailId}," +
					" externalId {ExternalEmailId}, status changed to {NewStatus}",
					storedEmail.Id,
					storedEmail.ExternalId,
					storedEmail.State);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Error occured while saving new Email Status: {ExceptionMessage}",
					ex.Message);
				return;
			}

			_channel.BasicAck(e.DeliveryTag, false);

			await Task.CompletedTask;
		}
	}
}
