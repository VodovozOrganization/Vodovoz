using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Vodovoz.Infrastructure.WebApi.Caching.Redis
{
	public class GarnetCacheService : IGarnetCacheService
	{
		private readonly IConnectionMultiplexer _connectionMultiplexer;

		public GarnetCacheService(
			IConnectionMultiplexer connectionMultiplexer
		)
		{
			_connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
		}
		
		private IDatabase Database => _connectionMultiplexer.GetDatabase();
		
		public async Task<string> GetStringAsync(string key, CommandFlags flags = CommandFlags.None)
		{
			return await Database.StringGetAsync(key, flags);
		}
		
		public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null, When when = When.Always)
		{
			return await Database.StringSetAsync(key, value, expiry, when);
		}
	}
}
