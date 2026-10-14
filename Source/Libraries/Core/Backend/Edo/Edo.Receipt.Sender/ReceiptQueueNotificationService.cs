using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EdoNotifications.Application.Factories;
using Edo.Receipt;
using Edo.Contracts.Messages.Events;
using ModulKassa;
using System.Linq;
using EdoNotifications.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using TransactionalOutbox.Domain;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Receipt.Sender
{
	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	public class ReceiptQueueNotificationService : IReceiptQueueNotificationService
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEdoRepository _edoRepository;
		private readonly IEdoNotificationMessageFactory _messageFactory;
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _publisher;
		private readonly IOptions<ReceiptQueueNotificationOptions> _options;
		private readonly ILogger<ReceiptQueueNotificationService> _logger;
		private readonly CashboxClientProvider _cashboxClientProvider;

		/// <summary>
		/// Конструктор.
		/// </summary>
		/// <param name="unitOfWorkFactory">Фабрика единиц работы</param>
		/// <param name="edoRepository">Репозиторий ЭДО</param>
		/// <param name="messageFactory">Фабрика уведомлений</param>
		/// <param name="publisher">Публикация через outbox</param>
		/// <param name="options">Интервалы уведомлений</param>
		/// <param name="logger">Журнал событий</param>
		/// <param name="cashboxClientProvider">Клиенты МодульКассы</param>
		public ReceiptQueueNotificationService(
			IUnitOfWorkFactory unitOfWorkFactory,
			IEdoRepository edoRepository,
			IEdoNotificationMessageFactory messageFactory,
			IOutboxNotificationPublisher<EdoNotificationMessage> publisher,
			IOptions<ReceiptQueueNotificationOptions> options,
			ILogger<ReceiptQueueNotificationService> logger,
			CashboxClientProvider cashboxClientProvider)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
			_publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public async Task ProcessAsync(DateTime now, CancellationToken cancellationToken)
		{
			var statusChangedBefore = now.AddDays(-1);
			var statusChangedNotBefore = now.AddDays(-_options.Value.LookbackDays);
			var notifiedNotAfter = now - _options.Value.RepeatInterval;
			IList<int> documentIds;
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				documentIds = await _edoRepository.GetFiscalDocumentIdsForQueueNotification(
					uow, FiscalDocumentStatus.Queued, statusChangedBefore, statusChangedNotBefore, notifiedNotAfter, cancellationToken);
			}

			foreach(var documentId in documentIds)
			{
				cancellationToken.ThrowIfCancellationRequested();
				try
				{
					// Publisher схлопывает сообщения одного типа внутри UoW: каждому чеку нужна своя транзакция.
					using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
					{
						var document = uow.GetById<EdoFiscalDocument>(documentId);
						if(document == null || document.Status != FiscalDocumentStatus.Queued)
						{
							continue;
						}

						if(!document.StatusChangeTime.HasValue)
						{
							_logger.LogWarning("Чек {FiscalDocumentId} в очереди без времени смены статуса. Уведомление пропущено", documentId);
							continue;
						}

						if(document.StatusChangeTime.Value >= statusChangedBefore
							|| document.StatusChangeTime.Value < statusChangedNotBefore
							|| document.LastQueueNotificationTime > notifiedNotAfter)
						{
							continue;
						}

						var task = document.ReceiptEdoTask;
						if(!task.CashboxId.HasValue)
						{
							_logger.LogWarning("Для задачи ЭДО {EdoTaskId} не указана касса. Уведомление пропущено", task.Id);
							continue;
						}
						var cashbox = await _cashboxClientProvider.GetCashboxAsync(task.CashboxId.Value, cancellationToken);
						var result = await cashbox.CheckFiscalDocument(document.DocumentGuid.ToString(), cancellationToken);
						cancellationToken.ThrowIfCancellationRequested();
						if(result.SendStatus == ModulKassa.DTO.SendStatus.Error || result.FiscalDocumentInfo == null)
						{
							_logger.LogWarning("Не удалось проверить состояние чека {FiscalDocumentId}: {ErrorMessage}. Уведомление пропущено",
								documentId, result.ErrorMessage);
							continue;
						}
						var currentState = result.FiscalDocumentInfo;
						if(currentState.Status != ModulKassa.DTO.FiscalDocumentStatus.Queued)
						{
							var status = ReceiptConverters.ConvertFiscalDocumentStatus(currentState.Status);
							if(status == FiscalDocumentStatus.Printed || status == FiscalDocumentStatus.Completed)
							{
								if(currentState.FiscalInfo == null)
								{
									_logger.LogWarning("Для завершённого чека {FiscalDocumentId} касса не вернула фискальные данные", documentId);
									continue;
								}
								ReceiptFiscalDocumentUpdater.UpdateFiscalDocument(document, currentState);
							}
							else
							{
								document.Status = status;
								document.StatusChangeTime = DateTime.Parse(currentState.TimeStatusChangedString);
							}
							document.FailureMessage = currentState.FailureInfo?.Message;
							await uow.SaveAsync(document, cancellationToken: cancellationToken);
							if(task.Status != EdoTaskStatus.Completed && document.Stage == FiscalDocumentStage.Completed
								&& task.FiscalDocuments.All(x => x.Stage == FiscalDocumentStage.Completed
									&& (x.Status == FiscalDocumentStatus.Printed || x.Status == FiscalDocumentStatus.Completed)))
							{
								await uow.SaveAsync(new OutboxMessage(
									new ReceiptCompleteEvent { ReceiptEdoTaskId = task.Id }), cancellationToken: cancellationToken);
							}
							await uow.CommitAsync(cancellationToken);
							_logger.LogInformation("Обновлён чек {FiscalDocumentId}, статус {FiscalStatus}. Уведомление об очереди пропущено",
								documentId, document.Status);
							continue;
						}

						var message = _messageFactory.Create(
							EdoNotificationType.ReceiptQueueStalled,
							("FiscalDocumentId", document.Id.ToString(CultureInfo.InvariantCulture)),
							("DocumentNumber", document.DocumentNumber ?? string.Empty),
							("EdoTaskId", task.Id.ToString(CultureInfo.InvariantCulture)),
							("OrderId", task.FormalEdoRequest?.Order?.Id.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
							("CashboxId", task.CashboxId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
							("QueuedSince", document.StatusChangeTime.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
							("PreviousNotificationTime", document.LastQueueNotificationTime?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty));

						if(!await _publisher.TryPublishAsync(uow, message, cancellationToken))
						{
							continue;
						}

						document.LastQueueNotificationTime = now;
						await uow.SaveAsync(document, cancellationToken: cancellationToken);
						await uow.CommitAsync(cancellationToken);
						_logger.LogInformation("Зарегистрировано уведомление о чеке {FiscalDocumentId}, зависшем в очереди", documentId);
					}
				}
				catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
				{
					throw;
				}
				catch(Exception exception)
				{
					_logger.LogError(exception, "Не удалось зарегистрировать уведомление о чеке {FiscalDocumentId}", documentId);
				}
			}
		}
	}
}
