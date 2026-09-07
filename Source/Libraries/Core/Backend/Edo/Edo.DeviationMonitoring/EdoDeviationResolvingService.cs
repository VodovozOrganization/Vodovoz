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

				using(var uow = _uowFactory.CreateWithoutRoot(_uowTitle))
				{
					var deviationNodes = await _edoDeviationRepository.GetActiveDeviationsPageAsync(
						uow,
						afterDeviationId,
						_options.BatchSize,
						cancellationToken);

					if(!deviationNodes.Any())
					{
						break;
					}

					result.CheckedDeviationsCount += deviationNodes.Count;

					await ResolveIrrelevantAsync(
						uow, deviationNodes.ToList(), validators, checkTime, result, cancellationToken);

					await uow.CommitAsync(cancellationToken);

					afterDeviationId = deviationNodes.Max(x => x.Deviation.Id);
				}
			}

			_logger.LogInformation(
				"Проход снятия отклонений ЭДО завершен. Проверено {CheckedCount}, снято {ResolvedCount}",
				result.CheckedDeviationsCount,
				result.ResolvedDeviationsCount);

			return result;
		}

		private async Task<EdoDeviationValidators> GetValidatorsAsync(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot(_uowTitle))
			{
				return await _validatorsProvider.GetValidatorsAsync(uow, cancellationToken);
			}
		}

		private async Task ResolveIrrelevantAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<EdoTaskDeviationNode> deviationNodes,
			EdoDeviationValidators validators,
			DateTime checkTime,
			EdoDeviationResolvingResult result,
			CancellationToken cancellationToken)
		{
			var taskIds = deviationNodes
				.Where(x => x.EdoTaskId.HasValue)
				.Select(x => x.EdoTaskId.Value)
				.Distinct()
				.ToArray();

			var requestIds = deviationNodes
				.Where(x => !x.EdoTaskId.HasValue && x.EdoRequestId.HasValue)
				.Select(x => x.EdoRequestId.Value)
				.Distinct()
				.ToArray();

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

			foreach(var deviationNode in deviationNodes)
			{
				var resolveReason = GetResolveReason(
					deviationNode, validators, taskNodes, transferNodes, requestNodes, checkTime);

				if(resolveReason is null)
				{
					continue;
				}

				var deviation = deviationNode.Deviation;

				deviation.Resolve(checkTime, resolveReason.Value);

				await uow.SaveAsync(deviation, cancellationToken: cancellationToken);
				result.ResolvedDeviationsCount++;
			}
		}

		private EdoDeviationResolveReason? GetResolveReason(
			EdoTaskDeviationNode deviationNode,
			EdoDeviationValidators validators,
			IReadOnlyDictionary<int, EdoTaskMonitoringNode> taskNodes,
			IReadOnlyDictionary<int, EdoTransferTaskMonitoringNode> transferNodes,
			IReadOnlyDictionary<int, EdoRequestMonitoringNode> requestNodes,
			DateTime checkTime)
		{
			if(deviationNode.EdoTaskId.HasValue)
			{
				if(taskNodes.TryGetValue(deviationNode.EdoTaskId.Value, out var task))
				{
					return GetTaskDeviationResolveReason(deviationNode, validators, task, checkTime);
				}

				if(transferNodes.TryGetValue(deviationNode.EdoTaskId.Value, out var transferTask))
				{
					return GetTransferDeviationResolveReason(deviationNode, validators, transferTask, checkTime);
				}

				_logger.LogWarning(
					"Задача {EdoTaskId} по отклонению {DeviationId} не найдена, отклонение будет снято",
					deviationNode.EdoTaskId.Value,
					deviationNode.Deviation.Id);

				return EdoDeviationResolveReason.EntityNotFound;
			}

			if(deviationNode.EdoRequestId.HasValue)
			{
				return GetRequestDeviationResolveReason(deviationNode, validators, requestNodes, checkTime);
			}

			_logger.LogWarning(
				"Отклонение {DeviationId} не связано ни с задачей, ни с заявкой и будет снято",
				deviationNode.Deviation.Id);

			return EdoDeviationResolveReason.EntityNotFound;
		}

		private EdoDeviationResolveReason? GetTaskDeviationResolveReason(
			EdoTaskDeviationNode deviationNode,
			EdoDeviationValidators validators,
			EdoTaskMonitoringNode task,
			DateTime checkTime)
		{
			var validation = FindValidation(validators.TaskValidators, deviationNode);

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

			if(validation.Validator.IsFallback
				&& hasApplicableSpecificValidator)
			{
				return EdoDeviationResolveReason.ConditionGone;
			}

			var isStillActive = IsStillActive(
				deviationNode,
				validation.Validator.DeviationType,
				() => validation.Validator.Validate(task, validation.Source, checkTime));

			return isStillActive ? (EdoDeviationResolveReason?)null : EdoDeviationResolveReason.ConditionGone;
		}

		private EdoDeviationResolveReason? GetTransferDeviationResolveReason(
			EdoTaskDeviationNode deviationNode,
			EdoDeviationValidators validators,
			EdoTransferTaskMonitoringNode transferTask,
			DateTime checkTime)
		{
			var validation = FindValidation(validators.TransferValidators, deviationNode);

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

			if(validation.Validator.IsFallback
				&& hasApplicableSpecificValidator)
			{
				return EdoDeviationResolveReason.ConditionGone;
			}

			var isStillActive = IsStillActive(
				deviationNode,
				validation.Validator.DeviationType,
				() => validation.Validator.Validate(transferTask, validation.Source, checkTime));

			return isStillActive ? (EdoDeviationResolveReason?)null : EdoDeviationResolveReason.ConditionGone;
		}

		private EdoDeviationResolveReason? GetRequestDeviationResolveReason(
			EdoTaskDeviationNode deviationNode,
			EdoDeviationValidators validators,
			IReadOnlyDictionary<int, EdoRequestMonitoringNode> requestNodes,
			DateTime checkTime)
		{
			if(!requestNodes.TryGetValue(deviationNode.EdoRequestId.Value, out var request))
			{
				_logger.LogWarning(
					"Заявка {EdoRequestId} по отклонению {DeviationId} не найдена, отклонение будет снято",
					deviationNode.EdoRequestId.Value,
					deviationNode.Deviation.Id);

				return EdoDeviationResolveReason.EntityNotFound;
			}

			if(request.HasTask)
			{
				return EdoDeviationResolveReason.TaskCreated;
			}

			var validation = FindValidation(validators.RequestValidators, deviationNode);

			if(validation is null)
			{
				return EdoDeviationResolveReason.ValidatorDisabled;
			}

			var isStillActive = IsStillActive(
				deviationNode,
				validation.Validator.DeviationType,
				() => validation.Validator.Validate(request, validation.Source, checkTime));

			return isStillActive ? (EdoDeviationResolveReason?)null : EdoDeviationResolveReason.ConditionGone;
		}

		private static EdoDeviationValidation<TValidator> FindValidation<TValidator>(
			IReadOnlyList<EdoDeviationValidation<TValidator>> validations,
			EdoTaskDeviationNode deviationNode)
			where TValidator : class, IEdoDeviationValidator =>
			validations.FirstOrDefault(x => x.Source.Id == deviationNode.DeviationSourceId);

		private bool IsStillActive(
			EdoTaskDeviationNode deviationNode,
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
					deviationNode.Deviation.Id,
					deviationType);

				return true;
			}
		}
	}
}
