using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mango.Business.Interfaces;
using Mango.Contracts.V1.Options;
using Mango.Domain.Entity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mango.Business.Services
{
	public class CallStatisticService : ICallStatisticService
	{
		private const string SourceName = "MangoCallsAnalytics";

		private readonly IMangoApiClient _apiClient;
		private readonly ICallStatisticParser _callStatisticParser;
		private readonly ICallStatisticRepository _callStatisticRepository;
		private readonly ISyncStateRepository _syncStateRepository;
		private readonly IOptions<SyncOptions> _syncOptions;
		private readonly ILogger<CallStatisticService> _logger;

		public CallStatisticService(
			IMangoApiClient apiClient,
			ICallStatisticParser callStatisticParser,
			ICallStatisticRepository callStatisticRepository,
			ISyncStateRepository syncStateRepository,
			IOptions<SyncOptions> syncOptions,
			ILogger<CallStatisticService> logger
		)
		{
			_apiClient = apiClient;
			_callStatisticParser = callStatisticParser;
			_callStatisticRepository = callStatisticRepository;
			_syncStateRepository = syncStateRepository;
			_syncOptions = syncOptions;
			_logger = logger;
		}

		public async Task LoadDataAsync(CancellationToken cancellationToken)
		{
			var state = await _syncStateRepository.GetAsync(SourceName, cancellationToken);

			var dateTimeNow = DateTime.Now;
			var fromDate = state.LastProcessedDate.AddHours(3);
			var toDate = fromDate.AddMinutes(_syncOptions.Value.RangeMinutes);
			
			if(toDate > dateTimeNow)
			{
				toDate = dateTimeNow;
				fromDate = toDate.AddMinutes(-_syncOptions.Value.RangeMinutes);
			}
			
			_logger.LogInformation(
				"Starting sync. FromDate={fromDate}, ToDate={toDate}",
				fromDate,
				toDate);
			
			var rawJson = await _apiClient.GetCallsRawJsonAsync(
				fromDate,
				toDate,
				cancellationToken);

			var parsed = _callStatisticParser.Parse(rawJson);

			foreach(var item in parsed)
			{
				item.UnicHash = BuildHash(item);
			}

			var deduplicated = parsed
				.GroupBy(x => x.UnicHash)
				.Select(g => g.First())
				.ToList();

			var callStatistics = await _callStatisticRepository.GetCallEntitiesAsync(fromDate, toDate, cancellationToken);
			
			var existingHashes = callStatistics
				.Select(x => x.UnicHash)
				.ToHashSet();

			var toInsert = deduplicated
				.Where(x => !existingHashes.Contains(x.UnicHash))
				.ToList();
			
			_logger.LogInformation(
				"Parsed {ParsedCount} records, deduplicated to {DeduplicatedCount}",
				parsed.Count,
				toInsert.Count);
			
			if(toInsert.Count > 0)
			{
				await _callStatisticRepository.InsertBatchAsync(toInsert, cancellationToken);
			}

			await _syncStateRepository.SaveAsync(SourceName, toDate, cancellationToken);

			_logger.LogInformation("Sync completed.");
		}

		private static string BuildHash(CallEntity item)
		{
			var raw = string.Join("|", 
				item.EntryId,
				item.StartTime.ToString("O"),
				item.EndTime.ToString("O"),
				item.AnswerTime?.ToString("O") ?? string.Empty,
				item.CallDirect,
				item.IsMissed);

			using var sha = SHA256.Create();
			var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));

			return Convert.ToHexString(bytes);
		}
	}
}
