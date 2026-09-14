using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Specifications.Edo;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Подбирает валидаторы отклонений, для типов которых
	/// в справочнике есть включенное описание
	/// </summary>
	public class EdoDeviationValidatorsProvider
	{
		private readonly ILogger<EdoDeviationValidatorsProvider> _logger;
		private readonly IGenericRepository<EdoDeviationSource> _deviationSourceRepository;
		private readonly IEnumerable<IEdoTaskDeviationValidator> _taskValidators;
		private readonly IEnumerable<IEdoRequestDeviationValidator> _requestValidators;
		private readonly IEnumerable<IEdoTransferDeviationValidator> _transferValidators;

		public EdoDeviationValidatorsProvider(
			ILogger<EdoDeviationValidatorsProvider> logger,
			IGenericRepository<EdoDeviationSource> deviationSourceRepository,
			IEnumerable<IEdoTaskDeviationValidator> taskValidators,
			IEnumerable<IEdoRequestDeviationValidator> requestValidators,
			IEnumerable<IEdoTransferDeviationValidator> transferValidators)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_deviationSourceRepository = deviationSourceRepository
				?? throw new ArgumentNullException(nameof(deviationSourceRepository));
			_taskValidators = taskValidators ?? throw new ArgumentNullException(nameof(taskValidators));
			_requestValidators = requestValidators ?? throw new ArgumentNullException(nameof(requestValidators));
			_transferValidators = transferValidators ?? throw new ArgumentNullException(nameof(transferValidators));
		}

		/// <summary>
		/// Возвращает валидаторы, для которых в справочнике есть включенное описание
		/// <see cref="EdoDeviationType"/> совпадает с порядком прохождения документооборота
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Валидаторы вместе с их описаниями из справочника</returns>
		public async Task<EdoDeviationValidators> GetValidatorsAsync(
			IUnitOfWork uow,
			CancellationToken cancellationToken)
		{
			var sourcesResult = await _deviationSourceRepository.GetAsync(
				uow,
				EdoDeviationSourceSpecification.CreateActive(),
				cancellationToken: cancellationToken);

			var sourcesByType = sourcesResult.Value
				.GroupBy(x => x.DeviationType)
				.ToDictionary(x => x.Key, x => x.First());

			return new EdoDeviationValidators(
				MatchWithSources(_taskValidators, sourcesByType),
				MatchWithSources(_requestValidators, sourcesByType),
				MatchWithSources(_transferValidators, sourcesByType));
		}

		private IReadOnlyList<EdoDeviationValidation<TValidator>> MatchWithSources<TValidator>(
			IEnumerable<TValidator> validators,
			IReadOnlyDictionary<EdoDeviationType, EdoDeviationSource> sourcesByType)
			where TValidator : class, IEdoDeviationValidator
		{
			var matched = new List<EdoDeviationValidation<TValidator>>();

			foreach(var validator in validators)
			{
				if(!sourcesByType.TryGetValue(validator.DeviationType, out var source))
				{
					_logger.LogDebug(
						"Валидатор отклонения {DeviationType} пропущен: в справочнике нет включенного описания",
						validator.DeviationType);

					continue;
				}

				matched.Add(new EdoDeviationValidation<TValidator>(validator, source));
			}

			return matched
				.OrderBy(x => x.Validator.DeviationType)
				.ToList();
		}
	}
}
