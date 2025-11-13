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
using VodovozBusiness.Models.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCodePoolChecker
	{
		private readonly ILogger<TrueMarkCodePoolChecker> _logger;
		private readonly TrueMarkCodesPoolManager _trueMarkCodesPool;
		private readonly TrueMarkCodesChecker _trueMarkCodesChecker;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly IEdoSettings _edoSettings;
		private readonly OurCodesChecker _ourCodesChecker;

		public TrueMarkCodePoolChecker(
			ILogger<TrueMarkCodePoolChecker> logger,
			TrueMarkCodesPoolManager trueMarkCodesPool,
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

			var unpromotedCodes = await _trueMarkCodesPool.SelectCodesAsync(codesCountToCheck, false, cancellationToken);
			var codeIdsToCheck = unpromotedCodes.ToList();

			_logger.LogInformation("Получено {selectedCodesCount} ранее не проверенных кодов.", codeIdsToCheck.Count);

			if(codeIdsToCheck.Count < codesCountToCheck)
			{
				var promotedCount = codesCountToCheck - codeIdsToCheck.Count;

				_logger.LogInformation("Добираем до требуемого количества ранее проверенными кодами.");

				var promotedCodes = _trueMarkCodesPool.SelectCodes(promotedCount, true);

				_logger.LogInformation("Получено {promotedCodesCount} ранее проверенных кодов.", promotedCodes.Count());

				codeIdsToCheck.AddRange(promotedCodes);
			}

			if(codeIdsToCheck.Count <= 0)
			{
				_logger.LogInformation("Нет кодов на проверку.");
				return;
			}

			_logger.LogInformation("Всего {selectedCodesCount} кодов на проверку.", codeIdsToCheck.Count);

			var codeIdsToPromote = new List<int>();
			var codeIdsToDelete = new List<int>();

			_logger.LogInformation("Загружаем сущности с подробной информацией о кодах.");
			var codesToCheck = await _trueMarkRepository.LoadWaterCodes(codeIdsToCheck, cancellationToken);

			_logger.LogInformation("Отправка на проверку {codesToCheckCount} кодов.", codesToCheck.Count());
			var checkResults = await _trueMarkCodesChecker.CheckCodes(codesToCheck, cancellationToken);

			foreach(var checkResult in checkResults)
			{
				var isIntroduced = checkResult.Value.Status == ProductInstanceStatusEnum.Introduced;
				var isOurOrganizationOwner = _ourCodesChecker.IsOurOrganizationOwner(checkResult.Value.OwnerInn);
				var isOurGtin = _ourCodesChecker.IsOurGtinOwner(checkResult.Value.Gtin);
				var notExpired = checkResult.Value.ExpirationDate >= DateTime.Today;

				if(isIntroduced && isOurOrganizationOwner && isOurGtin && notExpired)
				{
					codeIdsToPromote.Add(checkResult.Key.Id);
				}
				else
				{
					codeIdsToDelete.Add(checkResult.Key.Id);
				}
			}

			if(codeIdsToPromote.Any())
			{
				var extraSecondsPromotion = _edoSettings.CodePoolPromoteWithExtraSeconds;
				_logger.LogInformation(
					"Продвижение {promotedCodesCount} проверенных кодов на верх пула на дополнительыне {extraSecondsPromotion} секунд сверх текущего времени.",
					codeIdsToPromote.Count, extraSecondsPromotion);
				await _trueMarkCodesPool.PromoteCodesAsync(codeIdsToPromote, extraSecondsPromotion, cancellationToken);
			}

			if(codeIdsToDelete.Any())
			{
				_logger.LogInformation("Удаление из пула {codesToDeleteCount} кодов не прошедших проверку.", codeIdsToDelete.Count);
				await _trueMarkCodesPool.DeleteCodesAsync(codeIdsToDelete, cancellationToken);
			}
		}
	}
}
