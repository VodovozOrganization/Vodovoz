using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Transport
{
	/// <summary>
	/// Публикует события запуска задач ЭДО заказа
	/// </summary>
	public class OrderEdoTaskCreatedEventPublisher : IOrderEdoTaskCreatedEventPublisher
	{
		private readonly ILogger<OrderEdoTaskCreatedEventPublisher> _logger;
		private readonly IBus _bus;

		public OrderEdoTaskCreatedEventPublisher(
			ILogger<OrderEdoTaskCreatedEventPublisher> logger,
			IBus bus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}

		/// <inheritdoc />
		public async Task Publish(OrderEdoTask edoTask, CancellationToken cancellationToken = default)
		{
			if(edoTask is null)
			{
				throw new ArgumentNullException(nameof(edoTask));
			}

			switch(edoTask)
			{
				case DocumentEdoTask documentTask:
					await PublishDocumentCreatedEvent(documentTask, cancellationToken);
					break;
				case TenderEdoTask tenderTask:
					await PublishTenderCreatedEvent(tenderTask, cancellationToken);
					break;
				case ReceiptEdoTask receiptTask:
					await PublishReceiptCreatedEvent(receiptTask, cancellationToken);
					break;
				case SaveCodesEdoTask saveCodesTask:
					await PublishSaveCodesCreatedEvent(saveCodesTask, cancellationToken);
					break;
				default:
					throw new ArgumentOutOfRangeException(
						$"Задача ЭДО {edoTask.Id}: неизвестный тип задачи {edoTask.GetType().Name}, не удалось определить событие для запуска");
			}
		}

		private async Task PublishDocumentCreatedEvent(DocumentEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.Stage != DocumentEdoTaskStage.New)
			{
				LogInvalidState(edoTask, edoTask.Stage);
				return;
			}

			LogPublishing(edoTask, nameof(DocumentTaskCreatedEvent));
			await _bus.Publish(new DocumentTaskCreatedEvent { Id = edoTask.Id }, cancellationToken);
		}

		private async Task PublishTenderCreatedEvent(TenderEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.Stage != TenderEdoTaskStage.New)
			{
				LogInvalidState(edoTask, edoTask.Stage);
				return;
			}

			LogPublishing(edoTask, nameof(TenderTaskCreatedEvent));
			await _bus.Publish(new TenderTaskCreatedEvent { TenderEdoTaskId = edoTask.Id }, cancellationToken);
		}

		private async Task PublishReceiptCreatedEvent(ReceiptEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.ReceiptStatus != EdoReceiptStatus.New)
			{
				LogInvalidState(edoTask, edoTask.ReceiptStatus);
				return;
			}

			LogPublishing(edoTask, nameof(ReceiptTaskCreatedEvent));
			await _bus.Publish(new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = edoTask.Id }, cancellationToken);
		}

		private async Task PublishSaveCodesCreatedEvent(SaveCodesEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.Status != EdoTaskStatus.New)
			{
				LogInvalidState(edoTask, edoTask.Status);
				return;
			}

			LogPublishing(edoTask, nameof(SaveCodesTaskCreatedEvent));
			await _bus.Publish(new SaveCodesTaskCreatedEvent { EdoTaskId = edoTask.Id }, cancellationToken);
		}

		private void LogInvalidState(OrderEdoTask edoTask, object state)
		{
			_logger.LogWarning(
				"Задача ЭДО {EdoTaskId} ({TaskType}) находится в состоянии {State}. Запуск возможен только из начального состояния",
				edoTask.Id,
				edoTask.GetType().Name,
				state);
		}

		private void LogPublishing(OrderEdoTask edoTask, string eventName)
		{
			_logger.LogInformation(
				"Публикуем событие {EventName} для задачи ЭДО {EdoTaskId} ({TaskType})",
				eventName,
				edoTask.Id,
				edoTask.GetType().Name);
		}
	}
}
