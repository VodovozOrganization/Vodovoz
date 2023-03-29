﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Settings.Edo;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCodePoolChecker
	{
		private readonly ILogger<SelfdeliveryReceiptCreator> _logger;
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly TrueMarkCodesChecker _trueMarkCodesChecker;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly IEdoSettings _edoSettings;

		public TrueMarkCodePoolChecker(
			ILogger<SelfdeliveryReceiptCreator> logger,
			TrueMarkCodesPool trueMarkCodesPool,
			TrueMarkCodesChecker trueMarkCodesChecker,
			ITrueMarkRepository trueMarkRepository,
			IEdoSettings edoSettings
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_trueMarkCodesChecker = trueMarkCodesChecker ?? throw new ArgumentNullException(nameof(trueMarkCodesChecker));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_edoSettings = edoSettings;
		}

		public async Task StartCheck(CancellationToken cancellationToken)
		{
			var selectingCodesCount = _edoSettings.CodePoolCheckCodesDepth;

			_logger.LogInformation("Для проверки требуется {selectingCodesCount} кодов.", selectingCodesCount);
			_logger.LogInformation("Запрос ранее не проверенных кодов.");
			var selectedCodeIds = _trueMarkCodesPool.SelectCodes(selectingCodesCount, false).ToList();
			_logger.LogInformation("Получено {selectedCodesCount} ранее не проверенных кодов.", selectedCodeIds.Count);

			if(selectedCodeIds.Count < selectingCodesCount)
			{
				var promotedCount = selectingCodesCount - selectedCodeIds.Count;

				_logger.LogInformation("Добираем до требуемого количества ранее проверенными кодами.");

				var promotedCodes = _trueMarkCodesPool.SelectCodes(promotedCount, true);

				_logger.LogInformation("Получено {promotedCodesCount} ранее проверенных кодов.", promotedCodes.Count());

				selectedCodeIds.AddRange(promotedCodes);
			}

			if(selectedCodeIds.Count <= 0)
			{
				_logger.LogInformation("Нет кодов на проверку.");
				return;
			}

			_logger.LogInformation("Всего {selectedCodesCount} кодов на проверку.", selectedCodeIds.Count);

			var codeIdsToPromote = new List<int>();
			var codeIdsToDelete = new List<int>();

			_logger.LogInformation("Загружаем сущности с подробной информацией о кодах.");

			var selectedCodes = _trueMarkRepository.LoadWaterCodes(selectedCodeIds);

			var toSkip = 0;

			while(selectedCodes.Count() > toSkip)
			{
				var codesToCheck = selectedCodes.Skip(toSkip).Take(100);
				toSkip += 100;

				_logger.LogInformation("Отправка на проверку {codesToCheckCount}/{selectedCodesCount} кодов.", codesToCheck.Count(), selectedCodeIds.Count);

				await Task.Delay(2000);
				var checkResults = await _trueMarkCodesChecker.CheckCodesAsync(codesToCheck, cancellationToken);

				foreach(var checkResult in checkResults)
				{
					if(checkResult.Introduced)
					{
						codeIdsToPromote.Add(checkResult.Code.Id);
					}
					else
					{
						codeIdsToDelete.Add(checkResult.Code.Id);
					}
				}
			}

			if(codeIdsToPromote.Any())
			{
				var extraSecondsPromotion = _edoSettings.CodePoolPromoteWithExtraSeconds;
				_logger.LogInformation("Продвижение {promotedCodesCount} проверенных кодов на верх пула на дополнительыне {extraSecondsPromotion} секунд сверх текущего времени.", codeIdsToPromote.Count, extraSecondsPromotion);
				_trueMarkCodesPool.PromoteCodes(codeIdsToPromote, extraSecondsPromotion);
			}

			if(codeIdsToDelete.Any())
			{
				_logger.LogInformation("Удаление из пула {codesToDeleteCount} кодов не прошедших проверку.", codeIdsToDelete.Count);
				_trueMarkCodesPool.DeleteCodes(codeIdsToDelete);
			}
		}
	}
}
