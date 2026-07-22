using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	/// <summary>
	/// Сервис повторной обработки проблем с контактом чека.
	/// </summary>
	public class ReceiptContactProblemService
	{
		private const string ProblemSourceName = "Receipt.ContactValid";

		private readonly ILogger<ReceiptContactProblemService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptionsMonitor<ReceiptContactProblemWorkerOptions> _options;
		private readonly IEdoRepository _edoRepository;
		private readonly IBus _messageBus;
		private readonly IReceiptContactProblemNotificationService _notificationService;

		public ReceiptContactProblemService(
			ILogger<ReceiptContactProblemService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptionsMonitor<ReceiptContactProblemWorkerOptions> options,
			IEdoRepository edoRepository,
			IBus messageBus,
			IReceiptContactProblemNotificationService notificationService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
		}

		private DateTime MinEdoTaskCreationTime => DateTime.Today - _options.CurrentValue.ProblemTimeout;

		/// <summary>
		/// Обработать активные проблемы с контактом чека.
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		public async Task ProcessContactProblems(CancellationToken cancellationToken)
		{
			IList<int> receiptTaskIds;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(ReceiptContactProblemService)))
			{
				var receiptTasks = await _edoRepository.GetProblemEdoTasks<ReceiptEdoTask>(
					uow,
					ProblemSourceName,
					MinEdoTaskCreationTime,
					cancellationToken);

				_logger.LogInformation(
					"Найдено {Count} задач ЭДО с активной проблемой {ProblemName}",
					receiptTasks.Count,
					ProblemSourceName);

				receiptTaskIds = receiptTasks.Select(x => x.Id).ToList();
			}

			await ProcessContactProblems(receiptTaskIds, cancellationToken);
		}

		private async Task ProcessContactProblems(
			IEnumerable<int> receiptTaskIds,
			CancellationToken cancellationToken)
		{
			var retryCount = 0;
			var notificationCount = 0;
			var errors = new List<Exception>();

			foreach(var receiptTaskId in receiptTaskIds)
			{
				try
				{
					cancellationToken.ThrowIfCancellationRequested();

					using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(ReceiptContactProblemService)))
					{
						var receiptTask = await uow.Session.GetAsync<ReceiptEdoTask>(receiptTaskId, cancellationToken);

						if(receiptTask == null)
						{
							_logger.LogWarning("Задача ЭДО {EdoTaskId} не найдена", receiptTaskId);
							continue;
						}

						var result = await ProcessContactProblem(uow, receiptTask, cancellationToken);

						if(result.RetryPublished)
						{
							retryCount++;
						}

						if(result.NotificationRequested)
						{
							notificationCount++;
						}
					}
				}
				catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
				{
					throw;
				}
				catch(Exception ex)
				{
					_logger.LogError(
						ex,
						"Ошибка при обработке проблемы контакта по задаче ЭДО {EdoTaskId}",
						receiptTaskId);
					errors.Add(ex);
				}
			}

			_logger.LogInformation(
				"Обработка проблем контактов чеков завершена. Повторно запущено: {RetryCount}. Уведомлений: {NotificationCount}. Ошибок: {ErrorCount}",
				retryCount,
				notificationCount,
				errors.Count);

			if(errors.Any())
			{
				throw new AggregateException("Не удалось обработать часть проблем с контактом чека", errors);
			}
		}

		private async Task<ReceiptContactProblemProcessResult> ProcessContactProblem(
			IUnitOfWork uow,
			ReceiptEdoTask receiptTask,
			CancellationToken cancellationToken)
		{
			var problem = receiptTask.Problems.FirstOrDefault(x =>
				x.SourceName == ProblemSourceName
				&& x.State == TaskProblemState.Active);

			if(problem == null)
			{
				_logger.LogWarning(
					"У задачи ЭДО {EdoTaskId} не найдена активная проблема {ProblemName}",
					receiptTask.Id,
					ProblemSourceName);
				return ReceiptContactProblemProcessResult.Empty;
			}

			var state = GetOrCreateState(uow, problem);
			var now = DateTime.Now;

			if(!ReceiptContactProblemProcessingPolicy.CanRetry(
				state,
				now,
				_options.CurrentValue.WorkerInterval))
			{
				_logger.LogDebug(
					"Повторная обработка задачи ЭДО {EdoTaskId} уже запускалась {LastRetryTime}. Следующая попытка возможна через {WorkerInterval}",
					receiptTask.Id,
					state.LastRetryTime,
					_options.CurrentValue.WorkerInterval);

				return ReceiptContactProblemProcessResult.Empty;
			}

			var notificationRequested = false;

			if(ReceiptContactProblemProcessingPolicy.ShouldRequestNotification(
				state,
				_options.CurrentValue.RetryAttemptsBeforeNotification))
			{
				await _notificationService.NotifyAsync(
					receiptTask,
					problem,
					state.RetryCount,
					cancellationToken);

				notificationRequested = true;
			}

			if(receiptTask.ReceiptStatus != EdoReceiptStatus.New)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} находится в статусе чека {ReceiptStatus}. Повторная обработка возможна только в статусе New",
					receiptTask.Id,
					receiptTask.ReceiptStatus);

				return new ReceiptContactProblemProcessResult(false, notificationRequested);
			}

			await _messageBus.Publish(
				new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = receiptTask.Id },
				cancellationToken);

			state.RetryCount++;
			state.LastRetryTime = now;
			await uow.SaveAsync(state, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);

			_logger.LogInformation(
				"Опубликовано событие {EventName} для повторной обработки задачи ЭДО {EdoTaskId}. Попытка: {RetryCount}",
				nameof(ReceiptTaskCreatedEvent),
				receiptTask.Id,
				state.RetryCount);

			return new ReceiptContactProblemProcessResult(true, notificationRequested);
		}

		private EdoTaskProblemRoutineState GetOrCreateState(IUnitOfWork uow, EdoTaskProblem problem)
		{
			var state = uow.Session.Query<EdoTaskProblemRoutineState>()
				.FirstOrDefault(x => x.Problem.Id == problem.Id);

			return state ?? new EdoTaskProblemRoutineState
			{
				Problem = problem
			};
		}

		private struct ReceiptContactProblemProcessResult
		{
			public static ReceiptContactProblemProcessResult Empty => new ReceiptContactProblemProcessResult(false, false);

			public ReceiptContactProblemProcessResult(bool retryPublished, bool notificationRequested)
			{
				RetryPublished = retryPublished;
				NotificationRequested = notificationRequested;
			}

			public bool RetryPublished { get; }
			public bool NotificationRequested { get; }
		}
	}
}
