using BitrixNotificationsSend.Client;
using BitrixNotificationsSend.Contracts;
using BitrixNotificationsSend.Contracts.Dto;
using BitrixNotificationsSend.Library.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BitrixNotificationsSend.Library.Services
{
	/// <inheritdoc/>
	public class BitrixDealsStagesReadService : IBitrixDealsStagesReadService
	{
		/// <summary>
		/// Пауза между пакетными запросами чтения сделок из Битрикс24
		/// </summary>
		private static readonly TimeSpan _delayBetweenReadBatches = TimeSpan.FromSeconds(1);

		private readonly ILogger<BitrixDealsStagesReadService> _logger;
		private readonly IBitrixDealsClient _bitrixDealsClient;

		public BitrixDealsStagesReadService(
			ILogger<BitrixDealsStagesReadService> logger,
			IBitrixDealsClient bitrixDealsClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_bitrixDealsClient = bitrixDealsClient ?? throw new ArgumentNullException(nameof(bitrixDealsClient));
		}

		/// <inheritdoc/>
		public async Task<BitrixDealsStagesResult> GetDealsStages(
			IEnumerable<long> dealIds,
			CancellationToken cancellationToken)
		{
			var dealsStages = new BitrixDealsStagesResult();

			var dealIdsChunks = dealIds
				.Distinct()
				.SplitToChunks(BitrixApiLimits.MaxBatchCommandsCount)
				.ToList();

			for(var chunkIndex = 0; chunkIndex < dealIdsChunks.Count; chunkIndex++)
			{
				if(chunkIndex > 0)
				{
					await Task.Delay(_delayBetweenReadBatches, cancellationToken);
				}

				var chunkResult = await _bitrixDealsClient.GetDealsStages(dealIdsChunks[chunkIndex], cancellationToken);

				if(chunkResult.IsFailure)
				{
					_logger.LogError(
						"Ошибка чтения стадий сделок из Битрикс24: {ErrorMessage}",
						chunkResult.Errors.FirstOrDefault()?.Message);

					continue;
				}

				foreach(var stageByDealId in chunkResult.Value.StagesByDealIds)
				{
					dealsStages.StagesByDealIds[stageByDealId.Key] = stageByDealId.Value;
				}

				foreach(var notFoundDealId in chunkResult.Value.NotFoundDealIds)
				{
					dealsStages.NotFoundDealIds.Add(notFoundDealId);
				}

				foreach(var error in chunkResult.Value.Errors)
				{
					_logger.LogError(
						"Ошибка чтения стадии сделки командой {CommandKey} в Битрикс24: {ErrorMessage}",
						error.CommandKey,
						error.Message);
				}
			}

			return dealsStages;
		}
	}
}
