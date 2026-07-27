using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Errors;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Settings.Edo;

namespace Edo.Common
{
	public class TrueMarkCodesPoolCodeProvider : ITrueMarkCodesPoolCodeProvider
	{
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkCodesValidator _trueMarkCodesValidator;
		private readonly IEdoSettings _edoSettings;
		private readonly ILogger<TrueMarkCodesPoolCodeProvider> _logger;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;

		public TrueMarkCodesPoolCodeProvider(
			IUnitOfWork uow,
			ITrueMarkCodesValidator trueMarkCodesValidator,
			IEdoSettings edoSettings,
			ILogger<TrueMarkCodesPoolCodeProvider> logger,
			ITrueMarkCodeRepository trueMarkCodeRepository
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkCodesValidator = trueMarkCodesValidator ?? throw new ArgumentNullException(nameof(trueMarkCodesValidator));
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
		}

		public async Task<TrueMarkWaterIdentificationCode> TakeValidCodeAsync(
			ITrueMarkCodesPool codesPool,
			GtinEntity gtin,
			string organizationInn,
			CancellationToken cancellationToken)
		{
			if(codesPool is null)
			{
				throw new ArgumentNullException(nameof(codesPool));
			}

			if(gtin is null)
			{
				throw new ArgumentNullException(nameof(gtin));
			}

			return await TakeValidCodeAsync(codesPool, new[] { gtin }, organizationInn, cancellationToken);
		}

		public async Task<TrueMarkWaterIdentificationCode> TakeValidCodeAsync(
			ITrueMarkCodesPool codesPool,
			IEnumerable<GtinEntity> gtins,
			string organizationInn,
			CancellationToken cancellationToken)
		{
			if(codesPool is null)
			{
				throw new ArgumentNullException(nameof(codesPool));
			}

			if(gtins is null)
			{
				throw new ArgumentNullException(nameof(gtins));
			}

			var takeValidCodeAttempts = _edoSettings.CodePoolTakeValidCodeAttempts;
			var checkedGtins = new List<string>();

			_logger.LogInformation(
				"Начат подбор валидного кода из пула. Организация: {OrganizationInn}.",
				organizationInn);

			foreach(var gtin in gtins)
			{
				checkedGtins.Add(gtin.GtinNumber);

				for(var attempt = 1; attempt <= takeValidCodeAttempts; attempt++)
				{
					int codeId;

					try
					{
						codeId = await codesPool.TakeCode(gtin.GtinNumber, cancellationToken);
					}
					catch(EdoCodePoolMissingCodeException)
					{
						_logger.LogInformation("В пуле не найден код для GTIN {Gtin}.",
							gtin.GtinNumber);

						break;
					}

					_logger.LogInformation(
						"Из пула получен код ЧЗ Id {CodeId} для GTIN {Gtin}. Выполняется проверка валидности.",
						codeId,
						gtin.GtinNumber);

					var code = await _uow.Session.GetAsync<TrueMarkWaterIdentificationCode>(codeId, cancellationToken) 
						?? throw new InvalidOperationException($"Не найден код ЧЗ с Id {codeId}, полученный из пула.");

					var validationResult = await ValidateAsync(code, organizationInn, cancellationToken);

					if(validationResult.IsValid)
					{
						_logger.LogInformation(
							"Код ЧЗ Id {CodeId} из пула прошел проверку валидности.",
							code.Id);

						return code;
					}

					_logger.LogWarning(
						"Код ЧЗ Id {CodeId} из пула не прошел проверку валидности. " +
						"GTIN наш: {IsOurGtin}; владелец наша организация: {IsOwnedByOurOrganization}; " +
						"код в обороте: {IsIntroduced}; просрочен: {IsExpired}; попытка: {Attempt}/{AttemptsLimit}.",
						code.Id,
						validationResult.IsOurGtin,
						validationResult.IsOwnedByOurOrganization,
						validationResult.IsIntroduced,
						validationResult.IsExpired,
						attempt,
						takeValidCodeAttempts);
				}

				_logger.LogWarning(
					"В пуле не найден валидный код для GTIN {Gtin} за {AttemptsLimit} попыток.",
					gtin.GtinNumber,
					takeValidCodeAttempts);
			}

			var gtinsDescription = string.Join(", ", checkedGtins);

			_logger.LogWarning(
				"В пуле не найден валидный код из коллекции GTIN'ов: {Gtins}. Организация: {OrganizationInn}.",
				gtinsDescription,
				organizationInn);

			throw new EdoCodePoolMissingCodeException(
				$"В пуле не найден валидный код для следующих GTIN: {gtinsDescription}.");
		}

		public async Task<Result<IDictionary<string, IList<TrueMarkWaterIdentificationCode>>>> TakeValidCodesBatchAsync(
			ITrueMarkCodesPool codesPool,
			IDictionary<string, int> gtinCounts,
			string organizationInn,
			CancellationToken cancellationToken)
		{
			if(codesPool is null)
			{
				throw new ArgumentNullException(nameof(codesPool));
			}

			if(gtinCounts is null || !gtinCounts.Any())
			{
				throw new ArgumentNullException(nameof(gtinCounts));
			}

			var takeValidCodeAttempts = _edoSettings.CodePoolTakeValidCodeAttempts;
			var allCollectedCodes = InitializeCollectedCodes(gtinCounts);
			var allTakenCodes = new Dictionary<string, List<TrueMarkWaterIdentificationCode>>();

			_logger.LogInformation(
				"Начат пакетный подбор {TotalCount} кодов из пула для {GtinCount} GTIN. Организация: {OrganizationInn}.",
				gtinCounts.Values.Sum(),
				gtinCounts.Count,
				organizationInn);

			var attempt = 0;

			while(attempt < takeValidCodeAttempts)
			{
				attempt++;

				var remainingCounts = GetRemainingCounts(gtinCounts, allCollectedCodes);

				if(!remainingCounts.Any())
				{
					_logger.LogInformation("Все необходимые коды собраны за {Attempt} попыток.", attempt);
					break;
				}

				_logger.LogInformation(
					"Попытка {Attempt}/{MaxAttempts}: запрос {Count} кодов для {GtinCount} GTIN.",
					attempt,
					takeValidCodeAttempts,
					remainingCounts.Values.Sum(),
					remainingCounts.Count);

				var (validCodesByGtin, takenCodes) = await TryTakeAndValidateCodesAsync(
					codesPool,
					remainingCounts,
					organizationInn,
					attempt,
					cancellationToken);

				foreach(var gtinCodes in takenCodes)
				{
					if(!allTakenCodes.ContainsKey(gtinCodes.Key))
					{
						allTakenCodes[gtinCodes.Key] = new List<TrueMarkWaterIdentificationCode>();
					}
					allTakenCodes[gtinCodes.Key].AddRange(gtinCodes.Value);
				}


				if(validCodesByGtin is null || !validCodesByGtin.Any())
				{
					_logger.LogInformation("Не получено валидных кодов на попытке {Attempt}.", attempt);
					continue;
				}

				await DistributeValidCodes(
					codesPool,
					remainingCounts,
					validCodesByGtin,
					allCollectedCodes,
					gtinCounts,
					attempt,
					cancellationToken);
			}

			return await BuildResult(
				codesPool,
				gtinCounts,
				allCollectedCodes,
				allTakenCodes,
				attempt,
				cancellationToken);
		}

		private Dictionary<string, List<TrueMarkWaterIdentificationCode>> InitializeCollectedCodes(
			IDictionary<string, int> gtinCounts)
		{
			var result = new Dictionary<string, List<TrueMarkWaterIdentificationCode>>();
			foreach(var gtin in gtinCounts.Keys)
			{
				result[gtin] = new List<TrueMarkWaterIdentificationCode>();
			}
			return result;
		}

		private Dictionary<string, int> GetRemainingCounts(
			IDictionary<string, int> gtinCounts,
			Dictionary<string, List<TrueMarkWaterIdentificationCode>> allCollectedCodes)
		{
			return gtinCounts
				.Where(x => allCollectedCodes[x.Key].Count < x.Value)
				.ToDictionary(x => x.Key, x => x.Value - allCollectedCodes[x.Key].Count);
		}

		private async Task<(Dictionary<string, List<TrueMarkWaterIdentificationCode>> ValidCodes, Dictionary<string, List<TrueMarkWaterIdentificationCode>> TakenCodes)> TryTakeAndValidateCodesAsync(
			ITrueMarkCodesPool codesPool,
			Dictionary<string, int> remainingCounts,
			string organizationInn,
			int attempt,
			CancellationToken cancellationToken)
		{
			var validCodesResult = new Dictionary<string, List<TrueMarkWaterIdentificationCode>>();
			var takenCodesResult = new Dictionary<string, List<TrueMarkWaterIdentificationCode>>();

			foreach(var remainingCount in remainingCounts)
			{
				var gtin = remainingCount.Key;
				var needed = remainingCount.Value;
				var collectedForGtin = new List<TrueMarkWaterIdentificationCode>();
				var takenForGtin = new List<TrueMarkWaterIdentificationCode>();
				var attemptsForGtin = 0;
				var maxAttemptsForGtin = _edoSettings.CodePoolTakeValidCodeAttempts;

				while(collectedForGtin.Count < needed && attemptsForGtin < maxAttemptsForGtin)
				{
					attemptsForGtin++;
					var remainingForGtin = needed - collectedForGtin.Count;

					try
					{
						var codeIds = await codesPool.TakeCodes(gtin, remainingForGtin, cancellationToken);

						if(!codeIds.Any())
						{
							_logger.LogWarning("В пуле не найдены коды для GTIN {Gtin} (попытка {Attempt}).", gtin, attempt);
							break;
						}

						_logger.LogInformation(
							"Для GTIN {Gtin} получено {Count} кодов для проверки (попытка {Attempt}).",
							gtin,
							codeIds.Count,
							attempt);

						var codes = await _trueMarkCodeRepository.LoadWaterCodes(
							_uow,
							codeIds.ToArray(),
							cancellationToken);

						takenForGtin.AddRange(codes);

						var validationResult = await _trueMarkCodesValidator.ValidateAsync(
							codes,
							organizationInn,
							cancellationToken);

						var validCodes = validationResult.CodeResults
							.Where(r => r.IsValid)
							.Select(r => r.Code)
							.ToList();

						collectedForGtin.AddRange(validCodes);

						_logger.LogInformation(
							"Для GTIN {Gtin} собрано {CollectedCount} валидных кодов из {Needed}.",
							gtin,
							collectedForGtin.Count,
							needed);

						if(collectedForGtin.Count >= needed)
						{
							break;
						}
					}
					catch(EdoCodePoolMissingCodeException ex)
					{
						_logger.LogWarning(
							"Не удалось получить коды для GTIN {Gtin} (попытка {Attempt}): {Error}",
							gtin,
							attempt,
							ex.Message);
						break;
					}
				}

				if(collectedForGtin.Any())
				{
					validCodesResult[gtin] = collectedForGtin;
				}

				if(takenForGtin.Any())
				{
					takenCodesResult[gtin] = takenForGtin;
				}
			}

			return (validCodesResult, takenCodesResult);
		}

		private async Task DistributeValidCodes(
			ITrueMarkCodesPool codesPool,
			Dictionary<string, int> remainingCounts,
			Dictionary<string, List<TrueMarkWaterIdentificationCode>> validCodesByGtin,
			Dictionary<string, List<TrueMarkWaterIdentificationCode>> allCollectedCodes,
			IDictionary<string, int> gtinCounts,
			int attempt,
			CancellationToken cancellationToken)
		{
			foreach(var remainingCount in remainingCounts)
			{
				var gtin = remainingCount.Key;
				var needed = remainingCount.Value;

				if(validCodesByGtin.TryGetValue(gtin, out var validCodes))
				{
					var codesToAdd = validCodes.Take(needed).ToList();
					allCollectedCodes[gtin].AddRange(codesToAdd);

					if(validCodes.Count > needed)
					{
						var extraCodes = validCodes.Skip(needed).ToList();
						await ReturnCodesToPool(codesPool, extraCodes, gtin, cancellationToken);
					}

					var collectedCount = allCollectedCodes[gtin].Count;
					var targetCount = gtinCounts[gtin];

					_logger.LogInformation(
						"Для GTIN {Gtin} собрано {CollectedCount} из {TargetCount} кодов.",
						gtin,
						collectedCount,
						targetCount);
				}
				else
				{
					_logger.LogWarning(
						"Не найдены валидные коды для GTIN {Gtin} (попытка {Attempt}).",
						gtin,
						attempt);
				}
			}
		}

		private async Task ReturnCodesToPool(
			ITrueMarkCodesPool codesPool,
			List<TrueMarkWaterIdentificationCode> codes,
			string gtin,
			CancellationToken cancellationToken)
		{
			foreach(var code in codes)
			{
				await codesPool.PutCodeAsync(code.Id, cancellationToken);
			}

			_logger.LogDebug(
				"Для GTIN {Gtin} {CodeCount} кодов возвращено в пул.",
				gtin,
				codes.Count);
		}

		private async Task<Result<IDictionary<string, IList<TrueMarkWaterIdentificationCode>>>> BuildResult(
			ITrueMarkCodesPool codesPool,
			IDictionary<string, int> gtinCounts,
			Dictionary<string, List<TrueMarkWaterIdentificationCode>> allCollectedCodes,
			Dictionary<string, List<TrueMarkWaterIdentificationCode>> allTakenCodes,
			int attempt,
			CancellationToken cancellationToken)
		{
			var result = new Dictionary<string, IList<TrueMarkWaterIdentificationCode>>();
			var allSuccess = true;
			var hasAnyCodes = false;
			var missingGtins = new List<string>();

			foreach(var gtin in gtinCounts.Keys)
			{
				var collected = allCollectedCodes[gtin];
				var requested = gtinCounts[gtin];

				if(collected.Any())
				{
					hasAnyCodes = true;
					result[gtin] = collected;
					_logger.LogInformation(
						"Для GTIN {Gtin} итого собрано {CollectedCount} валидных кодов из {Requested}.",
						gtin,
						collected.Count,
						requested);
				}
				else
				{
					result[gtin] = new List<TrueMarkWaterIdentificationCode>();
					_logger.LogWarning(
						"Не удалось собрать валидные коды для GTIN {Gtin} за {Attempt} попыток.",
						gtin,
						attempt);
				}

				if(collected.Count < requested)
				{
					allSuccess = false;
					missingGtins.Add($"{gtin} ({collected.Count}/{requested})");
				}
			}

			var totalCollectedCount = result.Values.Sum(x => x.Count);
			var totalRequestedCount = gtinCounts.Values.Sum();

			if(!allSuccess || !hasAnyCodes)
			{
				_logger.LogWarning(
					"Не удалось собрать все коды. Возвращаем все взятые коды в пул.");

				foreach(var gtinCodes in allTakenCodes)
				{
					if(gtinCodes.Value.Any())
					{
						await ReturnCodesToPool(codesPool, gtinCodes.Value, gtinCodes.Key, cancellationToken);
					}
				}

				if(!hasAnyCodes)
				{
					var noCodesError = TrueMarkCodeErrors.NoCodes(missingGtins.FirstOrDefault());
					_logger.LogWarning(noCodesError.Message);

					return Result.Failure<IDictionary<string, IList<TrueMarkWaterIdentificationCode>>>(noCodesError);
				}

				var partialResultError = TrueMarkCodeErrors.PartialResult(string.Join(", ", missingGtins));
				_logger.LogWarning(partialResultError.Message);

				return Result.Failure<IDictionary<string, IList<TrueMarkWaterIdentificationCode>>>(partialResultError);
			}

			_logger.LogInformation(
				"Пакетный подбор завершен успешно. Собрано {TotalCollected} из {TotalRequested} кодов за {Attempt} попыток.",
				totalCollectedCount,
				totalRequestedCount,
				attempt);

			IDictionary<string, IList<TrueMarkWaterIdentificationCode>> resultAsIDictionary = result;
			return Result.Success(resultAsIDictionary);
		}

		private async Task<TrueMarkCodeValidationResult> ValidateAsync(
			TrueMarkWaterIdentificationCode code,
			string organizationInn,
			CancellationToken cancellationToken)
		{
			var validationResult = await _trueMarkCodesValidator.ValidateAsync(
				new[] { code },
				organizationInn,
				cancellationToken);

			var codeResult = validationResult.CodeResults.SingleOrDefault();

			if(codeResult is null)
			{
				throw new InvalidOperationException($"Не удалось получить результат проверки кода ЧЗ Id {code.Id} из пула.");
			}

			return codeResult;
		}
	}
}
