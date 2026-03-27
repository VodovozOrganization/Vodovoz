using QS.Project.DB;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Publisher.Cache
{
	/// <summary>
	/// Реализация кэша уведомлений на основе Redis,
	/// предотвращающая повторную отправку одного и того же события.
	/// </summary>
	public class RedisCustomerNotificationCache : ICustomerNotificationCache
	{
		private readonly IDatabase _db;
		private readonly TimeSpan _ttl = TimeSpan.FromDays(1);
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		/// <summary>
		/// Создаёт экземпляр кэша уведомлений Redis.
		/// </summary>
		/// <param name="redis">Подключение к Redis.</param>
		public RedisCustomerNotificationCache(IConnectionMultiplexer redis, IDatabaseConnectionSettings databaseConnectionSettings)
		{
			_db = redis?.GetDatabase() ?? throw new ArgumentNullException(nameof(redis));
			_databaseConnectionSettings = databaseConnectionSettings ?? throw new ArgumentNullException(nameof(databaseConnectionSettings));
		}

		/// <summary>
		/// Пытается пометить уведомление как отправленное.
		/// Возвращает true, если это первый раз, иначе false.
		/// </summary>
		public async Task<bool> TryMarkAsFirstSentAsync(int onlineOrderId, CustomerNotificationEventType eventType)
		{
			if(_ttl <= TimeSpan.Zero)
			{
				throw new ArgumentException("TTL должен быть положительным", nameof(_ttl));
			}

			var key = $"customer-notification:{_databaseConnectionSettings.DatabaseName}:[{nameof(onlineOrderId)}:{onlineOrderId}]:[{nameof(eventType)}:{eventType}]";

			// Lua-скрипт: если ключ не существует, создаем его с TTL и возвращаем 1, иначе 0
			const string lua = @"
                if redis.call('EXISTS', KEYS[1]) == 0 then
                    redis.call('SET', KEYS[1], 'sent', 'EX', ARGV[1])
                    return 1
                else
                    return 0
                end";

			var result = (int)await _db.ScriptEvaluateAsync
			(
				lua,
				keys: new RedisKey[] { key },
				values: new RedisValue[] { (long)_ttl.TotalSeconds }
			).ConfigureAwait(false);

			return result == 1;
		}
	}
}
