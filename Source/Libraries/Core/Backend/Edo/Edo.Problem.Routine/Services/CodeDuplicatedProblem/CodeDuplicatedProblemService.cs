using Core.Infrastructure;
using Edo.Contracts.Messages.Events;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services.CodeDuplicatedProblem
{
	/// <summary>
	/// Сервис обработки проблем с дубликатом кода в ЭДО
	/// </summary>
	public class CodeDuplicatedProblemService
	{
		private const string _problemSourceName = "CodeDuplicatedException";

		private readonly ILogger<CodeDuplicatedProblemService> _logger;
		private readonly IEdoRepository _edoRepository;
		private readonly IEdoNotificationMessageFactory _edoNotificationMessageFactory;
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _edoNotificationPublisher;
		private readonly IBus _messageBus;

		public CodeDuplicatedProblemService(
			ILogger<CodeDuplicatedProblemService> logger,
			IEdoRepository edoRepository,
			IEdoNotificationMessageFactory edoNotificationMessageFactory,
			IOutboxNotificationPublisher<EdoNotificationMessage> edoNotificationPublisher,
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_edoNotificationMessageFactory = edoNotificationMessageFactory ?? throw new ArgumentNullException(nameof(edoNotificationMessageFactory));
			_edoNotificationPublisher = edoNotificationPublisher ?? throw new ArgumentNullException(nameof(edoNotificationPublisher));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		/// <summary>
		/// Обработчик задач с дубликатом кода в ЭДО
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="minEdoTaskCreationTime">Минимальное время создания задачи ЭДО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		public async Task ProcessProblemTasksAsync(IUnitOfWork unitOfWork, DateTime minEdoTaskCreationTime, CancellationToken cancellationToken)
		{
			var problemNodes = await _edoRepository.GetProblemEdoTasksForResume(
				unitOfWork,
				_problemSourceName,
				minEdoTaskCreationTime,
				ReasonForLeaving.Resale,
				cancellationToken);

			_logger.LogInformation("Найдено {Count} задач ЭДО с активной проблемой {ProblemName}",
				problemNodes.Count, _problemSourceName);

			foreach(var problemNode in problemNodes)
			{
				if(problemNode.RoutineState is null)
				{
					await ResumeTaskAsync(problemNode.EdoTask, cancellationToken);
				}
				else
				{
					await NotifyAsync(unitOfWork, problemNode, cancellationToken);
				}

				await SaveProblemRoutineStateAsync(unitOfWork, problemNode, cancellationToken);

				await unitOfWork.CommitAsync(cancellationToken);
			}
		}

		private async Task NotifyAsync(IUnitOfWork unitOfWork, EdoTaskProblemRoutineNode problemNode, CancellationToken cancellationToken)
		{
			var edoNotificationMessage = _edoNotificationMessageFactory.Create(
				EdoNotificationType.CodeDuplicated,
				("OrderId", problemNode.OrderId.ToString()),
				("EdoTaskId", problemNode.EdoTask.Id.ToString()),
				("EdoTaskType", problemNode.EdoTask.TaskType.GetEnumDisplayName()),
				("Description", problemNode.ProblemDescription),
				("Recommendation", problemNode.Recommendation),
				("ExceptionMessage", problemNode.ExceptionMessage)
			);

			await _edoNotificationPublisher.TryPublishAsync(unitOfWork, edoNotificationMessage, cancellationToken);
		}

		private async Task SaveProblemRoutineStateAsync(
			IUnitOfWork uow,
			EdoTaskProblemRoutineNode problemNode,
			CancellationToken cancellationToken)
		{
			var problem = problemNode.Problem;
			var edoTask = problemNode.EdoTask;
			var state = problemNode.RoutineState ?? new EdoTaskProblemRoutineState { Problem = problem };
			var now = DateTime.Now;

			state.RetryCount++;
			state.LastRetryTime = now;

			await uow.SaveAsync(state, cancellationToken: cancellationToken);
		}

		private async Task ResumeTaskAsync(OrderEdoTask edoTask, CancellationToken cancellationToken)
		{
			await PublishResumeEvent(edoTask, cancellationToken);
		}

		private async Task PublishResumeEvent(OrderEdoTask edoTask, CancellationToken cancellationToken)
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
					_logger.LogWarning(
						"Задача ЭДО {EdoTaskId}: неизвестный тип задачи {TaskType}, не удалось определить событие для возобновления",
						edoTask.Id, edoTask.GetType().Name);
					break;
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

			await _messageBus.Publish(new DocumentTaskCreatedEvent { Id = edoTask.Id }, cancellationToken);
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

			await _messageBus.Publish(new TenderTaskCreatedEvent { TenderEdoTaskId = edoTask.Id }, cancellationToken);
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

			await _messageBus.Publish(new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = edoTask.Id }, cancellationToken);
		}
	}
}
