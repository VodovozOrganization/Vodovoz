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
    LastProcessedDate,
    UpdatedAtDate
FROM {_options.Value.SyncStateTableName}
WHERE Source = @Source
ORDER BY UpdatedAtDate DESC
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
                LastProcessedDate = DateTime.Now,
                UpdatedAtDate = DateTime.Now
            };
        }

        public async Task SaveAsync(
            string source,
            DateTime lastProcessedDate,
            CancellationToken cancellationToken)
        {
	        await using var connection = new ClickHouseConnection(_options.Value.ConnectionString);
	        
	        lastProcessedDate = lastProcessedDate.AddHours(-3);
	        
	        await connection.OpenAsync(cancellationToken);

	        await using var command = connection.CreateCommand();
            command.CommandText = $@"
			INSERT INTO {_options.Value.SyncStateTableName}
			(
    			Source,
    			LastProcessedDate,
    			UpdatedAtDate
			)
			VALUES
			(
    			@Source,
   				@LastProcessedDate,
    			@UpdatedAtDate
			)";

            command.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "Source",
                Value = source
            });

            command.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "LastProcessedDate",
                Value = lastProcessedDate
            });

            command.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "UpdatedAtDate",
                Value = DateTime.Now
            });

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
