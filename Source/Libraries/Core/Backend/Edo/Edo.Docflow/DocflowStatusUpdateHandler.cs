using Core.Infrastructure;
using Edo.Contracts.Messages.Events;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.NotificationSettings;

namespace Edo.Docflow
{
	public class DocflowStatusUpdateHandler : IDisposable
	{
		private readonly ILogger<DocflowStatusUpdateHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IEdoNotificationMessageFactory _notificationMessageFactory;
		private readonly INotificationPublisher<EdoNotificationMessage> _notificationPublisher;
		private readonly IPublishEndpoint _publishEndpoint;

		public DocflowStatusUpdateHandler(
			ILogger<DocflowStatusUpdateHandler> logger,
			IUnitOfWork uow,
			IEdoNotificationMessageFactory notificationMessageFactory,
			INotificationPublisher<EdoNotificationMessage> notificationPublisher,
			IPublishEndpoint publishEndpoint
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_notificationMessageFactory = notificationMessageFactory ?? throw new ArgumentNullException(nameof(notificationMessageFactory));
			_notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		public async Task HandleDocflowUpdatedEvent(EdoDocflowUpdatedEvent updatedEvent, CancellationToken cancellationToken)
		{
			var documentId = updatedEvent.EdoDocumentId;
			var document = await _uow.Session.GetAsync<OutgoingEdoDocument>(documentId, cancellationToken);
			if(document is null)
			{
				_logger.LogWarning("Документ {documentId} не найден", documentId);
				return;
			}

			var incomeStatus = updatedEvent.DocFlowStatus.TryParseAsEnum<EdoDocFlowStatus>();
			if(incomeStatus == null)
			{
				_logger.LogWarning("Получен пустой статус документооборота");
				return;
			}

			switch(document.Type)
			{
				case OutgoingEdoDocumentType.Transfer:
					await UpdateTransferDocumentDocflowStatus(
						(TransferEdoDocument)document, 
						updatedEvent, 
						incomeStatus.Value, 
						cancellationToken
					);
					break;
				case OutgoingEdoDocumentType.Order:
					await UpdateOrderDocumentDocflowStatus(
						(OrderEdoDocument)document, 
						updatedEvent, 
						incomeStatus.Value, 
						cancellationToken
					);
					break;
				case OutgoingEdoDocumentType.InformalOrderDocument:
					await UpdateInformalDocumentDocflowStatus(
						(OutgoingInformalEdoDocument)document, 
						updatedEvent, 
						incomeStatus.Value, 
						cancellationToken
					);
					break;
				default:
					throw new NotSupportedException($"Неизвестный тип документа {document.Type}");
			}

			await _uow.SaveAsync(document, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
		}

		private async Task UpdateOrderDocumentDocflowStatus(
			OrderEdoDocument document, 
			EdoDocflowUpdatedEvent updatedEvent,
			EdoDocFlowStatus incomeStatus,
			CancellationToken cancellationToken
		)
		{
			if(document.DiscrepancyState == EdoDiscrepancyState.Resolved)
			{
				// Расхождение уже разобрано: задача отменена вручную, по заказу при необходимости создана новая заявка.
				// Статус старого документооборота только фиксируем, событий не публикуем
				if(incomeStatus == EdoDocFlowStatus.Cancelled)
				{
					document.Status = EdoDocumentStatus.Cancelled;
				}

				_logger.LogInformation(
					"Получен статус документооборота {DocflowStatus} по документу {DocumentId} с разобранным расхождением",
					incomeStatus,
					document.Id);
				return;
			}

			switch(incomeStatus)
			{
				case EdoDocFlowStatus.NotStarted:
				case EdoDocFlowStatus.Unknown:
				case EdoDocFlowStatus.PreparingToSend:
					break;

				case EdoDocFlowStatus.Sent:
					document.Status = EdoDocumentStatus.Sent;
					await _publishEndpoint.Publish(new OrderDocumentSentEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.InProgress:
					document.Status = EdoDocumentStatus.InProgress;

					if(TaxcomDocflowStateByTransactions.ShouldMarkAsSent(incomeStatus, updatedEvent.IsReceived))
					{
						await _publishEndpoint.Publish(new OrderDocumentSentEvent(document.Id), cancellationToken);
					}
					break;

				case EdoDocFlowStatus.Succeed:
					if(document.DiscrepancyState == EdoDiscrepancyState.Active)
					{
						// Такском может вернуть Success вместо CompletedWithDivergences/NotAccepted,
						// расхождение при этом остается и должно быть разобрано вручную
						_logger.LogWarning(
							"Получен статус {DocflowStatus} по документу {DocumentId} с активным расхождением, статус проигнорирован",
							incomeStatus,
							document.Id);
						break;
					}

					var acceptTime = updatedEvent.StatusChangeTime ?? DateTime.Now;
					document.Status = EdoDocumentStatus.Succeed;
					document.AcceptTime = acceptTime;
					await _publishEndpoint.Publish(new OrderDocumentAcceptedEvent(document.Id), cancellationToken);
					break;
					 
				case EdoDocFlowStatus.Error:
					document.Status = EdoDocumentStatus.Error;
					await _publishEndpoint.Publish(new OrderDocumentProblemEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.Warning:
					await HandleDiscrepancy(document, EdoDocumentStatus.Warning, cancellationToken);
					break;

				case EdoDocFlowStatus.CompletedWithDivergences:
					await HandleDiscrepancy(document, EdoDocumentStatus.CompletedWithDivergences, cancellationToken);
					break;

				case EdoDocFlowStatus.NotAccepted:
					await HandleDiscrepancy(document, EdoDocumentStatus.NotAccepted, cancellationToken);
					break;

				case EdoDocFlowStatus.WaitingForCancellation:
					if(document.CancellationRequestTime.HasValue)
					{
						// Аннулирование запросили мы сами
						document.Status = EdoDocumentStatus.WaitingForCancellation;
						break;
					}

					await HandleDiscrepancy(document, EdoDocumentStatus.WaitingForCancellation, cancellationToken);
					break;

				case EdoDocFlowStatus.Cancelled:
					if(document.CancellationRequestTime.HasValue
						&& document.DiscrepancyState == EdoDiscrepancyState.None)
					{
						// Аннулирование, запрошенное нами, подтверждено
						document.Status = EdoDocumentStatus.Cancelled;
						await _publishEndpoint.Publish(new OrderDocumentCancelledEvent(document.Id), cancellationToken);
						break;
					}

					await HandleDiscrepancy(document, EdoDocumentStatus.Cancelled, cancellationToken);
					break;

				default:
					throw new InvalidOperationException($"Неизвестный статус документооборота {updatedEvent.DocFlowStatus}");
			}
		}

		private async Task UpdateTransferDocumentDocflowStatus(
			TransferEdoDocument document,
			EdoDocflowUpdatedEvent updatedEvent,
			EdoDocFlowStatus incomeStatus,
			CancellationToken cancellationToken
		)
		{
			switch(incomeStatus)
			{
				case EdoDocFlowStatus.Sent:
					document.Status = EdoDocumentStatus.Sent;
					break;

				case EdoDocFlowStatus.InProgress:
					document.Status = EdoDocumentStatus.InProgress;
					break;

				case EdoDocFlowStatus.Succeed:
					var acceptTime = updatedEvent.StatusChangeTime ?? DateTime.Now;
					document.Status = EdoDocumentStatus.Succeed;
					document.AcceptTime = acceptTime;
					await _publishEndpoint.Publish(new TransferDocumentAcceptedEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.Warning:
					document.Status = EdoDocumentStatus.Warning;
					await _publishEndpoint.Publish(new TransferDocumentProblemEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.CompletedWithDivergences:
					document.Status = EdoDocumentStatus.CompletedWithDivergences;
					await _publishEndpoint.Publish(new TransferDocumentProblemEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.Error:
					document.Status = EdoDocumentStatus.Error;
					await _publishEndpoint.Publish(new TransferDocumentProblemEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.WaitingForCancellation:
					document.Status = EdoDocumentStatus.WaitingForCancellation;
					await _publishEndpoint.Publish(new TransferDocumentProblemEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.Cancelled:
					document.Status = EdoDocumentStatus.Cancelled;
					await _publishEndpoint.Publish(new TransferDocumentCancelledEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.NotAccepted:
					await _publishEndpoint.Publish(new TransferDocumentProblemEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.NotStarted:
				case EdoDocFlowStatus.Unknown:
				case EdoDocFlowStatus.PreparingToSend:
					break;

				default:
					throw new InvalidOperationException($"Неизвестный статус документооборота {updatedEvent.DocFlowStatus}");
			}
		}

		private async Task UpdateInformalDocumentDocflowStatus(
			OutgoingInformalEdoDocument document,
			EdoDocflowUpdatedEvent updatedEvent,
			EdoDocFlowStatus incomeStatus,
			CancellationToken cancellationToken
		)
		{
			switch(incomeStatus)
			{
				case EdoDocFlowStatus.Sent:
					document.Status = EdoDocumentStatus.Sent;
					document.SendTime = updatedEvent.StatusChangeTime ?? DateTime.Now;
					break;

				case EdoDocFlowStatus.InProgress:
					document.Status = EdoDocumentStatus.InProgress;
					break;

				case EdoDocFlowStatus.Succeed:
					document.Status = EdoDocumentStatus.Succeed;
					document.AcceptTime = updatedEvent.StatusChangeTime ?? DateTime.Now;
					await _publishEndpoint.Publish(new InformalOrderDocumentAcceptedEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.Warning:
					document.Status = EdoDocumentStatus.Warning;
					await _publishEndpoint.Publish(new InformalOrderDocumentProblemEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.CompletedWithDivergences:
					document.Status = EdoDocumentStatus.CompletedWithDivergences;
					await _publishEndpoint.Publish(new InformalOrderDocumentProblemEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.Error:
					document.Status = EdoDocumentStatus.Error;
					await _publishEndpoint.Publish(new InformalOrderDocumentProblemEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.WaitingForCancellation:
					document.Status = EdoDocumentStatus.WaitingForCancellation;
					break;

				case EdoDocFlowStatus.Cancelled:
					document.Status = EdoDocumentStatus.Cancelled;
					await _publishEndpoint.Publish(new InformalOrderDocumentCancelledEvent(document.Id), cancellationToken);
					break;


				case EdoDocFlowStatus.NotAccepted:
					await _publishEndpoint.Publish(new InformalOrderDocumentProblemEvent(document.Id), cancellationToken);
					break;

				case EdoDocFlowStatus.NotStarted:
				case EdoDocFlowStatus.Unknown:
				case EdoDocFlowStatus.PreparingToSend:
					break;

				default:
					throw new InvalidOperationException($"Неизвестный статус документооборота {updatedEvent.DocFlowStatus}");
			}
		}

		private async Task HandleDiscrepancy(
			OrderEdoDocument document,
			EdoDocumentStatus discrepancyStatus,
			CancellationToken cancellationToken
		)
		{
			if(document.DiscrepancyState == EdoDiscrepancyState.Active && document.Status == discrepancyStatus)
			{
				// Повтор уже зафиксированного расхождения
				return;
			}

			var isNewDiscrepancy = document.DiscrepancyState != EdoDiscrepancyState.Active;

			document.Status = discrepancyStatus;
			document.DiscrepancyState = EdoDiscrepancyState.Active;

			if(!isNewDiscrepancy)
			{
				// Изменился тип расхождения, повторное уведомление не нужно
				return;
			}

			var edoRequest = await _uow.Session.QueryOver<FormalEdoRequest>()
				.Where(x => x.Task.Id == document.DocumentTaskId)
				.SingleOrDefaultAsync(cancellationToken);

			if(edoRequest == null)
			{
				throw new InvalidOperationException($"Не удалось найти FormalEdoRequest для документа {document.Id}");
			}

			var notification = _notificationMessageFactory.Create<DocflowDiscrepancyEdoNotificationSetting>(
				("OrderId", edoRequest.Order.Id.ToString(CultureInfo.InvariantCulture) ?? string.Empty)
			);

			var published = await _notificationPublisher.TryPublishAsync(_uow, notification, cancellationToken);
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
