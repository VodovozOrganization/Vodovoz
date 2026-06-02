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

namespace Edo.Common
{
	public class TrueMarkCodesPoolCodeProvider : ITrueMarkCodesPoolCodeProvider
	{
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkCodesValidator _trueMarkCodesValidator;
		private readonly ILogger<TrueMarkCodesPoolCodeProvider> _logger;

		public TrueMarkCodesPoolCodeProvider(
			IUnitOfWork uow,
			ITrueMarkCodesValidator trueMarkCodesValidator,
			ILogger<TrueMarkCodesPoolCodeProvider> logger)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkCodesValidator = trueMarkCodesValidator ?? throw new ArgumentNullException(nameof(trueMarkCodesValidator));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<TrueMarkWaterIdentificationCode> TakeValidCodeAsync(
			ITrueMarkCodesPool codesPool,
			GtinEntity gtin,
			string organizationInn,
			CancellationToken cancellationToken)
		{
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

			var gtinsList = gtins.ToList();
			EdoCodePoolMissingCodeException exception = null;

			_logger.LogInformation(
				"Начат подбор валидного кода из пула для GTIN: {Gtins}. Организация: {OrganizationInn}.",
				string.Join(", ", gtinsList.Select(x => x.GtinNumber)),
				organizationInn);

			foreach(var gtin in gtinsList)
			{
				while(true)
				{
					int codeId;

					try
					{
						codeId = await codesPool.TakeCode(gtin.GtinNumber, cancellationToken);
					}
					catch(EdoCodePoolMissingCodeException ex)
					{
						_logger.LogInformation(
							"В пуле не найден код для GTIN {Gtin}.",
							gtin.GtinNumber);

						exception = ex;
						break;
					}

					_logger.LogInformation(
						"Из пула получен код ЧЗ Id {CodeId} для GTIN {Gtin}. Выполняется актуальная валидация.",
						codeId,
						gtin.GtinNumber);

					var code = await _uow.Session.GetAsync<TrueMarkWaterIdentificationCode>(codeId, cancellationToken);

					if(code is null)
					{
						throw new InvalidOperationException($"Не найден код ЧЗ с Id {codeId}, полученный из пула.");
					}

					var validationResult = await ValidateAsync(code, organizationInn, cancellationToken);

					if(validationResult.IsValid)
					{
						_logger.LogInformation(
							"Код ЧЗ Id {CodeId} из пула прошел актуальную валидацию.",
							code.Id);

						return code;
					}

					_logger.LogWarning(
						"Код ЧЗ Id {CodeId} из пула не прошел актуальную валидацию. " +
						"GTIN наш: {IsOurGtin}; владелец наша организация: {IsOwnedByOurOrganization}; " +
						"код в обороте: {IsIntroduced}; просрочен: {IsExpired}.",
						code.Id,
						validationResult.IsOurGtin,
						validationResult.IsOwnedByOurOrganization,
						validationResult.IsIntroduced,
						validationResult.IsExpired);
				}
			}

			_logger.LogWarning(
				"В пуле не найден валидный код для GTIN: {Gtins}. Организация: {OrganizationInn}.",
				string.Join(", ", gtinsList.Select(x => x.GtinNumber)),
				organizationInn);

			throw exception ?? new EdoCodePoolMissingCodeException(
				$"В пуле не найден валидный код для GTIN: {string.Join(", ", gtinsList.Select(x => x.GtinNumber))}.");
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
