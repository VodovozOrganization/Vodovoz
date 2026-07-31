using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Data.Repositories.Goods;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services.CodePoolMissingProblem
{
	public class CodePoolMissingProblemService : ICodePoolMissingProblemService
	{
		private readonly string _problemSourceName;
		private readonly ILogger<CodePoolMissingProblemService> _logger;
		private readonly IBus _bus;
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _notificationPublisher;
		private readonly IEdoNotificationMessageFactory _notificationMessageFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEdoRepository _edoRepository;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly CodePoolMissingProblemWorkerOptions _options;

		public CodePoolMissingProblemService(
			ILogger<CodePoolMissingProblemService> logger,
			IBus messageBus,
			IOutboxNotificationPublisher<EdoNotificationMessage> notificationPublisher,
			IEdoNotificationMessageFactory notificationMessageFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEdoRepository edoRepository,
			INomenclatureRepository nomenclatureRepository,
			IOptions<CodePoolMissingProblemWorkerOptions> options
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_bus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
			_notificationMessageFactory = notificationMessageFactory ?? throw new ArgumentNullException(nameof(notificationMessageFactory));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));

			_problemSourceName = nameof(EdoCodePoolMissingCodeException);
		}

		public async Task TryResumeTaskAsync(OrderEdoTask edoTask, CancellationToken cancellationToken)
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

				throw new ArgumentException(
					$"Задача ЭДО {edoTask.Id} (DocumentEdoTask) находится на стадии {edoTask.Stage}. Возобновление возможно только на стадии New");
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

				throw new ArgumentException(
					$"Задача ЭДО {edoTask.Id} (TenderEdoTask) находится на стадии {edoTask.Stage}. Возобновление возможно только на стадии New");
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

				throw new ArgumentException(
					$"Задача ЭДО {edoTask.Id} (ReceiptEdoTask) находится в статусе {edoTask.ReceiptStatus}. Возобновление возможно только на стадии New");
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId} (ReceiptEdoTask) находится в статусе {ReceiptStatus}. Публикуем событие {EventName}",
				edoTask.Id,
				edoTask.ReceiptStatus,
				nameof(ReceiptTaskCreatedEvent));

			await _bus.Publish(new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = edoTask.Id }, cancellationToken);
		}

		public async Task ProcessProblemTasks(CancellationToken cancellationToken)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
				{
					_logger.LogInformation("Начинаем поиск проблем ЭДО с ошибкой нехватки кодов в пуле");

					var problemNodes = await _edoRepository.GetCodePoolMissingProblemNodes(
						uow,
						_problemSourceName,
						_options.MaxAttempts,
						_options.BatchSize,
						cancellationToken);

					if(!problemNodes.Any())
					{
						_logger.LogDebug("Нет активных проблем ЭДО с ошибкой нехватки кодов для обработки");
						return;
					}

					_logger.LogInformation("Найдено {Count} активных проблем ЭДО с ошибкой нехватки кодов", problemNodes.Count);

					var processedCount = 0;
					var failedCount = 0;
					var notifiedCount = 0;

					foreach(var problemNode in problemNodes)
					{
						try
						{
							cancellationToken.ThrowIfCancellationRequested();

							var result = await ProcessProblem(uow, problemNode, cancellationToken);

							if(result.NotificationSent)
							{
								notifiedCount++;
							}

							if(result.Processed)
							{
								processedCount++;
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

					_logger.LogInformation(
						"Обработка проблем ЭДО с ошибкой нехватки кодов завершена: " +
						"Обработано {ProcessedCount}, Ошибок {FailedCount}, Уведомлений {NotifiedCount}",
						processedCount,
						failedCount,
						notifiedCount);
				}
			}
			catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
			{
				_logger.LogInformation("Обработка проблем ЭДО с ошибкой нехватки кодов была отменена");
				throw;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникла непредвиденная ошибка в сервисе {ServiceName}",
					nameof(CodePoolMissingProblemService));
				throw;
			}
		}

		private async Task<CodePoolMissingProblemProcessResult> ProcessProblem(
			IUnitOfWork uow,
			CodePoolMissingProblemNode problemNode,
			CancellationToken cancellationToken)
		{
			var problem = problemNode.Problem;
			var edoTask = problemNode.EdoTask;
			var state = problemNode.RoutineState ?? new EdoTaskProblemRoutineState { Problem = problem };
			var now = DateTime.Now;

			/*if(!CodePoolMissingProblemProcessingPolicy.CanRetry(
				state,
				now,
				_options.WorkerInterval))
			{
				_logger.LogDebug(
					"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: повторная обработка уже запускалась {LastRetryTime}. Следующая попытка через {WorkerInterval}",
					problem.Id,
					edoTask?.Id ?? 0,
					state.LastRetryTime,
					_options.WorkerInterval);

				return CodePoolMissingProblemProcessResult.Empty;
			}*/

			if(CodePoolMissingProblemProcessingPolicy.ShouldRequestNotification(
				state,
				_options.MaxAttempts))
			{
				return await SendNotification(uow, problem, edoTask, cancellationToken);
			}

			state.RetryCount++;
			state.LastRetryTime = now;
			await uow.SaveAsync(state, cancellationToken: cancellationToken);

			try
			{
				await TryResumeTaskAsync(edoTask, cancellationToken);

				await uow.CommitAsync(cancellationToken);

				return new CodePoolMissingProblemProcessResult(true, false);
			}
			catch(Exception ex)
			{
				_logger.LogWarning(ex,
					"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: ошибка при обработке, попытка #{RetryCount}",
					problem.Id,
					edoTask?.Id ?? 0,
					state.RetryCount);

				return new CodePoolMissingProblemProcessResult(false, false);
			}
		}

		private async Task<CodePoolMissingProblemProcessResult> SendNotification(
			IUnitOfWork uow,
			ExceptionEdoTaskProblem problem,
			OrderEdoTask edoTask,
			CancellationToken cancellationToken)
		{
			_logger.LogError(
				"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: достигнуто максимальное количество попыток ({MaxAttempts}), отправляем уведомление",
				problem.Id,
				edoTask?.Id ?? 0,
				_options.MaxAttempts);

			var orderId = edoTask?.FormalEdoRequest?.Order?.Id ?? 0;
			var notificationSent = await TryNotifyAsync(
				uow,
				orderId,
				problem,
				cancellationToken);

			if(notificationSent)
			{
				_logger.LogInformation(
					"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: уведомление отправлено успешно",
					problem.Id,
					edoTask?.Id ?? 0);
			}
			else
			{
				_logger.LogWarning(
					"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: не удалось отправить уведомление",
					problem.Id,
					edoTask?.Id ?? 0);
			}

			await uow.CommitAsync(cancellationToken);

			return new CodePoolMissingProblemProcessResult(false, notificationSent);
		}

		public async Task<bool> TryNotifyAsync(
			IUnitOfWork uow,
			int orderId,
			ExceptionEdoTaskProblem problem,
			CancellationToken cancellationToken)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(problem is null)
			{
				throw new ArgumentNullException(nameof(problem));
			}

			var (gtin, nomenclatureName) = await GetGtinAndNomenclature(uow, problem, cancellationToken);

			var notification = _notificationMessageFactory.Create(
				EdoNotificationType.CodePoolMissingProblem,
				("OrderId", orderId.ToString()),
				("Gtin", gtin ?? "не указан"),
				("NomenclatureName", nomenclatureName ?? "не указана"));

			return await _notificationPublisher.TryPublishAsync(uow, notification, cancellationToken);
		}

		private async Task<(string Gtin, string NomenclatureName)> GetGtinAndNomenclature(
			IUnitOfWork uow,
			ExceptionEdoTaskProblem problem,
			CancellationToken cancellationToken)
		{
			try
			{
				string gtin = null;
				string nomenclatureName = null;

				if(problem.CustomItems?.Any() == true)
				{
					var gtinItem = problem.CustomItems
						.OfType<EdoProblemGtinItem>()
						.FirstOrDefault();

					if(gtinItem?.Gtin != null)
					{
						gtin = gtinItem.Gtin.GtinNumber;
						_logger.LogDebug(
							"Проблема {ProblemId}: найден GTIN в CustomItems: {Gtin}",
							problem.Id,
							gtin);
					}
				}

				if(string.IsNullOrEmpty(gtin))
				{
					_logger.LogWarning(
						"Проблема {ProblemId}: не удалось получить GTIN",
						problem.Id);
					return (null, null);
				}

				var nomenclature = await _nomenclatureRepository.GetNomenclatureByGtinAsync(uow, gtin, cancellationToken);

				if(nomenclature != null)
				{
					nomenclatureName = nomenclature.Name;
					_logger.LogDebug(
						"Проблема {ProblemId}: найдена номенклатура {Nomenclature} по GTIN {Gtin}",
						problem.Id,
						nomenclatureName,
						gtin);
				}
				else
				{
					_logger.LogWarning(
						"Проблема {ProblemId}: номенклатура не найдена по GTIN {Gtin}",
						problem.Id,
						gtin);
				}

				return (gtin, nomenclatureName);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Проблема {ProblemId}: ошибка при получении GTIN и номенклатуры",
					problem.Id);
				return (null, null);
			}
		}
	}

	public readonly struct CodePoolMissingProblemProcessResult
	{
		public static CodePoolMissingProblemProcessResult Empty => new CodePoolMissingProblemProcessResult(false, false);

		public CodePoolMissingProblemProcessResult(bool processed, bool notificationSent)
		{
			Processed = processed;
			NotificationSent = notificationSent;
		}

		public bool Processed { get; }
		public bool NotificationSent { get; }
	}
}
