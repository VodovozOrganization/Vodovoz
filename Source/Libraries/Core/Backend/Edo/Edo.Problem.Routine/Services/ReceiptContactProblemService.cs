using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Problem.Routine.Options;
using Edo.Problems.Validation;
using Edo.Problems.Validation.Sources;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	/// <summary>
	/// Сервис обработки проблем с контактом для отправки чека.
	/// </summary>
	public class ReceiptContactProblemService : IReceiptContactProblemService
	{
		private readonly ILogger<ReceiptContactProblemService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptionsMonitor<ReceiptContactProblemWorkerOptions> _options;
		private readonly IEdoTaskValidator _receiptContactValidator;
		private readonly IEdoRepository _edoRepository;
		private readonly IReceiptEdoTaskResendService _resendService;
		private readonly IReceiptContactProblemNotificationService _notificationService;

		public ReceiptContactProblemService(
			ILogger<ReceiptContactProblemService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptionsMonitor<ReceiptContactProblemWorkerOptions> options,
			IEnumerable<IEdoTaskValidator> validators,
			IEdoRepository edoRepository,
			IReceiptEdoTaskResendService resendService,
			IReceiptContactProblemNotificationService notificationService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_receiptContactValidator = (validators ?? throw new ArgumentNullException(nameof(validators)))
				.OfType<ReceiptContactEdoValidator>()
				.SingleOrDefault()
				?? throw new InvalidOperationException(
					$"Валидатор {nameof(ReceiptContactEdoValidator)} не зарегистрирован");
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_resendService = resendService ?? throw new ArgumentNullException(nameof(resendService));
			_notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
		}

		private DateTime MinEdoTaskCreationTime => DateTime.Today - _options.CurrentValue.ProblemTimeout;
		private string ProblemSourceName => _receiptContactValidator.Name;

		/// <summary>
		/// Обработать активные проблемы с контактом чека.
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		public async Task ProcessContactProblems(CancellationToken cancellationToken)
		{
			IList<int> receiptTaskIds;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис обработки активных проблем с контактом чека"))
			{
				receiptTaskIds = await _edoRepository.GetProblemEdoTaskIds<ReceiptEdoTask>(
					uow,
					ProblemSourceName,
					MinEdoTaskCreationTime,
					cancellationToken);

				_logger.LogInformation(
					"Найдено {Count} задач ЭДО с активной проблемой {ProblemName}",
					receiptTaskIds.Count,
					ProblemSourceName);

				await ProcessContactProblems(uow, receiptTaskIds, cancellationToken);
			}
		}

		private async Task ProcessContactProblems(
			IUnitOfWork uow,
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

					var receiptTask = await _edoRepository.GetEdoTaskById<ReceiptEdoTask>(
						uow,
						receiptTaskId,
						cancellationToken);

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

			var state = await GetOrCreateState(uow, problem, cancellationToken);
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

			if(!_resendService.CanResend(receiptTask))
			{
				return ReceiptContactProblemProcessResult.Empty;
			}

			var notificationRequested = false;

			if(ReceiptContactProblemProcessingPolicy.ShouldRequestNotification(
				state,
				_options.CurrentValue.RetryAttemptsBeforeNotification))
			{
				notificationRequested = await _notificationService.TryNotifyAsync(
					uow,
					receiptTask,
					problem,
					state.RetryCount,
					cancellationToken);
			}

			state.RetryCount++;
			state.LastRetryTime = now;
			await uow.SaveAsync(state, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);

			await _resendService.PublishResendEventAsync(receiptTask, cancellationToken);

			_logger.LogInformation(
				"Опубликовано событие повторного запуска задачи ЭДО {EdoTaskId}. Попытка: {RetryCount}",
				receiptTask.Id,
				state.RetryCount);

			return new ReceiptContactProblemProcessResult(true, notificationRequested);
		}

		private async Task<EdoTaskProblemRoutineState> GetOrCreateState(
			IUnitOfWork uow,
			EdoTaskProblem problem,
			CancellationToken cancellationToken)
		{
			var state = await _edoRepository.GetEdoTaskProblemRoutineState(
				uow,
				problem.Id,
				cancellationToken);

			return state ?? new EdoTaskProblemRoutineState
			{
				Problem = problem
			};
		}

		private readonly struct ReceiptContactProblemProcessResult
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
