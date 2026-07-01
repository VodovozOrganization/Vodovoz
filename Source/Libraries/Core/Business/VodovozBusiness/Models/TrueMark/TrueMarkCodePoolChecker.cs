using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using TrueMark.Contracts;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Settings.Edo;

namespace VodovozBusiness.Models.TrueMark
{
	public class TrueMarkCodePoolChecker
	{
		private readonly ILogger<TrueMarkCodePoolChecker> _logger;
		private readonly ITrueMarkCodesPoolManager _trueMarkCodesPool;
		private readonly TrueMarkCodesChecker _trueMarkCodesChecker;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly IEdoSettings _edoSettings;
		private readonly OurCodesChecker _ourCodesChecker;

		public TrueMarkCodePoolChecker(
			ILogger<TrueMarkCodePoolChecker> logger,
			ITrueMarkCodesPoolManager trueMarkCodesPool,
			TrueMarkCodesChecker trueMarkCodesChecker,
			ITrueMarkRepository trueMarkRepository,
			IEdoSettings edoSettings,
			OurCodesChecker ourCodesChecker
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_trueMarkCodesChecker = trueMarkCodesChecker ?? throw new ArgumentNullException(nameof(trueMarkCodesChecker));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_edoSettings = edoSettings;
			_ourCodesChecker = ourCodesChecker ?? throw new ArgumentNullException(nameof(ourCodesChecker));
		}

		public async Task StartCheck(CancellationToken cancellationToken)
		{
			var codesCountToCheck = _edoSettings.CodePoolCheckCodesDepth;

			_logger.LogInformation("Для проверки требуется {selectingCodesCount} кодов.", codesCountToCheck);
			_logger.LogInformation("Запрос ранее не проверенных кодов.");

			var unpromotedCodes = await _trueMarkCodesPool.SelectCodesForCheckAsync(codesCountToCheck, cancellationToken);
			var codeIdsToCheck = unpromotedCodes.ToList();

			_logger.LogInformation("Получено {selectedCodesCount} ранее не проверенных кодов.", codeIdsToCheck.Count);

			if(codeIdsToCheck.Count <= 0)
			{
				_logger.LogInformation("Нет кодов на проверку.");
				return;
			}

			_logger.LogInformation("Всего {selectedCodesCount} кодов на проверку.", codeIdsToCheck.Count);

			var codeExpirationMap = new Dictionary<int, DateTime>();
			var validCodeIds = new List<int>();
			var invalidCodeIds = new List<int>();

			_logger.LogInformation("Загружаем сущности с подробной информацией о кодах.");
			var codesToCheck = await _trueMarkRepository.LoadWaterCodes(codeIdsToCheck, cancellationToken);

			_logger.LogInformation("Отправка на проверку {codesToCheckCount} кодов.", codesToCheck.Count());
			var checkResults = await _trueMarkCodesChecker.CheckCodes(codesToCheck, cancellationToken);

			foreach(var checkResult in checkResults)
			{
				var code = checkResult.Key;
				var status = checkResult.Value;

				var expirationDate = status.ExpirationDate;
				var isIntroduced = status.Status == ProductInstanceStatusEnum.Introduced;
				var isOurOrganizationOwner = _ourCodesChecker.IsOurOrganizationOwner(status.OwnerInn);
				var isOurGtin = _ourCodesChecker.IsOurGtinOwner(status.Gtin);
				var notExpired = expirationDate >= DateTime.Today;

				codeExpirationMap[code.Id] = expirationDate.Value;

				var isValid = isIntroduced
					&& isOurOrganizationOwner
					&& isOurGtin
					&& notExpired;

				if(isValid)
				{
					validCodeIds.Add(code.Id);
				}
				else
				{
					invalidCodeIds.Add(code.Id);
				}
			}

			_logger.LogInformation("Обновление сроков годности для {count} кодов.", codeExpirationMap.Count);
			await _trueMarkCodesPool.UpdateCodesExpirationAsync(codeExpirationMap, cancellationToken);

			if(validCodeIds.Any())
			{
				var extraSecondsPromotion = _edoSettings.CodePoolPromoteWithExtraSeconds;
				_logger.LogInformation(
					"Продвижение {promotedCodesCount} проверенных кодов на верх пула на дополнительные {extraSecondsPromotion} секунд.",
					validCodeIds.Count,
					extraSecondsPromotion);
				await _trueMarkCodesPool.PromoteCodesAsync(validCodeIds, extraSecondsPromotion, cancellationToken);
			}

			if(invalidCodeIds.Any())
			{
				_logger.LogInformation("Удаление из пула {codesToDeleteCount} невалидных кодов.", invalidCodeIds.Count);
				await _trueMarkCodesPool.DeleteCodesAsync(invalidCodeIds, cancellationToken);
			}
		}
	}
}
