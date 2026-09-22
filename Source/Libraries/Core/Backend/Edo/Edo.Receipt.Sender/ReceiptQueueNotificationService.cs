using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
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

		/// <summary>
		/// Конструктор.
		/// </summary>
		/// <param name="unitOfWorkFactory">Фабрика единиц работы</param>
		/// <param name="edoRepository">Репозиторий ЭДО</param>
		/// <param name="messageFactory">Фабрика уведомлений</param>
		/// <param name="publisher">Публикация через outbox</param>
		/// <param name="options">Интервалы уведомлений</param>
		/// <param name="logger">Журнал событий</param>
		public ReceiptQueueNotificationService(
			IUnitOfWorkFactory unitOfWorkFactory,
			IEdoRepository edoRepository,
			IEdoNotificationMessageFactory messageFactory,
			IOutboxNotificationPublisher<EdoNotificationMessage> publisher,
			IOptions<ReceiptQueueNotificationOptions> options,
			ILogger<ReceiptQueueNotificationService> logger)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
			_publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public async Task ProcessAsync(DateTime now, CancellationToken cancellationToken)
		{
			var statusChangedBefore = now.AddDays(-1);
			var statusChangedNotBefore = now.AddMonths(-_options.Value.LookbackMonths);
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
