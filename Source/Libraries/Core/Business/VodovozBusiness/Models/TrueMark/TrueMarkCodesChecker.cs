using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCodesChecker
	{
		private const int _checkDelayMs = 2000;

		private readonly ILogger<TrueMarkCodesChecker> _logger;
		private readonly TrueMarkWaterCodeParser _codeParser;
		private readonly TrueMarkApiClient _trueMarkClient;

		public TrueMarkCodesChecker(
			ILogger<TrueMarkCodesChecker> logger,
			TrueMarkWaterCodeParser codeParser,
			TrueMarkApiClient trueMarkClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_codeParser = codeParser ?? throw new ArgumentNullException(nameof(codeParser));
			_trueMarkClient = trueMarkClient ?? throw new ArgumentNullException(nameof(trueMarkClient));
		}

		public async Task<IEnumerable<TrueMarkProductCheckResult>> CheckCodesAsync(IEnumerable<CashReceiptProductCode> codes, CancellationToken cancellationToken)
		{
			var result = new List<TrueMarkProductCheckResult>();

			var validCodes = codes.Where(x => x.IsValid);
			var sourceCodesDic = validCodes.ToDictionary(x => x.SourceCode);

			var checkResults = await CheckCodesAsync(sourceCodesDic.Keys, cancellationToken);

			foreach(var checkResult in checkResults)
			{
				if(!sourceCodesDic.TryGetValue(checkResult.Code, out CashReceiptProductCode productCode))
				{
					throw new TrueMarkException($"Невозможно найти код {checkResult.Code.RawCode} в списке отправленных на проверку.");
				}

				var productCheckResult = new TrueMarkProductCheckResult
				{
					Code = productCode,
					Introduced = checkResult.Introduced,
					OwnerInn = checkResult.OwnerInn,
					OwnerName = checkResult.OwnerName
				};

				result.Add(productCheckResult);
			}

			return result;
		}

		public async Task<IEnumerable<TrueMarkCheckResult>> CheckCodesAsync(IEnumerable<TrueMarkWaterIdentificationCode> codes, CancellationToken cancellationToken)
		{
			var result = new List<TrueMarkCheckResult>();
			var codesCount = codes.Count();
			var toSkip = 0;

			while(codesCount > toSkip)
			{
				var codesToCheck = codes.Skip(toSkip).Take(100);
				toSkip += 100;

				_logger.LogInformation("Отправка на проверку {codesToCheckCount}/{codesCount} кодов.", codesToCheck.Count(), codesCount);

				await Task.Delay(_checkDelayMs);
				var checkResult = await Check(codesToCheck, cancellationToken);

				result.AddRange(checkResult);
			}

			return result;
		}

		private async Task<IEnumerable<TrueMarkCheckResult>> Check(IEnumerable<TrueMarkWaterIdentificationCode> codes, CancellationToken cancellationToken)
		{
			var result = new List<TrueMarkCheckResult>();

			var productCodes = codes.ToDictionary(x => _codeParser.GetWaterIdentificationCode(x));

			var productInstancesInfo = await _trueMarkClient.GetProductInstanceInfoAsync(productCodes.Keys, cancellationToken);

			if(!string.IsNullOrWhiteSpace(productInstancesInfo.ErrorMessage) && !productInstancesInfo.InstanceStatuses.Any())
			{
				throw new TrueMarkException($"Не удалось получить информацию о состоянии товаров в системе Честный знак. Подробности: {productInstancesInfo.ErrorMessage}");
			}

			foreach(var instanceStatus in productInstancesInfo.InstanceStatuses)
			{
				var codeFound = productCodes.TryGetValue(instanceStatus.IdentificationCode, out TrueMarkWaterIdentificationCode code);

				if(!codeFound)
				{
					continue;
				}

				result.Add(new TrueMarkCheckResult
				{
					Code = code,
					Introduced = instanceStatus.Status == ProductInstanceStatusEnum.Introduced,
					OwnerInn = instanceStatus.OwnerInn,
					OwnerName = instanceStatus.OwnerName
				});
			}

			return result;
		}
	}
}
