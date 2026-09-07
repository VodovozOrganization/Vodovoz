using Edo.DeviationMonitoring.Errors;
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
	/// <inheritdoc cref="IEdoDeviationRegistrationService"/>
	public class EdoDeviationRegistrationService : IEdoDeviationRegistrationService
	{
		private const string _uowTitle = "Регистрация отклонений документооборота ЭДО";

		private readonly ILogger<EdoDeviationRegistrationService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IEdoDeviationRepository _edoDeviationRepository;
		private readonly EdoDeviationValidatorsProvider _validatorsProvider;
		private readonly EdoDeviationMonitoringOptions _options;

		public EdoDeviationRegistrationService(
			ILogger<EdoDeviationRegistrationService> logger,
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
		public async Task<EdoDeviationRegistrationResult> RegisterAsync(CancellationToken cancellationToken)
		{
			var context = new DeviationRegistrationContext(await GetValidatorsAsync(cancellationToken), DateTime.Now);

			if(!context.Validators.TaskValidators.Any()
				&& !context.Validators.RequestValidators.Any()
				&& !context.Validators.TransferValidators.Any())
			{
				_logger.LogWarning(
					"В справочнике описаний отклонений нет ни одной включенной записи, проверять нечего");

				return context.Result;
			}

			await RegisterByRequestsAsync(context, cancellationToken);
			await RegisterByTasksAsync(context, cancellationToken);
			await RegisterByTransferTasksAsync(context, cancellationToken);
			await RegisterByFinishedDocflowsAsync(context, cancellationToken);

			_logger.LogInformation(
				"Проход регистрации отклонений ЭДО завершен. Проверено заявок {RequestsCount}, задач {TasksCount}, "
				+ "трансферов {TransferTasksCount}, "
				+ "задач с завершенным документооборотом {FinishedDocflowTasksCount}, "
				+ "зарегистрировано отклонений {RegisteredCount}",
				context.Result.CheckedRequestsCount,
				context.Result.CheckedTasksCount,
				context.Result.CheckedTransferTasksCount,
				context.Result.CheckedFinishedDocflowTasksCount,
				context.Result.RegisteredDeviationsCount);

			return context.Result;
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

		/// <summary>
		/// Обходит заявки без задач и без незакрытых отклонений
		/// </summary>
		private async Task RegisterByRequestsAsync(DeviationRegistrationContext context, CancellationToken cancellationToken)
		{
			if(!context.Validators.RequestValidators.Any())
			{
				return;
			}

			var createdBefore = context.CheckTime - context.Validators.RequestValidators.Min(x => x.Source.Timeout);

			await RegisterPagesAsync(
				context,
				context.Validators.RequestValidators,
				(uow, cursor, token) => _edoDeviationRepository.GetRequestsWithoutTaskAsync(
					uow, createdBefore, cursor, _options.BatchSize, token),
				node => node.RequestId,
				count => context.Result.CheckedRequestsCount += count,
				cancellationToken);
		}

		/// <summary>
		/// Обходит незавершенные задачи без незакрытых отклонений
		/// </summary>
		private async Task RegisterByTasksAsync(DeviationRegistrationContext context, CancellationToken cancellationToken)
		{
			await RegisterPagesAsync(
				context,
				context.Validators.TaskValidators,
				(uow, cursor, token) => _edoDeviationRepository.GetMonitoredTasksAsync(
					uow, cursor, _options.BatchSize, token),
				node => node.EdoTaskId,
				count => context.Result.CheckedTasksCount += count,
				cancellationToken);
		}

		/// <summary>
		/// Обходит незавершенные задачи трансфера без незакрытых отклонений.
		/// Трансфер проверяется отдельно от задач заказов: у него свои стадии,
		/// а его УПД идет тем же трактом, что и УПД заказа
		/// </summary>
		private async Task RegisterByTransferTasksAsync(DeviationRegistrationContext context, CancellationToken cancellationToken)
		{
			await RegisterPagesAsync(
				context,
				context.Validators.TransferValidators,
				(uow, cursor, token) => _edoDeviationRepository.GetMonitoredTransferTasksAsync(
					uow, cursor, _options.BatchSize, token),
				node => node.EdoTaskId,
				count => context.Result.CheckedTransferTasksCount += count,
				cancellationToken);
		}

		/// <summary>
		/// Обходит задачи с завершенным документооборотом.
		/// Отдельный проход нужен правилам по результату ГИС МТ: он приходит после того,
		/// как завершение документооборота уже перевело задачу в завершенный статус,
		/// поэтому в основную выборку такие задачи не попадают
		/// </summary>
		private async Task RegisterByFinishedDocflowsAsync(DeviationRegistrationContext context, CancellationToken cancellationToken)
		{
			var gisMtTrackedFrom = _options.GisMtTrackingStartDate;

			await RegisterPagesAsync(
				context,
				context.Validators.FinishedTaskValidators,
				(uow, cursor, token) => GetFinishedDocflowTasksAsync(uow, gisMtTrackedFrom, cursor, token),
				node => node.EdoTaskId,
				count => context.Result.CheckedFinishedDocflowTasksCount += count,
				cancellationToken);

			await RegisterPagesAsync(
				context,
				context.Validators.FinishedTransferValidators,
				(uow, cursor, token) => GetFinishedDocflowTransferTasksAsync(uow, gisMtTrackedFrom, cursor, token),
				node => node.EdoTaskId,
				count => context.Result.CheckedFinishedDocflowTasksCount += count,
				cancellationToken);
		}

		private async Task<IList<EdoTaskMonitoringNode>> GetFinishedDocflowTasksAsync(
			IUnitOfWork uow,
			DateTime gisMtTrackedFrom,
			int afterTaskId,
			CancellationToken cancellationToken)
		{
			var taskIds = await _edoDeviationRepository.GetTaskIdsWithFinishedDocflowAsync(
				uow, gisMtTrackedFrom, afterTaskId, _options.BatchSize, cancellationToken);

			if(!taskIds.Any())
			{
				return new List<EdoTaskMonitoringNode>();
			}

			return await _edoDeviationRepository.GetTaskNodesAsync(uow, taskIds.ToArray(), cancellationToken);
		}

		private async Task<IList<EdoTransferTaskMonitoringNode>> GetFinishedDocflowTransferTasksAsync(
			IUnitOfWork uow,
			DateTime gisMtTrackedFrom,
			int afterTaskId,
			CancellationToken cancellationToken)
		{
			var taskIds = await _edoDeviationRepository.GetTransferTaskIdsWithFinishedDocflowAsync(
				uow, gisMtTrackedFrom, afterTaskId, _options.BatchSize, cancellationToken);

			if(!taskIds.Any())
			{
				return new List<EdoTransferTaskMonitoringNode>();
			}

			return await _edoDeviationRepository.GetTransferTaskNodesAsync(
				uow, taskIds.ToArray(), cancellationToken);
		}

		/// <summary>
		/// Обходит выборку страницами и заводит отклонения по первому сработавшему валидатору
		/// </summary>
		/// <param name="context">Состояние текущего прохода</param>
		/// <param name="validations">Валидаторы с описаниями, по порядку прохождения документооборота</param>
		/// <param name="getPageAsync">Выборка очередной страницы по курсору</param>
		/// <param name="cursorSelector">Код записи, он же курсор выборки</param>
		/// <param name="countChecked">Учет проверенных записей в результате прохода</param>
		/// <param name="cancellationToken">Токен отмены</param>
		private async Task RegisterPagesAsync<TNode, TValidator>(
			DeviationRegistrationContext context,
			IReadOnlyList<EdoDeviationValidation<TValidator>> validations,
			Func<IUnitOfWork, int, CancellationToken, Task<IList<TNode>>> getPageAsync,
			Func<TNode, int> cursorSelector,
			Action<int> countChecked,
			CancellationToken cancellationToken)
			where TNode : class
			where TValidator : class, IEdoDeviationValidator<TNode>
		{
			if(!validations.Any())
			{
				return;
			}

			var afterId = 0;

			while(true)
			{
				cancellationToken.ThrowIfCancellationRequested();

				using(var uow = _uowFactory.CreateWithoutRoot(_uowTitle))
				{
					var page = await getPageAsync(uow, afterId, cancellationToken);

					if(!page.Any())
					{
						return;
					}

					countChecked(page.Count);

					foreach(var node in page)
					{
						var validationResult = Validate(node, validations, context.CheckTime, cursorSelector(node));
						var deviationError = validationResult.Errors.OfType<EdoDeviationError>().FirstOrDefault();

						if(deviationError is null)
						{
							continue;
						}

						await RegisterDeviationAsync(uow, deviationError.Deviation, context, cancellationToken);
					}

					await uow.CommitAsync(cancellationToken);

					afterId = page.Max(cursorSelector);
				}
			}
		}

		/// <summary>
		/// Прогоняет валидаторы по записи и возвращает первое сработавшее отклонение.
		/// Валидаторы упорядочены по ходу документооборота, поэтому первое сработавшее —
		/// это самая ранняя непройденная стадия. Сбой самого валидатора не должен ронять проход,
		/// поэтому проверка продолжается следующим валидатором
		/// Резервный валидатор пропускается, пока к записи применим хотя бы один частный:
		/// иначе он перехватывал бы частные условия с таймаутом больше своего
		/// </summary>
		/// <returns>
		/// Успешный результат, если ни один валидатор не сработал;
		/// иначе <see cref="EdoDeviationError"/> первого сработавшего валидатора
		/// </returns>
		private Result Validate<TNode, TValidator>(
			TNode node,
			IReadOnlyList<EdoDeviationValidation<TValidator>> validations,
			DateTime checkTime,
			int entityId)
			where TNode : class
			where TValidator : class, IEdoDeviationValidator<TNode>
		{
			var hasApplicableSpecificValidator =
				EdoDeviationFallbackPolicy.HasApplicableSpecificValidator(_logger, node, validations, entityId);

			foreach(var validation in validations)
			{
				if(validation.Validator.IsFallback && hasApplicableSpecificValidator)
				{
					continue;
				}

				try
				{
					var validationResult = validation.Validator.Validate(node, validation.Source, checkTime);

					if(validationResult.IsFailure)
					{
						return validationResult;
					}
				}
				catch(Exception ex)
				{
					_logger.LogError(ex,
						"Ошибка проверки валидатором {DeviationType} по записи {EntityId}",
						validation.Validator.DeviationType,
						entityId);
				}
			}

			return Result.Success();
		}

		/// <summary>
		/// Заводит новое отклонение
		/// </summary>
		private async Task RegisterDeviationAsync(
			IUnitOfWork uow,
			EdoDeviationValidationResult validationResult,
			DeviationRegistrationContext context,
			CancellationToken cancellationToken)
		{
			var deviation = new EdoTaskDeviation
			{
				DeviationSource = uow.Session.Load<EdoDeviationSource>(validationResult.DeviationSourceId),
				StageName = validationResult.StageName,
				StageStartTime = validationResult.StageStartTime,
				Threshold = validationResult.Threshold,
				Details = validationResult.Details,
				State = TaskProblemState.Active,
				DetectedTime = context.CheckTime
			};

			if(validationResult.EdoTaskId != null)
			{
				deviation.EdoTask = uow.Session.Load<EdoTask>(validationResult.EdoTaskId.Value);
			}

			if(validationResult.EdoRequestId != null)
			{
				deviation.EdoRequest = uow.Session.Load<FormalEdoRequest>(validationResult.EdoRequestId.Value);
			}

			await uow.SaveAsync(deviation, cancellationToken: cancellationToken);
			context.Result.RegisteredDeviationsCount++;
		}
	}
}
