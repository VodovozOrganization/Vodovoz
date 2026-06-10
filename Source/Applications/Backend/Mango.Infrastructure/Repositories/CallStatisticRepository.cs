using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using Mango.Business.Interfaces;
using Mango.Contracts.V1.Options;
using Mango.Domain.Entity;
using Mango.Domain.Enums;
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
			ILogger<CallStatisticRepository> logger)
		{
			_options = options;
			_logger = logger;
		}

		public async Task<IEnumerable<CallEntity>> GetCallEntitiesAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
		{
			await using var connection = new ClickHouseConnection(_options.Value.ConnectionString);
			await connection.OpenAsync(cancellationToken);

			await using var command = connection.CreateCommand();
			command.CommandText = $@"
SELECT
    UnicHash,
    EntryId,
    GroupName,
    StartTime,
    EndTime,
    AnswerTime,
    Direction,
    IsMissed
FROM {_options.Value.CallsTableName}
WHERE StartTime >= {{StartDate:DateTime}}
  AND StartTime <= {{EndDate:DateTime}}
ORDER BY StartTime";

			command.Parameters.Add(new ClickHouseDbParameter
			{
				ParameterName = "StartDate",
				Value = startDate
			});

			command.Parameters.Add(new ClickHouseDbParameter
			{
				ParameterName = "EndDate",
				Value = endDate
			});

			var result = new List<CallEntity>();

			await using var reader = await command.ExecuteReaderAsync(cancellationToken);

			while (await reader.ReadAsync(cancellationToken))
			{
				var directionRaw = reader.GetValue(6)?.ToString();

				result.Add(new CallEntity
				{
					UnicHash = reader.GetString(0),
					EntryId = reader.GetString(1),
					GroupName = reader.IsDBNull(2) ? null : reader.GetString(2),
					StartTime = reader.GetDateTime(3),
					EndTime = reader.GetDateTime(4),
					AnswerTime = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
					CallDirect = ParseCallDirect(directionRaw),
					IsMissed = !reader.IsDBNull(7) && Convert.ToByte(reader.GetValue(7)) == 1
				});
			}

			return result;
		}

		public async Task InsertBatchAsync(
			IReadOnlyCollection<CallEntity> records,
			CancellationToken cancellationToken)
		{
			if(records == null || records.Count == 0)
			{
				return;
			}

			await using var connection = new ClickHouseConnection(_options.Value.ConnectionString);
			await connection.OpenAsync(cancellationToken);

			var recordsList = records.ToList();
			var batchSize = Math.Max(1, _options.Value.InsertBatchSize);

			for(var offset = 0; offset < recordsList.Count; offset += batchSize)
			{
				var chunk = recordsList
					.Skip(offset)
					.Take(batchSize)
					.ToList();

				await using var command = connection.CreateCommand();

				var values = new List<string>();

				for(var i = 0; i < chunk.Count; i++)
				{
					values.Add($@"
(
	{{UnicHash{i}:String}},
	{{EntryId{i}:String}},
	{{GroupName{i}:Nullable(String)}},
	{{StartTime{i}:DateTime}},
	{{EndTime{i}:DateTime}},
	{{AnswerTime{i}:Nullable(DateTime)}},
	CAST({{Direction{i}:String}} AS Enum8('None' = 0, 'Inbound' = 1, 'Outbound' = 2, 'Inner' = 3)),
	{{IsMissed{i}:UInt8}}
)");

					var item = chunk[i];

					command.Parameters.Add(new ClickHouseDbParameter
					{
						ParameterName = $"UnicHash{i}",
						Value = item.UnicHash
					});

					command.Parameters.Add(new ClickHouseDbParameter
					{
						ParameterName = $"EntryId{i}",
						Value = item.EntryId
					});

					command.Parameters.Add(new ClickHouseDbParameter
					{
						ParameterName = $"GroupName{i}",
						Value = (object?)item.GroupName ?? DBNull.Value
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
						ParameterName = $"Direction{i}",
						Value = item.CallDirect.ToString()
					});

					command.Parameters.Add(new ClickHouseDbParameter
					{
						ParameterName = $"IsMissed{i}",
						Value = item.IsMissed ? (byte)1 : (byte)0
					});
				}

				command.CommandText = $@"
INSERT INTO {_options.Value.CallsTableName}
(
	UnicHash,
	EntryId,
	GroupName,
	StartTime,
	EndTime,
	AnswerTime,
	Direction,
	IsMissed
)
VALUES
{string.Join(",\n", values)}";

				await command.ExecuteNonQueryAsync(cancellationToken);
			}

			_logger.LogInformation(
				"Inserted {Count} analytics rows into {Table}",
				records.Count,
				_options.Value.CallsTableName);
		}
		
		private static CallDirect ParseCallDirect(string? value)
		{
			return value switch
			{
				"Inbound" => CallDirect.Inbound,
				"Outbound" => CallDirect.Outbound,
				"Inner" => CallDirect.Inner,
				_ => CallDirect.None
			};
		}
	}
}
