using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactionalOutbox.Domain;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Transport
{
	/// <summary>
	/// Кладёт в аутбокс события запуска задач ЭДО заказа. Фактическая отправка в RabbitMQ
	/// происходит асинхронно, силами OutboxWorker, после коммита транзакции с задачей.
	/// </summary>
	public class OrderEdoTaskCreatedEventPublisher : IOrderEdoTaskCreatedEventPublisher
	{
		private readonly ILogger<OrderEdoTaskCreatedEventPublisher> _logger;

		public OrderEdoTaskCreatedEventPublisher(ILogger<OrderEdoTaskCreatedEventPublisher> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public void Publish(IUnitOfWork uow, OrderEdoTask edoTask, CancellationToken cancellationToken = default)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(edoTask is null)
			{
				throw new ArgumentNullException(nameof(edoTask));
			}

			switch(edoTask)
			{
				case DocumentEdoTask documentTask:
					PublishDocumentCreatedEvent(uow, documentTask);
					break;
				case TenderEdoTask tenderTask:
					PublishTenderCreatedEvent(uow, tenderTask);
					break;
				case ReceiptEdoTask receiptTask:
					PublishReceiptCreatedEvent(uow, receiptTask);
					break;
				case SaveCodesEdoTask saveCodesTask:
					PublishSaveCodesCreatedEvent(uow, saveCodesTask);
					break;
				default:
					throw new ArgumentOutOfRangeException(
						$"Задача ЭДО {edoTask.Id}: неизвестный тип задачи {edoTask.GetType().Name}, не удалось определить событие для запуска");
			}
		}

		private void PublishDocumentCreatedEvent(IUnitOfWork uow, DocumentEdoTask edoTask)
		{
			if(edoTask.Stage != DocumentEdoTaskStage.New)
			{
				LogInvalidState(edoTask, edoTask.Stage);
				return;
			}

			var @event = new DocumentTaskCreatedEvent { Id = edoTask.Id };
			SaveToOutbox(uow, edoTask, @event, nameof(DocumentTaskCreatedEvent));
		}

		private void PublishTenderCreatedEvent(IUnitOfWork uow, TenderEdoTask edoTask)
		{
			if(edoTask.Stage != TenderEdoTaskStage.New)
			{
				LogInvalidState(edoTask, edoTask.Stage);
				return;
			}

			var @event = new TenderTaskCreatedEvent { TenderEdoTaskId = edoTask.Id };
			SaveToOutbox(uow, edoTask, @event, nameof(TenderTaskCreatedEvent));
		}

		private void PublishReceiptCreatedEvent(IUnitOfWork uow, ReceiptEdoTask edoTask)
		{
			if(edoTask.ReceiptStatus != EdoReceiptStatus.New)
			{
				LogInvalidState(edoTask, edoTask.ReceiptStatus);
				return;
			}

			var @event = new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = edoTask.Id };
			SaveToOutbox(uow, edoTask, @event, nameof(ReceiptTaskCreatedEvent));
		}

		private void PublishSaveCodesCreatedEvent(IUnitOfWork uow, SaveCodesEdoTask edoTask)
		{
			if(edoTask.Status != EdoTaskStatus.New)
			{
				LogInvalidState(edoTask, edoTask.Status);
				return;
			}

			var @event = new SaveCodesTaskCreatedEvent { EdoTaskId = edoTask.Id };
			SaveToOutbox(uow, edoTask, @event, nameof(SaveCodesTaskCreatedEvent));
		}

		private void SaveToOutbox(IUnitOfWork uow, OrderEdoTask edoTask, object @event, string eventName)
		{
			var outboxMessage = new OutboxMessage(@event);

			LogPublishing(edoTask, eventName);

			uow.Save(outboxMessage);
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
				"Кладём в outbox событие {EventName} для задачи ЭДО {EdoTaskId} ({TaskType})",
				eventName,
				edoTask.Id,
				edoTask.GetType().Name);
		}
	}
}
