using Edo.DeviationMonitoring.Options;
using Edo.DeviationMonitoring.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;

namespace Edo.DeviationMonitoring
{
	/// <inheritdoc cref="IEdoDeviationResolvingService"/>
	public class EdoDeviationResolvingService : IEdoDeviationResolvingService
	{
		private const string _uowTitle = "Снятие отклонений документооборота ЭДО";

		private readonly ILogger<EdoDeviationResolvingService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IEdoDeviationRepository _edoDeviationRepository;
		private readonly EdoDeviationValidatorsProvider _validatorsProvider;
		private readonly EdoDeviationMonitoringOptions _options;

		public EdoDeviationResolvingService(
			ILogger<EdoDeviationResolvingService> logger,
			IUnitOfWorkFactory uowFactory,
			IEdoDeviationRepository edoDeviationRepository,
			EdoDeviationValidatorsProvider validatorsProvider,
			IOptionsSnapshot<EdoDeviationMonitoringOptions> options)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_edoDeviationRepository = edoDeviationRepository
				?? throw new ArgumentNullException(nameof(edoDeviationRepository));
			_validatorsProvider = validatorsProvider ?? throw new ArgumentNullException(nameof(validatorsProvider));

			if(options is null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			_options = options.Value;
		}

		/// <inheritdoc/>
		public async Task<EdoDeviationResolvingResult> ResolveAsync(CancellationToken cancellationToken)
		{
			var checkTime = DateTime.Now;
			var result = new EdoDeviationResolvingResult();
			var validators = await GetValidatorsAsync(cancellationToken);
			var afterDeviationId = 0;

			while(true)
			{
				cancellationToken.ThrowIfCancellationRequested();

				// каждая страница читается и фиксируется в своем UnitOfWork:
				// проход идет по всему журналу отклонений и не должен держать одну сессию
				using(var uow = _uowFactory.CreateWithoutRoot(_uowTitle))
				{
					var deviations = await _edoDeviationRepository.GetActiveDeviationsPageAsync(
						uow,
						afterDeviationId,
						_options.BatchSize,
						cancellationToken);

					if(!deviations.Any())
					{
						break;
					}

					result.CheckedDeviationsCount += deviations.Count;

					await ResolveIrrelevantAsync(
						uow, deviations.ToList(), validators, checkTime, result, cancellationToken);

					await uow.CommitAsync(cancellationToken);

					afterDeviationId = deviations.Max(x => x.Id);
				}
			}

			_logger.LogInformation(
				"Проход снятия отклонений ЭДО завершен. Проверено {CheckedCount}, снято {ResolvedCount}",
				result.CheckedDeviationsCount,
				result.ResolvedDeviationsCount);

			return result;
		}

		/// <summary>
		/// Читает справочник описаний отклонений: он один на весь проход
		/// </summary>
		private async Task<EdoDeviationValidators> GetValidatorsAsync(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot(_uowTitle))
			{
				return await _validatorsProvider.GetValidatorsAsync(uow, cancellationToken);
			}
		}

		private async Task ResolveIrrelevantAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<EdoTaskDeviation> deviations,
			EdoDeviationValidators validators,
			DateTime checkTime,
			EdoDeviationResolvingResult result,
			CancellationToken cancellationToken)
		{
			var taskIds = deviations
				.Where(x => x.EdoTask != null)
				.Select(x => x.EdoTask.Id)
				.Distinct()
				.ToArray();

			var requestIds = deviations
				.Where(x => x.EdoTask == null && x.EdoRequest != null)
				.Select(x => x.EdoRequest.Id)
				.Distinct()
				.ToArray();

			// коды задач читаются обеими выборками: задачи заказов и трансферов лежат
			// в одной таблице, и к какому семейству относится отклонение,
			// показывает то, в какой выборке нашлась задача
			var taskNodes = (await _edoDeviationRepository.GetTaskNodesAsync(uow, taskIds, cancellationToken))
				.GroupBy(x => x.EdoTaskId)
				.ToDictionary(x => x.Key, x => x.First());

			var transferNodes =
				(await _edoDeviationRepository.GetTransferTaskNodesAsync(uow, taskIds, cancellationToken))
				.GroupBy(x => x.EdoTaskId)
				.ToDictionary(x => x.Key, x => x.First());

			var requestNodes = (await _edoDeviationRepository.GetRequestNodesAsync(uow, requestIds, cancellationToken))
				.GroupBy(x => x.RequestId)
				.ToDictionary(x => x.Key, x => x.First());

			foreach(var deviation in deviations)
			{
				var resolveReason = GetResolveReason(
					deviation, validators, taskNodes, transferNodes, requestNodes, checkTime);

				if(resolveReason is null)
				{
					continue;
				}

				deviation.Resolve(checkTime, resolveReason.Value);

				await uow.SaveAsync(deviation, cancellationToken: cancellationToken);
				result.ResolvedDeviationsCount++;
			}
		}

		/// <summary>
		/// Определяет причину, по которой отклонение нужно снять.
		/// Возвращает <c>null</c>, если отклонение еще актуально
		/// </summary>
		private EdoDeviationResolveReason? GetResolveReason(
			EdoTaskDeviation deviation,
			EdoDeviationValidators validators,
			IReadOnlyDictionary<int, EdoTaskMonitoringNode> taskNodes,
			IReadOnlyDictionary<int, EdoTransferTaskMonitoringNode> transferNodes,
			IReadOnlyDictionary<int, EdoRequestMonitoringNode> requestNodes,
			DateTime checkTime)
		{
			if(deviation.EdoTask != null)
			{
				if(taskNodes.TryGetValue(deviation.EdoTask.Id, out var task))
				{
					return GetTaskDeviationResolveReason(deviation, validators, task, checkTime);
				}

				if(transferNodes.TryGetValue(deviation.EdoTask.Id, out var transferTask))
				{
					return GetTransferDeviationResolveReason(deviation, validators, transferTask, checkTime);
				}

				_logger.LogWarning(
					"Задача {EdoTaskId} по отклонению {DeviationId} не найдена, отклонение будет снято",
					deviation.EdoTask.Id,
					deviation.Id);

				return EdoDeviationResolveReason.EntityNotFound;
			}

			if(deviation.EdoRequest != null)
			{
				return GetRequestDeviationResolveReason(deviation, validators, requestNodes, checkTime);
			}

			_logger.LogWarning(
				"Отклонение {DeviationId} не связано ни с задачей, ни с заявкой и будет снято",
				deviation.Id);

			return EdoDeviationResolveReason.EntityNotFound;
		}

		private EdoDeviationResolveReason? GetTaskDeviationResolveReason(
			EdoTaskDeviation deviation,
			EdoDeviationValidators validators,
			EdoTaskMonitoringNode task,
			DateTime checkTime)
		{
			var validation = FindValidation(validators.TaskValidators, deviation);

			// завершение задачи снимает отклонение, кроме проверок,
			// которые как раз и выполняются по завершенным задачам: результат ГИС МТ
			// приходит после того, как документооборот уже завершил задачу
			if(task.IsFinished && !(validation?.Validator.IsAppliesToFinishedTask ?? false))
			{
				return EdoDeviationResolveReason.TaskFinished;
			}

			if(task.HasActiveProblem)
			{
				return EdoDeviationResolveReason.ProblemRegistered;
			}

			if(validation is null)
			{
				return EdoDeviationResolveReason.ValidatorDisabled;
			}

			var hasApplicableSpecificValidator =
				EdoDeviationFallbackPolicy.HasApplicableSpecificValidator(_logger, task, validators.TaskValidators, task.EdoTaskId);

			// резервное отклонение уступает место частному, как только стадия задачи определилась:
			// иначе оно висело бы до завершения задачи и не давало бы завести точное условие
			if(validation.Validator.IsFallback
				&& hasApplicableSpecificValidator)
			{
				return EdoDeviationResolveReason.ConditionGone;
			}

			var stillFires = StillFires(
				deviation,
				validation.Validator.DeviationType,
				() => validation.Validator.Validate(task, validation.Source, checkTime));

			return stillFires ? (EdoDeviationResolveReason?)null : EdoDeviationResolveReason.ConditionGone;
		}

		/// <summary>
		/// Отклонения по задаче трансфера снимаются по тем же правилам, что и по задачам заказов,
		/// но проблема ожидания перемещения кодов причиной снятия не считается:
		/// ровно ее длительность и меряет отклонение
		/// </summary>
		private EdoDeviationResolveReason? GetTransferDeviationResolveReason(
			EdoTaskDeviation deviation,
			EdoDeviationValidators validators,
			EdoTransferTaskMonitoringNode transferTask,
			DateTime checkTime)
		{
			var validation = FindValidation(validators.TransferValidators, deviation);

			if(transferTask.IsFinished && !(validation?.Validator.IsAppliesToFinishedTask ?? false))
			{
				return EdoDeviationResolveReason.TaskFinished;
			}

			if(transferTask.HasActiveProblem)
			{
				return EdoDeviationResolveReason.ProblemRegistered;
			}

			if(validation is null)
			{
				return EdoDeviationResolveReason.ValidatorDisabled;
			}

			var hasApplicableSpecificValidator =
				EdoDeviationFallbackPolicy.HasApplicableSpecificValidator(_logger, transferTask, validators.TransferValidators, transferTask.EdoTaskId);

			// резервное отклонение уступает место частному, как только стадия трансфера определилась
			if(validation.Validator.IsFallback
				&& hasApplicableSpecificValidator)
			{
				return EdoDeviationResolveReason.ConditionGone;
			}

			var stillFires = StillFires(
				deviation,
				validation.Validator.DeviationType,
				() => validation.Validator.Validate(transferTask, validation.Source, checkTime));

			return stillFires ? (EdoDeviationResolveReason?)null : EdoDeviationResolveReason.ConditionGone;
		}

		private EdoDeviationResolveReason? GetRequestDeviationResolveReason(
			EdoTaskDeviation deviation,
			EdoDeviationValidators validators,
			IReadOnlyDictionary<int, EdoRequestMonitoringNode> requestNodes,
			DateTime checkTime)
		{
			if(!requestNodes.TryGetValue(deviation.EdoRequest.Id, out var request))
			{
				_logger.LogWarning(
					"Заявка {EdoRequestId} по отклонению {DeviationId} не найдена, отклонение будет снято",
					deviation.EdoRequest.Id,
					deviation.Id);

				return EdoDeviationResolveReason.EntityNotFound;
			}

			if(request.HasTask)
			{
				return EdoDeviationResolveReason.TaskCreated;
			}

			var validation = FindValidation(validators.RequestValidators, deviation);

			if(validation is null)
			{
				return EdoDeviationResolveReason.ValidatorDisabled;
			}

			var stillFires = StillFires(
				deviation,
				validation.Validator.DeviationType,
				() => validation.Validator.Validate(request, validation.Source, checkTime));

			return stillFires ? (EdoDeviationResolveReason?)null : EdoDeviationResolveReason.ConditionGone;
		}

		/// <summary>
		/// Ищет валидатор, которым отклонение было зарегистрировано.
		/// Сопоставление идет по коду записи справочника, а не по типу отклонения:
		/// чтение кода не поднимает саму запись из базы.
		/// Отсутствие означает, что запись выключили или удалили из справочника
		/// </summary>
		private static EdoDeviationValidation<TValidator> FindValidation<TValidator>(
			IReadOnlyList<EdoDeviationValidation<TValidator>> validations,
			EdoTaskDeviation deviation)
			where TValidator : class, IEdoDeviationValidator =>
			validations.FirstOrDefault(x => x.Source.Id == deviation.DeviationSource?.Id);

		private bool StillFires(
			EdoTaskDeviation deviation,
			EdoDeviationType deviationType,
			Func<Result> validate)
		{
			try
			{
				return validate().IsFailure;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка проверки актуальности отклонения {DeviationId} типа {DeviationType}",
					deviation.Id,
					deviationType);

				return true;
			}
		}
	}
}
