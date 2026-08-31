using Edo.Problem.Routine.Options;
using Edo.Problems.Exception.Sources;
using Edo.Transport;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
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

namespace Edo.Problem.Routine.Services.TaxcomSendProblem
{
	public class TaxcomSendProblemService : ITaxcomSendProblemService
	{
		private readonly string _problemSourceName;
		private readonly ILogger<TaxcomSendProblemService> _logger;
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _notificationPublisher;
		private readonly IEdoNotificationMessageFactory _notificationMessageFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEdoRepository _edoRepository;
		private readonly MessageService _messageService;
		private readonly TaxcomSendProblemWorkerOptions _options;

		public TaxcomSendProblemService(
			ILogger<TaxcomSendProblemService> logger,
			IOutboxNotificationPublisher<EdoNotificationMessage> notificationPublisher,
			IEdoNotificationMessageFactory notificationMessageFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEdoRepository edoRepository,
			MessageService messageService,
			IOptions<TaxcomSendProblemWorkerOptions> options)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
			_notificationMessageFactory = notificationMessageFactory ?? throw new ArgumentNullException(nameof(notificationMessageFactory));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));

			_problemSourceName = nameof(TaxcomSendDocumentProblemSource);
		}

		public async Task TryResumeTaskAsync(OrderEdoTask edoTask, CancellationToken cancellationToken)
		{
			await _messageService.PublishTaskCreatedEvent(edoTask, cancellationToken);
		}

		public async Task ProcessProblemTasks(CancellationToken cancellationToken)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
				{
					_logger.LogInformation("Начинаем поиск проблем ЭДО с ошибкой отправки в Такском");

					var problemNodes = await _edoRepository.GetTaxcomSendProblemNodes(
						uow,
						_problemSourceName,
						_options.BatchSize,
						_options.RetryDelays,
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

							var result = await ProcessProblem(uow, problemNode, cancellationToken);

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
						}
					}

					if(notificationsToSend.Any())
					{
						await SendGroupNotificationsAsync(uow, notificationsToSend, cancellationToken);
					}

					if(uow.HasChanges)
					{
						await uow.CommitAsync(cancellationToken);
					}

					_logger.LogInformation(
						"Обработка проблем ЭДО с ошибкой отправки завершена: " +
						"Обработано {ProcessedCount}, Ошибок {FailedCount}, Уведомлений {NotifiedCount}",
						processedCount,
						failedCount,
						notificationsToSend.Count);
				}
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

			state.RetryCount++;
			state.LastRetryTime = now;
			await uow.SaveAsync(state, cancellationToken: cancellationToken);

			try
			{
				await TryResumeTaskAsync(edoTask, cancellationToken);

				if(TaxcomSendProblemProcessingPolicy.ShouldRequestNotification(state, _options.MaxAttempts))
				{
					return await PrepareNotificationData(uow, problem, edoTask, state.RetryCount, cancellationToken);
				}

				return new TaxcomSendProblemProcessResult(true, false, null);
			}
			catch(Exception ex)
			{
				_logger.LogWarning(ex,
					"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: ошибка при обработке, попытка #{RetryCount}",
					problem.Id,
					edoTask?.Id ?? 0,
					state.RetryCount);

				return new TaxcomSendProblemProcessResult(false, false, null);
			}
		}

		private async Task<TaxcomSendProblemProcessResult> PrepareNotificationData(
			IUnitOfWork uow,
			ExceptionEdoTaskProblem problem,
			OrderEdoTask edoTask,
			int retryCount,
			CancellationToken cancellationToken)
		{
			_logger.LogError(
				"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: достигнуто максимальное количество попыток ({MaxAttempts}), подготовка данных для уведомления",
				problem.Id,
				edoTask?.Id ?? 0,
				_options.MaxAttempts);

			var orderId = edoTask?.FormalEdoRequest?.Order?.Id ?? 0;
			var mainDocumentId = edoTask?.FormalEdoRequest?.Order?.Id.ToString() ?? "Неизвестный заказ";
			var errorMessage = problem.ExceptionMessage ?? "Неизвестная ошибка отправки в Такском";

			var notificationData = new TaxcomSendProblemNotificationData(
				orderId: orderId,
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
					messageBuilder.AppendLine($"- Заказ №{notification.OrderId}, " +
						$"документ: {notification.MainDocumentId}, " +
						$"попыток: {notification.RetryCount}, " +
						$"ошибка: {notification.ErrorMessage}");
				}

				var message = messageBuilder.ToString();

				var notificationMessage = _notificationMessageFactory.Create(
					EdoNotificationType.TaxcomSendProblem,
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
