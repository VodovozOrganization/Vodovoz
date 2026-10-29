using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using MassTransit;
using TransactionalOutbox.MassTransit;

namespace Edo.Transport
{
	/// <summary>
	/// Кладёт в outbox транзакции UoW события запуска задач ЭДО заказа.
	/// Отправка в RabbitMQ выполняется после коммита транзакции с задачей службой отправки outbox MassTransit.
	/// </summary>
	public class OrderEdoTaskCreatedEventPublisher : IOrderEdoTaskCreatedEventPublisher
	{
		private readonly ILogger<OrderEdoTaskCreatedEventPublisher> _logger;
		// UoW передаётся параметром: класс вызывается и с десктопа, где у диалогов свои UoW.
		// GetPublishEndpoint(uow) пишет сообщение в таблицы outbox в транзакции этого UoW.
		private readonly IUnitOfWorkOutbox<IBus> _outbox;

		public OrderEdoTaskCreatedEventPublisher(
			ILogger<OrderEdoTaskCreatedEventPublisher> logger,
			IUnitOfWorkOutbox<IBus> outbox)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
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

		private void SaveToOutbox<TEvent>(IUnitOfWork uow, OrderEdoTask edoTask, TEvent @event, string eventName)
			where TEvent : class
		{
			LogPublishing(edoTask, eventName);

			SaveToOutbox(uow, @event);
		}

		/// <summary>
		/// Сохраняет событие в outbox транзакции <paramref name="uow"/>
		/// </summary>
		private void SaveToOutbox<TEvent>(IUnitOfWork uow, TEvent @event)
			where TEvent : class
		{
			_outbox.GetPublishEndpoint(uow)
				.Publish(@event)
				.GetAwaiter()
				.GetResult();
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
