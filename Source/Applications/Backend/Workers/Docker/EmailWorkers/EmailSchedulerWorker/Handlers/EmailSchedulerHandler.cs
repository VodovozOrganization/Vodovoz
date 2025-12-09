using EmailSend.Library.Services;
using Mailganer.Api.Client.Dto;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;

namespace EmailSchedulerWorker.Handlers
{
	/// <summary>
	/// Обработчик планировщика по отправке email
	/// </summary>
	public class EmailSchedulerHandler : IEmailSchedulerHandler
	{
		private readonly ILogger<EmailSchedulerHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IEmailRepository _emailRepository;
		private readonly IEmailSendService _emailSendService;

		public EmailSchedulerHandler(
			ILogger<EmailSchedulerHandler> logger,
			IUnitOfWork uow,
			IEmailRepository emailRepository,
			IEmailSendService emailSendService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_emailSendService = emailSendService ?? throw new ArgumentNullException(nameof(emailSendService));
		}

		public async Task HandleNew(EmailMessage emailMessage, int clientId, IEnumerable<int> orderIds, CancellationToken cancellationToken)
		{
			try
			{
				var client = _uow.GetById<Counterparty>(clientId);
				if(client == null)
				{
					_logger.LogWarning("Контрагент с Id {ClientId} не найден", clientId);
					throw new ArgumentException($"Контрагент с Id {clientId} не найден");
				}

				if(emailMessage == null)
				{
					_logger.LogWarning("Email сообщение равно null");
					throw new ArgumentNullException(nameof(emailMessage));
				}

				var sendResult = await _emailSendService.SendEmail(emailMessage);

				if(!sendResult.IsSuccess)
				{
					_logger.LogError("Failed to send debt notification to client {ClientId}: {Error}",
						client.Id, sendResult.Errors);
					return;
				}

				_logger.LogInformation("Debt notification sent to client {ClientId} for {OrderCount} orders",
						client.Id, orderIds.Count());

				var orderIdList = orderIds.ToList();

				await _emailRepository.MarkOrdersAsSentAsync(_uow, orderIdList, DateTime.UtcNow, cancellationToken);
				await CreateCounterpartyDebtEmailRecordAsync(client, orderIdList, emailMessage, cancellationToken);

				_logger.LogInformation("Email сообщение успешно обработано и зафиксировано в БД");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке email сообщения");
				throw;
			}
		}

		/// <summary>
		/// Записывает в БД факт создания письма с уведомлением о задолженности
		/// </summary>
		private async Task CreateCounterpartyDebtEmailRecordAsync(
			Counterparty client,
			List<int> ordersIds,
			EmailMessage emailMessage,
			CancellationToken cancellationToken)
		{
			try
			{
				var storedEmail = new StoredEmail
				{
					State = StoredEmailStates.WaitingToSend,
					Author = null,
					ManualSending = false,
					SendDate = DateTime.Now,
					StateChangeDate = DateTime.Now,
					Subject = emailMessage.Subject,
					RecipientAddress = emailMessage.To,
					Guid = Guid.NewGuid(),
					Description = $"Уведомление о задолженности по заказам: {string.Join(", ", ordersIds)}",
				};

				await _uow.SaveAsync(storedEmail, cancellationToken: cancellationToken);

				var bulkEmail = new BulkEmail
				{
					StoredEmail = storedEmail,
					Counterparty = client
				};

				await _uow.SaveAsync(bulkEmail, cancellationToken: cancellationToken);

				await _uow.CommitAsync(cancellationToken);

				_logger.LogInformation("Created debt email record for client {ClientId} with {OrderCount} orders",
					client.Id, ordersIds.Count());
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error creating counterparty debt email record for client {ClientId}", client.Id);
				throw;
			}
		}
	}
}
