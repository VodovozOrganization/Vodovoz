using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Settings.Edo;
using VodovozBusiness.Models.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCodePoolChecker
	{
		private readonly ILogger<TrueMarkCodePoolChecker> _logger;
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly TrueMarkCodesChecker _trueMarkCodesChecker;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly IEdoSettings _edoSettings;
		private readonly OurCodesChecker _ourCodesChecker;
		private readonly ITag1260Checker _tag1260Checker;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public TrueMarkCodePoolChecker(
			ILogger<TrueMarkCodePoolChecker> logger,
			TrueMarkCodesPool trueMarkCodesPool,
			TrueMarkCodesChecker trueMarkCodesChecker,
			ITrueMarkRepository trueMarkRepository,
			IEdoSettings edoSettings,
			OurCodesChecker ourCodesChecker,
			ITag1260Checker tag1260Checker,
			IUnitOfWorkFactory unitOfWorkFactory
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_trueMarkCodesChecker = trueMarkCodesChecker ?? throw new ArgumentNullException(nameof(trueMarkCodesChecker));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_edoSettings = edoSettings;
			_ourCodesChecker = ourCodesChecker ?? throw new ArgumentNullException(nameof(ourCodesChecker));
			_tag1260Checker = tag1260Checker ?? throw new ArgumentNullException(nameof(tag1260Checker));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
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

				using(var unitOfWorkTag1260 = _unitOfWorkFactory.CreateWithoutRoot("Tag1260 codes Check"))
				{
					// Проверям код на валидность от организации 1, т.к. пока не знаем, для какой организации он будет использоваться, в пуле просто убеждаемся, что код валидный.
					// Проверяется от нужной организации ещё раз в месте использования.
					var organizationId = 1;

					await _tag1260Checker.UpdateInfoForTag1260Async(codesToCheck, unitOfWorkTag1260, organizationId, cancellationToken);

					unitOfWorkTag1260.Commit();
				}

				var checkResults = await _trueMarkCodesChecker.CheckCodesAsync(codesToCheck, cancellationToken);

				foreach(var checkResult in checkResults)
				{
					var isOurOrganizationOwner = _ourCodesChecker.IsOurOrganizationOwner(checkResult.OwnerInn);
					var isOurGtin = _ourCodesChecker.IsOurGtinOwner(checkResult.Code.GTIN);
					var isTag1260Valid = checkResult.Code?.IsTag1260Valid ?? false;

					if(checkResult.Introduced && isOurOrganizationOwner && isOurGtin && isTag1260Valid)
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
