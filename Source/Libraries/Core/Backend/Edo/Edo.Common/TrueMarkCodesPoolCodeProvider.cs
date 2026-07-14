using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Settings.Edo;

namespace Edo.Common
{
	public class TrueMarkCodesPoolCodeProvider : ITrueMarkCodesPoolCodeProvider
	{
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkCodesValidator _trueMarkCodesValidator;
		private readonly IEdoSettings _edoSettings;
		private readonly ILogger<TrueMarkCodesPoolCodeProvider> _logger;

		public TrueMarkCodesPoolCodeProvider(
			IUnitOfWork uow,
			ITrueMarkCodesValidator trueMarkCodesValidator,
			IEdoSettings edoSettings,
			ILogger<TrueMarkCodesPoolCodeProvider> logger)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkCodesValidator = trueMarkCodesValidator ?? throw new ArgumentNullException(nameof(trueMarkCodesValidator));
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

		public async Task<IDictionary<string, IList<TrueMarkWaterIdentificationCode>>> TakeValidCodesBatchAsync(
			ITrueMarkCodesPool codesPool,
			IDictionary<string, int> gtinCounts,
			string organizationInn,
			CancellationToken cancellationToken)
		{
			if(codesPool is null)
			{
				throw new ArgumentNullException(nameof(codesPool));
			}

			if(gtinCounts is null)
			{
				throw new ArgumentNullException(nameof(gtinCounts));
			}

			var result = new Dictionary<string, IList<TrueMarkWaterIdentificationCode>>();
			var allCodeIds = new List<int>();
			var gtinCodeMap = new Dictionary<string, List<int>>();

			_logger.LogInformation(
				"Начат пакетный подбор {TotalCount} кодов из пула для {GtinCount} GTIN. Организация: {OrganizationInn}.",
				gtinCounts.Values.Sum(),
				gtinCounts.Count,
				organizationInn);

			foreach(var gtinCount in gtinCounts)
			{
				var gtin = gtinCount.Key;
				var count = gtinCount.Value;

				try
				{
					var codeIds = await codesPool.TakeCodes(gtin, count, cancellationToken);

					if(!codeIds.Any())
					{
						_logger.LogWarning("Не найдены коды для GTIN {Gtin}.", gtin);
						result[gtin] = new List<TrueMarkWaterIdentificationCode>();
						continue;
					}

					_logger.LogInformation(
						"Для GTIN {Gtin} получено {Count} кодов из {Requested}.",
						gtin,
						codeIds.Count,
						count);

					gtinCodeMap[gtin] = codeIds.ToList();
					allCodeIds.AddRange(codeIds);
				}
				catch(EdoCodePoolMissingCodeException ex)
				{
					_logger.LogWarning(
						"Не удалось получить коды для GTIN {Gtin}: {Error}",
						gtin,
						ex.Message);
					result[gtin] = new List<TrueMarkWaterIdentificationCode>();
				}
			}

			if(allCodeIds.Any())
			{
				TrueMarkWaterIdentificationCode codeAlias = null;

				var codes = await _uow.Session
					.QueryOver(() => codeAlias)
					.WhereRestrictionOn(() => codeAlias.Id)
					.IsIn(allCodeIds.ToArray())
					.ListAsync(cancellationToken);

				var validationResult = await _trueMarkCodesValidator.ValidateAsync(
					codes,
					organizationInn,
					cancellationToken);

				var validCodesByGtin = validationResult.CodeResults
					.Where(r => r.IsValid)
					.GroupBy(r => r.Code.Gtin)
					.ToDictionary(g => g.Key, g => g.Select(r => r.Code).ToList());

				foreach(var gtin in gtinCounts.Keys)
				{
					if(result.ContainsKey(gtin) && !result[gtin].Any())
					{
						continue;
					}

					if(validCodesByGtin.TryGetValue(gtin, out var validCodes))
					{
						result[gtin] = validCodes;

						var requestedCount = gtinCounts[gtin];
						if(validCodes.Count < requestedCount)
						{
							_logger.LogWarning(
								"Для GTIN {Gtin} валидны только {ValidCount} из {Requested} кодов.",
								gtin,
								validCodes.Count,
								requestedCount);
						}
					}
					else
					{
						_logger.LogWarning("Не найдены валидные коды для GTIN {Gtin}.", gtin);
						result[gtin] = new List<TrueMarkWaterIdentificationCode>();
					}
				}

				var invalidCodes = validationResult.CodeResults
					.Where(r => !r.IsValid)
					.ToList();

				if(invalidCodes.Any())
				{
					_logger.LogWarning(
						"Обнаружено {Count} невалидных кодов.",
						invalidCodes.Count);
				}
			}

			return result;
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
