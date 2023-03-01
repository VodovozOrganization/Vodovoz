using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMarkApi.Library;
using TrueMarkApi.Library.Dto;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Settings.Edo;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCodePoolChecker
	{
		private readonly ILogger<TrueMarkSelfDeliveriesHandler> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly TrueMarkApiClient _trueMarkApiClient;
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly IEdoSettings _edoSettings;

		public TrueMarkCodePoolChecker(
			ILogger<TrueMarkSelfDeliveriesHandler> logger, 
			IUnitOfWorkFactory uowFactory, 
			TrueMarkApiClient trueMarkApiClient,
			TrueMarkCodesPool trueMarkCodesPool,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			IEdoSettings edoSettings
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_edoSettings = edoSettings;
		}

		public async Task StartCheck(CancellationToken cancellationToken)
		{
			var selectingCodesCount = _edoSettings.CodePoolCheckCodesDepth;

			_logger.LogInformation("Для проверки требуется {0} кодов.", selectingCodesCount);
			_logger.LogInformation("Запрос ранее не проверенных кодов.");
			var selectedCodeIds = _trueMarkCodesPool.SelectCodes(selectingCodesCount, false).ToList();
			_logger.LogInformation("Получено {0} ранее не проверенных кодов.", selectedCodeIds.Count);

			if(selectedCodeIds.Count < selectingCodesCount)
			{
				var promotedCount = selectingCodesCount - selectedCodeIds.Count;

				_logger.LogInformation("Добираем до требуемого количества ранее проверенными кодами.");

				var promotedCodes = _trueMarkCodesPool.SelectCodes(promotedCount, true);

				_logger.LogInformation("Получено {0} ранее проверенных кодов.", promotedCodes.Count());

				selectedCodeIds.AddRange(promotedCodes);
			}

			if(selectedCodeIds.Count <= 0)
			{
				_logger.LogInformation("Нет кодов на проверку.");
				return;
			}

			_logger.LogInformation("Всего {0} кодов на проверку.", selectedCodeIds.Count);

			var codeIdsToPromote = new List<int>();
			var codeIdsToDelete = new List<int>();

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				_logger.LogInformation("Загружаем сущности с подробной информацией о кодах.");

				var selectedCodes = uow.Session.QueryOver<TrueMarkWaterIdentificationCode>()
					.WhereRestrictionOn(x => x.Id).IsIn(selectedCodeIds)
					.List();

				var toSkip = 0;

				while(selectedCodes.Count > toSkip)
				{
					var codesToCheck = selectedCodes.Skip(toSkip).Take(100);
					toSkip += 100;

					_logger.LogInformation("Отправка на проверку {0}/{1} кодов.", codesToCheck.Count(), selectedCodeIds.Count);

					await Task.Delay(2000);
					var checkResult = await CheckCodes(codesToCheck, cancellationToken);

					var validCodesIds = checkResult.ValidCodes.Select(x => x.Id);
					codeIdsToPromote.AddRange(validCodesIds);

					var invalidCodesIds = checkResult.InvalidCodes.Select(x => x.Id);
					codeIdsToDelete.AddRange(invalidCodesIds);
				}

				if(codeIdsToPromote.Any())
				{
					var extraSecondsPromotion = _edoSettings.CodePoolPromoteWithExtraSeconds;
					_logger.LogInformation("Продвижение {0} проверенных кодов на верх пула на дополнительыне {1} секунд сверх текущего времени.", codeIdsToPromote.Count, extraSecondsPromotion);
					_trueMarkCodesPool.PromoteCodes(codeIdsToPromote, extraSecondsPromotion);
				}

				if(codeIdsToDelete.Any())
				{
					_logger.LogInformation("Удаление из пула {0} кодов не прошедших проверку.", codeIdsToDelete.Count);
					_trueMarkCodesPool.DeleteCodes(codeIdsToDelete);
				}
			}
		}

		private async Task<CheckResult> CheckCodes(IEnumerable<TrueMarkWaterIdentificationCode> codes, CancellationToken cancellationToken)
		{
			var result = new CheckResult();

			var productCodes = codes.ToDictionary(x => _trueMarkWaterCodeParser.GetWaterIdentificationCode(x));

			var productInstancesInfo = await _trueMarkApiClient.GetProductInstanceInfoAsync(productCodes.Keys, cancellationToken);

			if(!string.IsNullOrWhiteSpace(productInstancesInfo.ErrorMessage))
			{
				throw new TrueMarkException($"Не удалось получить информацию о состоянии товаров в системе Честный знак. Подробности: {productInstancesInfo.ErrorMessage}");
			}

			foreach(var instanceStatus in productInstancesInfo.InstanceStatuses)
			{
				var codeFound = productCodes.TryGetValue(instanceStatus.IdentificationCode, out TrueMarkWaterIdentificationCode code);
				if(!codeFound)
				{
					_logger.LogError("Проверенный в системе Честный знак, код ({0}) не был найден среди отправленных на проверку.", instanceStatus.IdentificationCode);
					continue;
				}

				if(instanceStatus.Status == ProductInstanceStatusEnum.Introduced)
				{
					result.ValidCodes.Add(code);
				}
				else
				{
					result.InvalidCodes.Add(code);
				}
			}

			return result;
		}

		private class CheckResult
		{
			public List<TrueMarkWaterIdentificationCode> ValidCodes { get; set; } = new List<TrueMarkWaterIdentificationCode>();
			public List<TrueMarkWaterIdentificationCode> InvalidCodes { get; set; } = new List<TrueMarkWaterIdentificationCode>();
		}
	}
}
