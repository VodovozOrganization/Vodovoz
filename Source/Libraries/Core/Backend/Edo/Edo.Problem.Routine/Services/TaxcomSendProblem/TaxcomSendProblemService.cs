using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Exception.Sources;
using Edo.Transport;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.NotificationSettings;
using Vodovoz.Core.Domain.Rules.Edo;

namespace Edo.Problem.Routine.Services.TaxcomSendProblem
{
	public class TaxcomSendProblemService : ITaxcomSendProblemService
	{
		private const string _problemSourceName = "Custom.TaxcomDocumentSendingFail";
		private readonly ILogger<TaxcomSendProblemService> _logger;
		private readonly INotificationPublisher<EdoNotificationMessage> _notificationPublisher;
		private readonly IEdoNotificationMessageFactory _notificationMessageFactory;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEdoRepository _edoRepository;
		private readonly IPublishEndpoint _publishEndpoint;
		private readonly TaxcomSendProblemWorkerOptions _options;

		public TaxcomSendProblemService(
			ILogger<TaxcomSendProblemService> logger,
			INotificationPublisher<EdoNotificationMessage> notificationPublisher,
			IEdoNotificationMessageFactory notificationMessageFactory,
			IUnitOfWork unitOfWork,
			IEdoRepository edoRepository,
			IOptions<TaxcomSendProblemWorkerOptions> options,
			IPublishEndpoint publishEndpoint)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
			_notificationMessageFactory = notificationMessageFactory ?? throw new ArgumentNullException(nameof(notificationMessageFactory));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public async Task TryResumeOrderDocumentSendAsync(int orderDocumentId, CancellationToken cancellationToken)
		{			
			await _publishEndpoint.Publish(new OrderDocumentSendEvent { OrderDocumentId = orderDocumentId }, cancellationToken);
		}

		private async Task TryResumeDocumentSendAsync(OutgoingEdoDocument document, CancellationToken cancellationToken)
		{
			switch(document.Type)
			{
				case OutgoingEdoDocumentType.Order:
					await TryResumeOrderDocumentSendAsync(document.Id, cancellationToken);
					break;
				case OutgoingEdoDocumentType.Transfer:
					await _publishEndpoint.Publish(
						new TransferDocumentSendEvent { TransferDocumentId = document.Id }, cancellationToken);
					break;
				default:
					throw new NotSupportedException($"Не поддерживается повторная отправка документа типа {document.Type}");
			}
		}

		public async Task ProcessProblemTasks(CancellationToken cancellationToken)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				_logger.LogInformation("Начинаем поиск проблем ЭДО с ошибкой отправки в Такском");

				var problemNodes = await _edoRepository.GetTaxcomSendProblemNodes(
					_unitOfWork,
					_problemSourceName,
					_options.BatchSize,
					_options.RetryDelays,
					TransferEdoTaskCancellationRule.CancellationStatuses,
					cancellationToken);

				if(!problemNodes.Any())
				{
					_logger.LogDebug("Нет активных проблем ЭДО с ошибкой отправки для обработки");
					return;
				}

				_logger.LogInformation("Найдено {Count} активных проблем ЭДО с ошибкой отправки", problemNodes.Count);

				var processedCount = 0;
				var failedCount = 0;
				var notificationsToSend = new List<TaxcomSendProblemNotificationData>();

				foreach(var problemNode in problemNodes)
				{
					try
					{
						cancellationToken.ThrowIfCancellationRequested();

						var result = await ProcessProblem(_unitOfWork, problemNode, cancellationToken);

						if(result.Processed)
						{
							processedCount++;
						}

						if(result.ShouldNotify)
						{
							notificationsToSend.Add(result.NotificationData);
						}
					}
					catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
					{
						throw;
					}
					catch(Exception ex)
					{
						failedCount++;
						_logger.LogError(ex,
							"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: ошибка при обработке",
							problemNode.Problem.Id,
							problemNode.EdoTask?.Id ?? 0);
						throw;
					}
				}

				if(notificationsToSend.Any())
				{
					await SendGroupNotificationsAsync(_unitOfWork, notificationsToSend, cancellationToken);
				}

				// События возобновления сохраняются в outbox вместе с состоянием попыток и уходят после коммита
				await _unitOfWork.CommitAsync(cancellationToken);

				_logger.LogInformation(
					"Обработка проблем ЭДО с ошибкой отправки завершена: " +
					"Обработано {ProcessedCount}, Ошибок {FailedCount}, Уведомлений {NotifiedCount}",
					processedCount,
					failedCount,
					notificationsToSend.Count);
			}
			catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
			{
				_logger.LogInformation("Обработка проблем ЭДО с ошибкой отправки была отменена");
				throw;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникла непредвиденная ошибка в сервисе {ServiceName}",
					nameof(TaxcomSendProblemService));
				throw;
			}
		}

		private async Task<TaxcomSendProblemProcessResult> ProcessProblem(
			IUnitOfWork uow,
			TaxcomSendProblemNode problemNode,
			CancellationToken cancellationToken)
		{
			var problem = problemNode.Problem;
			var edoTask = problemNode.EdoTask;
			if(edoTask.TaskType == EdoTaskType.Transfer && TransferEdoTaskCancellationRule.IsCancelledOrCancelling(edoTask.Status))
			{
				return TaxcomSendProblemProcessResult.Empty;
			}

			var document = problemNode.EdoDocument;
			var state = problemNode.RoutineState ?? new EdoTaskProblemRoutineState { Problem = problem };
			var now = DateTime.Now;

			if(!TaxcomSendProblemProcessingPolicy.CanRetry(state, now, _options.WorkerInterval))
			{
				_logger.LogDebug(
					"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: повторная обработка уже запускалась {LastRetryTime}. Следующая попытка через {WorkerInterval}",
					problem.Id,
					edoTask?.Id ?? 0,
					state.LastRetryTime,
					_options.WorkerInterval);

				return TaxcomSendProblemProcessResult.Empty;
			}

			await TryResumeDocumentSendAsync(document, cancellationToken);

			state.RetryCount++;
			state.LastRetryTime = now;
			await uow.SaveAsync(state, cancellationToken: cancellationToken);

			if(TaxcomSendProblemProcessingPolicy.ShouldRequestNotification(state, _options.MaxAttempts))
			{
				return await PrepareNotificationData(uow, problem, edoTask, document.Id, state.RetryCount, cancellationToken);
			}

			return new TaxcomSendProblemProcessResult(true, false, null);
		}

		private async Task<TaxcomSendProblemProcessResult> PrepareNotificationData(
			IUnitOfWork uow,
			CustomEdoTaskProblem problem,
			EdoTask edoTask,
			int edoDocumentId,
			int retryCount,
			CancellationToken cancellationToken)
		{
			_logger.LogError(
				"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: достигнуто максимальное количество попыток ({MaxAttempts}), подготовка данных для уведомления",
				problem.Id,
				edoTask?.Id ?? 0,
				_options.MaxAttempts);

			string taskDescription;
			string mainDocumentId;
			if(edoTask is TransferEdoTask transferTask)
			{
				taskDescription = $"Трансфер №{transferTask.Id}, " +
					$"заказ перемещения №{transferTask.TransferOrderId?.ToString() ?? "не указан"}";
				mainDocumentId = edoDocumentId.ToString();
			}
			else
			{
				var orderTask = (OrderEdoTask)edoTask;
				var orderId = orderTask?.FormalEdoRequest?.Order?.Id ?? 0;
				taskDescription = $"Заказ №{orderId}";
				mainDocumentId = orderTask?.FormalEdoRequest?.Order?.Id.ToString() ?? "Неизвестный заказ";
			}
			var errorMessage = problem.CustomMessage ?? "Неизвестная ошибка отправки в Такском";

			var notificationData = new TaxcomSendProblemNotificationData(
				taskDescription: taskDescription,
				mainDocumentId: mainDocumentId,
				errorMessage: errorMessage,
				problemId: problem.Id,
				retryCount: retryCount);

			return new TaxcomSendProblemProcessResult(
				processed: false,
				shouldNotify: true,
				notificationData: notificationData);
		}

		private async Task SendGroupNotificationsAsync(
			IUnitOfWork uow,
			List<TaxcomSendProblemNotificationData> notifications,
			CancellationToken cancellationToken)
		{
			if(!notifications.Any())
			{
				return;
			}

			try
			{
				var messageBuilder = new StringBuilder();
				messageBuilder.AppendLine("Достигнуто максимальное количество попыток отправки документов в Такском:");
				messageBuilder.AppendLine();

				foreach(var notification in notifications)
				{
					messageBuilder.AppendLine($"- {notification.TaskDescription}, " +
						$"документ: {notification.MainDocumentId}, " +
						$"попыток: {notification.RetryCount}, " +
						$"ошибка: {notification.ErrorMessage}");
				}

				var message = messageBuilder.ToString();

				var notificationMessage = _notificationMessageFactory.Create<TaxcomSendProblemEdoNotificationSetting>(
					("Message", message));

				var published = await _notificationPublisher.TryPublishAsync(uow, notificationMessage, cancellationToken);

				if(published)
				{
					_logger.LogInformation(
						"Отправлено сводное уведомление по {ProblemCount} проблемам отправки в Такском",
						notifications.Count);
				}
				else
				{
					_logger.LogWarning("Не удалось отправить сводное уведомление по {ProblemCount} проблемам", notifications.Count);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке сводного уведомления");
				throw;
			}
		}
	}
}
