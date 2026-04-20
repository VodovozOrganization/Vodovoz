using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mango.Business.Interfaces;
using Mango.Contracts.V1.Options;
using Mango.Contracts.V1.Response;
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
		private readonly IMangoReferenceDataBuilder _referenceDataBuilder;

		public CallStatisticService(
			IMangoApiClient apiClient,
			ICallStatisticParser callStatisticParser,
			ICallStatisticRepository callStatisticRepository,
			ISyncStateRepository syncStateRepository,
			IOptions<SyncOptions> syncOptions,
			ILogger<CallStatisticService> logger,
			IMangoReferenceDataBuilder referenceDataBuilder	
		)
		{
			_apiClient = apiClient ?? throw new  ArgumentNullException(nameof(apiClient));
			_callStatisticParser = callStatisticParser ?? throw new  ArgumentNullException(nameof(callStatisticParser));
			_callStatisticRepository = callStatisticRepository ?? throw new  ArgumentNullException(nameof(callStatisticRepository));
			_syncStateRepository = syncStateRepository ?? throw new  ArgumentNullException(nameof(syncStateRepository));
			_syncOptions = syncOptions ?? throw new  ArgumentNullException(nameof(syncOptions));
			_logger = logger ?? throw new  ArgumentNullException(nameof(logger));
			_referenceDataBuilder = referenceDataBuilder ?? throw new  ArgumentNullException(nameof(referenceDataBuilder));
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
			
			var groupsResponse = await _apiClient.GetGroupsAsync(cancellationToken);
			
			if(groupsResponse.Groups == null || !groupsResponse.Groups.Any())
			{
				_logger.LogError("Wrong call groups information response");
				throw new OperationCanceledException("Wrong call groups information response");
			}
			
			var referenceData = _referenceDataBuilder.Build(groupsResponse);

			await Task.Delay(5000, cancellationToken);

			var callStatResponse = await _apiClient.GetCallsStatAsync(fromDate, toDate, cancellationToken);

			if(callStatResponse == null || string.IsNullOrEmpty(callStatResponse.Key))
			{
				_logger.LogError("Wrong call stats response");
				throw new OperationCanceledException("Wrong call stats response");
			}

			var calls = new CallsResponse();
			
			for(var i = 0; i < _syncOptions.Value.ResultRetryCount; i++)
			{
				await Task.Delay(TimeSpan.FromSeconds(_syncOptions.Value.ResultRetryDelaySeconds), cancellationToken);
				
				var callsResponse = await _apiClient.GetCallsAsync(callStatResponse.Key, cancellationToken);

				if(callsResponse.Result != 1000 && callsResponse.Status != "complete")
				{
					_logger.LogInformation("Result ={Result}, Status = {Status} waiting next attempt for request result status",
						callsResponse.Result,
						callsResponse.Status);
					
					continue;
				}

				calls =  callsResponse;
				break;
			}

			var parsed = _callStatisticParser.Parse(calls, referenceData);

			if(parsed.Count == 0)
			{
				_logger.LogInformation(
					"Parsed {ParsedCount} records",
					parsed.Count);
				return;
			}
			
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
			var sb = new StringBuilder();

			sb.Append(item.EntryId)
				.Append('|')
				.Append(item.StartTime.ToString("O"))
				.Append('|')
				.Append(item.EndTime.ToString("O"))
				.Append('|')
				.Append(item.AnswerTime?.ToString("O") ?? string.Empty)
				.Append('|')
				.Append(item.CallDirect)
				.Append('|')
				.Append(item.IsMissed);

			var raw = sb.ToString();

			using var sha = SHA256.Create();
			var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));

			return Convert.ToHexString(bytes);
		}
	}
}
