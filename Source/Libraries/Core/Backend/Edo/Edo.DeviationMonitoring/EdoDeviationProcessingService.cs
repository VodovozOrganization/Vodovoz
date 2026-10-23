using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Edo.DeviationMonitoring.Options;
using Edo.Transport;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using Microsoft.Extensions.Options;
using NHibernate.Proxy;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using TransactionalOutbox.Domain;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring
{
	/// <inheritdoc/>
	public class EdoDeviationProcessingService : IEdoDeviationProcessingService
	{
		private readonly IEdoDeviationRepository _repository;
		private readonly IOrderEdoTaskCreatedEventPublisher _taskPublisher;
		private readonly IEdoNotificationMessageFactory _messageFactory;
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _notificationPublisher;
		private readonly EdoDeviationMonitoringOptions _options;

		/// <summary>Создает обработчик автоматических действий по отклонениям.</summary>
		/// <param name="repository">Данные документов и итераций.</param>
		/// <param name="taskPublisher">Публикация событий запуска существующих задач.</param>
		/// <param name="messageFactory">Фабрика ЭДО-уведомлений.</param>
		/// <param name="notificationPublisher">Публикация уведомлений через аутбокс.</param>
		/// <param name="options">Настройки мониторинга.</param>
		public EdoDeviationProcessingService(IEdoDeviationRepository repository,
			IOrderEdoTaskCreatedEventPublisher taskPublisher,
			IEdoNotificationMessageFactory messageFactory,
			IOutboxNotificationPublisher<EdoNotificationMessage> notificationPublisher,
			IOptionsSnapshot<EdoDeviationMonitoringOptions> options)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_taskPublisher = taskPublisher ?? throw new ArgumentNullException(nameof(taskPublisher));
			_messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
			_notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		/// <inheritdoc/>
		public bool CanProcess(EdoTaskDeviation deviation, DateTime checkTime)
		{
			if(!_options.ProcessingStartDate.HasValue
				|| deviation.DetectedTime < _options.ProcessingStartDate.Value
				|| deviation.State != TaskProblemState.Active
				|| !deviation.DeviationSource.IsActive
				|| deviation.NotificationTime.HasValue)
			{
				return false;
			}

			switch(deviation.DeviationSource.DeviationType)
			{
				case EdoDeviationType.ClientNotAcceptedDocflow:
				case EdoDeviationType.TaskStalled:
				case EdoDeviationType.TransferProviderNoResponse:
				case EdoDeviationType.TransferTooLong:
				case EdoDeviationType.TransferStalled:
					return false;
				case EdoDeviationType.TaskNotCreated:
				case EdoDeviationType.TaskNotStarted:
				case EdoDeviationType.TransferNotStarted:
				case EdoDeviationType.DocumentNotSentToProvider:
				case EdoDeviationType.TransferDocumentNotCreated:
				case EdoDeviationType.TransferDocumentNotSentToProvider:
					return !deviation.RetryTime.HasValue
						|| checkTime >= deviation.RetryTime.Value + _options.ResolvingWorkerInterval;
				default:
					return true;
			}
		}

		/// <inheritdoc/>
		public async Task ProcessAsync(IUnitOfWork uow, EdoTaskDeviation deviation, DateTime checkTime,
			CancellationToken cancellationToken)
		{
			if(!CanProcess(deviation, checkTime))
			{
				return;
			}

			switch(deviation.DeviationSource.DeviationType)
			{
				case EdoDeviationType.TaskNotCreated:
				case EdoDeviationType.TaskNotStarted:
				case EdoDeviationType.TransferNotStarted:
				case EdoDeviationType.DocumentNotSentToProvider:
				case EdoDeviationType.TransferDocumentNotCreated:
				case EdoDeviationType.TransferDocumentNotSentToProvider:
					if(!deviation.RetryTime.HasValue)
					{
						await RetryAsync(uow, deviation, cancellationToken);
						deviation.RetryTime = checkTime;
						await uow.SaveAsync(deviation, cancellationToken: cancellationToken);
						return;
					}

					break;
				case EdoDeviationType.ProviderNoResponse:
				case EdoDeviationType.CancellationNotCompleted:
				case EdoDeviationType.GisMtResultMissing:
				case EdoDeviationType.GisMtRejected:
				case EdoDeviationType.ReceiptNotFiscalized:
				case EdoDeviationType.TransferWaitingRequestsTooLong:
				case EdoDeviationType.TransferGisMtResultMissing:
				case EdoDeviationType.TransferGisMtRejected:
				case EdoDeviationType.TransferCodesNotMoved:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(deviation), "Неизвестный тип отклонения ЭДО");
			}

			var message = _messageFactory.Create(EdoNotificationType.DocflowDeviation,
				("DeviationId", deviation.Id.ToString(CultureInfo.InvariantCulture)),
				("DeviationType", deviation.DeviationSource.DeviationType.ToString()),
				("Message", deviation.DeviationSource.ErrorMessage),
				("Details", deviation.Details),
				("EdoTaskId", deviation.EdoTask?.Id.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
				("EdoRequestId", deviation.EdoRequest?.Id.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
				("RetryTime", deviation.RetryTime?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty));

			if(await _notificationPublisher.TryPublishAsync(uow, message, cancellationToken))
			{
				deviation.NotificationTime = checkTime;
				await uow.SaveAsync(deviation, cancellationToken: cancellationToken);
			}
		}

		private async Task RetryAsync(IUnitOfWork uow, EdoTaskDeviation deviation, CancellationToken cancellationToken)
		{
			switch(deviation.DeviationSource.DeviationType)
			{
				case EdoDeviationType.TaskNotCreated:
					await EnqueueAsync(uow, deviation,
						new EdoRequestCreatedEvent { Id = deviation.EdoRequest.Id }, "request", cancellationToken);
					break;
				case EdoDeviationType.TaskNotStarted:
					var task = (OrderEdoTask)GetTaskImplementation(deviation.EdoTask);
					if(!task.WaitingProcessingTaskCreatedEvent)
					{
						_taskPublisher.Publish(uow, task, cancellationToken);
						task.UpdateWaitingProcessingTaskCreatedEvent(true);
						await uow.SaveAsync(task, cancellationToken: cancellationToken);
					}
					break;
				case EdoDeviationType.TransferNotStarted:
					var iterationIds = await _repository.GetTransferIterationIdsAsync(uow, deviation.EdoTask.Id,
						TransferEdoRequestIterationStatus.InProgress, cancellationToken);
					foreach(var id in iterationIds)
					{
						await EnqueueAsync(uow, deviation, new TransferRequestCreatedEvent { TransferIterationId = id },
							$"iteration:{id}", cancellationToken);
					}
					break;
				case EdoDeviationType.DocumentNotSentToProvider:
					var documentId = await _repository.GetOrderDocumentIdAsync(uow, deviation.EdoTask.Id, cancellationToken);
					await EnqueueAsync(uow, deviation, new OrderDocumentSendEvent
					{
						OrderDocumentId = documentId ?? throw new InvalidOperationException("Не найден исходящий документ задачи ЭДО")
					}, "document", cancellationToken);
					break;
				case EdoDeviationType.TransferDocumentNotCreated:
					var transferTask = (TransferEdoTask)GetTaskImplementation(deviation.EdoTask);
					object transferEvent = transferTask.TransferStatus == EdoTransferTaskStage.ReadyToSend
						? (object)new TransferTaskReadyToSendEvent { TransferTaskId = transferTask.Id }
						: new TransferTaskPrepareToSendEvent { TransferTaskId = transferTask.Id };
					await EnqueueAsync(uow, deviation, transferEvent, "prepare-transfer", cancellationToken);
					break;
				case EdoDeviationType.TransferDocumentNotSentToProvider:
					var transferDocumentId = await _repository.GetTransferDocumentIdAsync(uow, deviation.EdoTask.Id, cancellationToken);
					await EnqueueAsync(uow, deviation, new TransferDocumentSendEvent
					{
						TransferDocumentId = transferDocumentId ?? throw new InvalidOperationException("Не найден исходящий документ трансфера")
					}, "transfer-document", cancellationToken);
					break;
			}
		}

		private static EdoTask GetTaskImplementation(EdoTask task) => task is INHibernateProxy proxy
			? (EdoTask)proxy.HibernateLazyInitializer.GetImplementation()
			: task;

		private static Task EnqueueAsync(IUnitOfWork uow, EdoTaskDeviation deviation, object message,
			string actionKey, CancellationToken cancellationToken) =>
			uow.SaveAsync(new OutboxMessage(message, $"EdoDeviation:{deviation.Id}:{actionKey}"), cancellationToken: cancellationToken);
	}
}
