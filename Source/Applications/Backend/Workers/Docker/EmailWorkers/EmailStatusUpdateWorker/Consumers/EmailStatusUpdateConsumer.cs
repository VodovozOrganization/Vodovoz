using System;
using System.Threading.Tasks;
using EmailStatusUpdateWorker.Extensions;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;

namespace EmailStatusUpdateWorker.Consumers
{
	public class EmailStatusUpdateConsumer : IConsumer<UpdateStoredEmailStatusMessage>
	{
		private readonly ILogger<EmailStatusUpdateConsumer> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmailRepository _emailRepository;

		public EmailStatusUpdateConsumer(
			ILogger<EmailStatusUpdateConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmailRepository emailRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
		}
		
		public async Task Consume(ConsumeContext<UpdateStoredEmailStatusMessage> context)
		{
			var message = context.Message;

			_logger.LogInformation(
				"Recieved message to update status for stored email with id: {EmailId}" +
				" to status: {NewStatus}, request recieved at: {RecievedDateTime}",
				message.EventPayload.Id,
				message.Status,
				message.RecievedAt);

			if(!message.EventPayload.Trackable)
			{
				return;
			}

			using var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Обновление статуса письма");

			var storedEmail = _emailRepository.GetById(
				unitOfWork,
				message.EventPayload.Id);

			if(storedEmail is null)
			{
				_logger.LogWarning(
					"Stored Email with id: {EmailId} externalId: {MailjetMessageId} not found",
					message.EventPayload.Id,
					message.MailjetMessageId);

				return;
			}

			_logger.LogInformation("Found Email: {EmailId}," +
				" externalId {ExternalEmailId}, status {OldStatus}",
				storedEmail.Id,
				storedEmail.ExternalId,
				storedEmail.State);

			if(storedEmail.StateChangeDate >= message.RecievedAt)
			{
				_logger.LogInformation("Skipped event for email with id: {EmailId}," +
					" externalId {ExternalEmailId} for status change to {NewStatus}. StateChangeDate: {StateChangeDate}. RecievedAt: {RecievedAt}",
					storedEmail.Id,
					storedEmail.ExternalId,
					message.Status,
					storedEmail.StateChangeDate,
					message.RecievedAt);
				
				return;
			}

			var newStatus = StoredEmailStates.WaitingToSend;

			try
			{
				newStatus = message.Status.MapToStoredEmailStates();
			}
			catch(ArgumentOutOfRangeException)
			{
				_logger.LogInformation("Skipped event for email with id: {EmailId}," +
					" externalId {ExternalEmailId} for status change to {NewStatus}",
					storedEmail.Id,
					storedEmail.ExternalId,
					message.Status);

				return;
			}

			if(newStatus is StoredEmailStates.Undelivered or StoredEmailStates.SendingError)
			{
				storedEmail.AddDescription(message.ErrorInfo);
			}

			storedEmail.State = newStatus;
			storedEmail.StateChangeDate = message.RecievedAt;
			storedEmail.ExternalId = message.MailjetMessageId;

			try
			{
				await unitOfWork.SaveAsync(storedEmail);
				await unitOfWork.CommitAsync();

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
			}
		}
	}
}
