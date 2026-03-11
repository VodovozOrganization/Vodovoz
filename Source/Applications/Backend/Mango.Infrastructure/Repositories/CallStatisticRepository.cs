using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using Mango.Business.Interfaces;
using Mango.Contracts.V1.Options;
using Mango.Domain.Entity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mango.Infrastructure.Repositories
{
	public class CallStatisticRepository : ICallStatisticRepository
	{
		private readonly IOptions<DatabaseOptions> _options;
		private readonly ILogger<CallStatisticRepository> _logger;

		public CallStatisticRepository(
			IOptions<DatabaseOptions> options,
			ILogger<CallStatisticRepository> logger
		)
		{
			_options = options;
			_logger = logger;
		}

		public async Task InsertBatchAsync(
			IReadOnlyCollection<CallEntity> records,
			CancellationToken cancellationToken)
		{
			if(records == null || records.Count == 0)
				return;

			using var connection = new ClickHouseConnection(_options.Value.ConnectionString);
			await connection.OpenAsync(cancellationToken);

			var recordsList = new List<CallEntity>(records);
			var batchSize = Math.Max(1, _options.Value.InsertBatchSize);

			for(var offset = 0; offset < recordsList.Count; offset += batchSize)
			{
				var chunk = recordsList.GetRange(
					offset,
					Math.Min(batchSize, recordsList.Count - offset));

				using var command = connection.CreateCommand();

				var sql = $@"
					INSERT INTO {_options.Value.CallsTableName}
					(
    					RowHash,
    					StartTime,
    					EndTime,
    					AnswerTime,
    					Direction,
    					IsMissed,
					)
					VALUES ";

				var values = new List<string>();

				for(var i = 0; i < chunk.Count; i++)
				{
					values.Add(
						$"(@RowHash{i}, @Date{i}, @StartTime{i}, @EndTime{i}, @AnswerTime{i}, @Direction{i}, @IsMissed{i}, @ImportedAtUtc{i})");

					var item = chunk[i];

					command.Parameters.Add(new ClickHouseDbParameter
					{
						ParameterName = $"RowHash{i}",
						Value = item.UnicHash
					});

					command.Parameters.Add(new ClickHouseDbParameter
					{
						ParameterName = $"StartTime{i}",
						Value = item.StartTime
					});

					command.Parameters.Add(new ClickHouseDbParameter
					{
						ParameterName = $"EndTime{i}",
						Value = item.EndTime
					});

					command.Parameters.Add(new ClickHouseDbParameter
					{
						ParameterName = $"AnswerTime{i}",
						Value = (object?)item.AnswerTime ?? DBNull.Value
					});

					command.Parameters.Add(new ClickHouseDbParameter
					{
						ParameterName = $"CallDirect{i}",
						Value = item.CallDirect
					});

					command.Parameters.Add(new ClickHouseDbParameter
					{
						ParameterName = $"IsMissed{i}",
						Value = item.IsMissed ? 1 : 0
					});
				}

				command.CommandText = sql + string.Join(", ", values);

				await command.ExecuteNonQueryAsync(cancellationToken);
			}

			_logger.LogInformation(
				"Inserted {Count} analytics rows into {Table}",
				records.Count,
				_options.Value.CallsTableName);
		}
	}
}
