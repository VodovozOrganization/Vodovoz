using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Vodovoz.Presentation.WebApi.Caching.Idempotency;

namespace Vodovoz.Infrastructure.WebApi.Caching.Garnet
{
	/// <summary>
	/// Хранилище записей идемпотентных запросов на Garnet (протокол Redis)
	/// </summary>
	internal sealed class GarnetIdempotencyStore : IIdempotencyStore
	{
		private readonly ILogger<GarnetIdempotencyStore> _logger;
		private readonly IConnectionMultiplexer _connectionMultiplexer;

		public GarnetIdempotencyStore(
			ILogger<GarnetIdempotencyStore> logger,
			IConnectionMultiplexer connectionMultiplexer)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
		}

		/// <inheritdoc/>
		public async Task<IdempotencyBeginResult> TryBeginAsync(string key, IdempotencyRecord markerRecord, TimeSpan markerLifetime)
		{
			try
			{
				var isSet = await _connectionMultiplexer
					.GetDatabase()
					.StringSetAsync(key, Serialize(markerRecord), markerLifetime, When.NotExists);

				return isSet ? IdempotencyBeginResult.Started : IdempotencyBeginResult.AlreadyExists;
			}
			catch(Exception ex)
			{
				_logger.LogWarning(ex, "Garnet недоступен, не удалось поставить метку обработки по ключу {IdempotencyStorageKey}", key);
				return IdempotencyBeginResult.Unavailable;
			}
		}

		/// <inheritdoc/>
		public async Task<IdempotencyReadResult> GetAsync(string key)
		{
			RedisValue value;

			try
			{
				value = await _connectionMultiplexer
					.GetDatabase()
					.StringGetAsync(key);
			}
			catch(Exception ex)
			{
				_logger.LogWarning(ex, "Garnet недоступен, не удалось прочитать запись по ключу {IdempotencyStorageKey}", key);
				return IdempotencyReadResult.Unavailable;
			}

			if(value.IsNullOrEmpty)
			{
				return IdempotencyReadResult.NotFound;
			}

			IdempotencyRecord record;

			try
			{
				record = JsonSerializer.Deserialize<IdempotencyRecord>((string)value);
			}
			catch(Exception ex)
			{
				_logger.LogWarning(ex, "Не удалось разобрать запись Garnet по ключу {IdempotencyStorageKey}", key);
				return IdempotencyReadResult.Unavailable;
			}

			if(record is null || record.FormatVersion != IdempotencyRecord.CurrentFormatVersion)
			{
				_logger.LogWarning(
					"Запись Garnet по ключу {IdempotencyStorageKey} имеет неизвестную версию формата {FormatVersion}",
					key,
					record?.FormatVersion);

				return IdempotencyReadResult.Unavailable;
			}

			return IdempotencyReadResult.Found(record);
		}

		/// <inheritdoc/>
		public async Task<bool> SaveCompletedAsync(string key, IdempotencyRecord record, TimeSpan lifetime)
		{
			try
			{
				return await _connectionMultiplexer
					.GetDatabase()
					.StringSetAsync(key, Serialize(record), lifetime, When.Always);
			}
			catch(Exception ex)
			{
				_logger.LogWarning(ex, "Garnet недоступен, не удалось сохранить ответ по ключу {IdempotencyStorageKey}", key);
				return false;
			}
		}

		/// <inheritdoc/>
		public async Task<bool> RemoveAsync(string key)
		{
			try
			{
				await _connectionMultiplexer
					.GetDatabase()
					.KeyDeleteAsync(key);

				return true;
			}
			catch(Exception ex)
			{
				_logger.LogWarning(ex, "Garnet недоступен, не удалось снять метку обработки по ключу {IdempotencyStorageKey}", key);
				return false;
			}
		}

		private static string Serialize(IdempotencyRecord record) =>
			JsonSerializer.Serialize(record);
	}
}
