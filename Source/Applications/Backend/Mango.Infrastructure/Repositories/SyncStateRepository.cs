using System;
using System.Threading;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using Mango.Business.Interfaces;
using Mango.Contracts.V1.Options;
using Mango.Domain.Entity;
using Microsoft.Extensions.Options;

namespace Mango.Infrastructure.Repositories
{
	public class SyncStateRepository : ISyncStateRepository
	{
		private readonly IOptions<DatabaseOptions> _options;

		public SyncStateRepository(IOptions<DatabaseOptions> options)
		{
			_options = options;
		}
		
		public async Task<SyncStateEntity> GetAsync(string source, CancellationToken cancellationToken)
        {
            using var connection = new ClickHouseConnection(_options.Value.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = $@"
SELECT
    Source,
    LastProcessedAtUtc,
    UpdatedAtUtc
FROM {_options.Value.SyncStateTableName}
WHERE Source = @Source
ORDER BY UpdatedAtUtc DESC
LIMIT 1";

            command.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "Source",
                Value = source
            });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                return new SyncStateEntity()
                {
                    Source = reader.GetString(0),
                    LastProcessedDate = reader.GetDateTime(1),
                    UpdatedAtDate = reader.GetDateTime(2)
                };
            }

            return new SyncStateEntity()
            {
                Source = source,
                LastProcessedDate = DateTime.Now.AddHours(-1),
                UpdatedAtDate = DateTime.Now
            };
        }

        public async Task SaveAsync(
            string source,
            DateTime lastProcessedAtUtc,
            CancellationToken cancellationToken)
        {
	        await using var connection = new ClickHouseConnection(_options.Value.ConnectionString);
	        
	        await connection.OpenAsync(cancellationToken);

	        await using var command = connection.CreateCommand();
            command.CommandText = $@"
			INSERT INTO {_options.Value.SyncStateTableName}
			(
    			Source,
    			LastProcessedAtUtc,
    			UpdatedAtUtc
			)
			VALUES
			(
    			@Source,
   				@LastProcessedAtUtc,
    			@UpdatedAtUtc
			)";

            command.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "Source",
                Value = source
            });

            command.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "LastProcessedAtUtc",
                Value = lastProcessedAtUtc
            });

            command.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "UpdatedAtUtc",
                Value = DateTime.UtcNow
            });

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
