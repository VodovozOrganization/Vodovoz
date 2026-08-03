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
	/// Сервис для отправки сообщений в шину сообщений
	/// </summary>
	[Obsolete("Не используйте этот сервис. Разные сообщения отправляйте в отдельных сервисах " +
		"согласно их назначения, или в сервисах где создается документ соотвествующий сообщению")]
	public class MessageService
	{
		private readonly ILogger<MessageService> _logger;
		private readonly IBus _bus;

		public MessageService(ILogger<MessageService> logger, IBus bus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}

		/// <summary>
		/// Опубликовать событие о создании неформальной заявки по ЭДО
		/// </summary>
		/// <param name="informalRequestId"></param>
		/// <returns></returns>
		public async Task PublishInformalEdoRequestCreatedEvent(int informalRequestId)
		{
			_logger.LogInformation("Отправляем событие на создание новой заявки по ЭДО, запрос: {RequestId}.", informalRequestId);

			try
			{
				await _bus.Publish(new InformalEdoRequestCreatedEvent { InformalRequestId = informalRequestId });
				_logger.LogInformation("Событие на создание новой заявки по ЭДО отправлено успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при отправке события на создание новой заявки по ЭДО. Id запроса: {RequestId}. Exception: {ExceptionMessage}",
					informalRequestId,
					ex.Message);
			}
		}

		/// <summary>
		/// Опубликовать событие о создании эдо задачи для повторной проверки на новые ошибки
		/// </summary>
		/// <param name="edoTaskId">ID ЭДО задачи</param>
		/// <returns></returns>
		public async Task PublishSendDocumentTaskCreatedEvent(int edoTaskId)
		{
			_logger.LogInformation("Отправляем событие о создании эдо задачи для повторной проверки на новые ошибки: {RequestId}.", edoTaskId);

			try
			{
				await _bus.Publish(new DocumentTaskCreatedEvent { Id = edoTaskId });
				_logger.LogInformation("Событие о создании эдо задачи для повторной проверки на новые ошибки отправлено успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при отправке события о создании эдо задачи для повторной проверки на новые ошибки. Id запроса: {RequestId}. Exception: {ExceptionMessage}",
					edoTaskId,
					ex.Message);
			}
		}

		public async Task PublishResumeEvent(OrderEdoTask edoTask, CancellationToken cancellationToken = default)
		{
			switch(edoTask)
			{
				case DocumentEdoTask documentTask:
					await PublishDocumentResumeEvent(documentTask, cancellationToken);
					break;
				case TenderEdoTask tenderTask:
					await PublishTenderResumeEvent(tenderTask, cancellationToken);
					break;
				case ReceiptEdoTask receiptTask:
					await PublishReceiptResumeEvent(receiptTask, cancellationToken);
					break;
				default:
					throw new ArgumentOutOfRangeException(
						$"Задача ЭДО {edoTask.Id}: неизвестный тип задачи {edoTask.GetType().Name}, не удалось определить событие для возобновления");
			}
		}

		private async Task PublishDocumentResumeEvent(DocumentEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.Stage != DocumentEdoTaskStage.New)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} (DocumentEdoTask) находится на стадии {Stage}. Возобновление возможно только на стадии New",
					edoTask.Id,
					edoTask.Stage);
				return;
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId} (DocumentEdoTask) находится на стадии {Stage}. Публикуем событие {EventName}",
				edoTask.Id,
				edoTask.Stage,
				nameof(DocumentTaskCreatedEvent));

			await _bus.Publish(new DocumentTaskCreatedEvent { Id = edoTask.Id }, cancellationToken);
		}

		private async Task PublishTenderResumeEvent(TenderEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.Stage != TenderEdoTaskStage.New)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} (TenderEdoTask) находится на стадии {Stage}. Возобновление возможно только на стадии New",
					edoTask.Id,
					edoTask.Stage);
				return;
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId} (TenderEdoTask) находится на стадии {Stage}. Публикуем событие {EventName}",
				edoTask.Id,
				edoTask.Stage,
				nameof(TenderTaskCreatedEvent));

			await _bus.Publish(new TenderTaskCreatedEvent { TenderEdoTaskId = edoTask.Id }, cancellationToken);
		}

		private async Task PublishReceiptResumeEvent(ReceiptEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.ReceiptStatus != EdoReceiptStatus.New)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} (ReceiptEdoTask) находится в статусе {ReceiptStatus}. Возобновление возможно только в статусе New",
					edoTask.Id,
					edoTask.ReceiptStatus);
				return;
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId} (ReceiptEdoTask) находится в статусе {ReceiptStatus}. Публикуем событие {EventName}",
				edoTask.Id,
				edoTask.ReceiptStatus,
				nameof(ReceiptTaskCreatedEvent));

			await _bus.Publish(new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = edoTask.Id }, cancellationToken);
		}
	}
}
