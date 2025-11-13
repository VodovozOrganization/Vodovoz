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
		private readonly ILogger<TrueMarkCodesChecker> _logger;
		private readonly ITrueMarkApiClient _trueMarkClient;

		public TrueMarkCodesChecker(
			ILogger<TrueMarkCodesChecker> logger,
			ITrueMarkApiClient trueMarkClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkClient = trueMarkClient ?? throw new ArgumentNullException(nameof(trueMarkClient));
		}

		public async Task<IEnumerable<TrueMarkCheckResult>> Check(IEnumerable<TrueMarkWaterIdentificationCode> codes, CancellationToken cancellationToken)
		{
			var result = new List<TrueMarkCheckResult>();

			var productCodes = codes.ToDictionary(x => x.IdentificationCode);

			var productInstancesInfo = await _trueMarkClient.GetProductInstanceInfoAsync(productCodes.Keys, cancellationToken);

			if(!string.IsNullOrWhiteSpace(productInstancesInfo.ErrorMessage)
				&& (productInstancesInfo.InstanceStatuses is null || !productInstancesInfo.InstanceStatuses.Any()))
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
					OwnerName = instanceStatus.OwnerName,
					ExpirationDate = instanceStatus.ExpirationDate,
				});
			}

			return result;
		}

		public async Task<IDictionary<TrueMarkWaterIdentificationCode, ProductInstanceStatus>> CheckCodes(
			IEnumerable<TrueMarkWaterIdentificationCode> codes, 
			CancellationToken cancellationToken
			)
		{
			var productCodes = codes.ToDictionary(x => x.IdentificationCode);
			var productInstancesInfo = await _trueMarkClient.GetProductInstanceInfoAsync(productCodes.Keys, cancellationToken);

			if(!string.IsNullOrWhiteSpace(productInstancesInfo.ErrorMessage)
				&& (productInstancesInfo.InstanceStatuses is null || !productInstancesInfo.InstanceStatuses.Any()))
			{
				throw new TrueMarkException($"Не удалось получить информацию о состоянии товаров в системе Честный знак. " +
					$"Подробности: {productInstancesInfo.ErrorMessage}");
			}

			var result = new Dictionary<TrueMarkWaterIdentificationCode, ProductInstanceStatus>();

			foreach(var instanceStatus in productInstancesInfo.InstanceStatuses)
			{
				var codeFound = productCodes.TryGetValue(instanceStatus.IdentificationCode, 
					out TrueMarkWaterIdentificationCode code);

				if(!codeFound)
				{
					continue;
				}

				result.Add(code, instanceStatus);
			}

			return result;
		}
	}
}
