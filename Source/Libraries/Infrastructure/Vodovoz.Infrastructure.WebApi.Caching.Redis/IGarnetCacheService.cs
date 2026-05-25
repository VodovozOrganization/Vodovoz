using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Vodovoz.Infrastructure.WebApi.Caching.Redis
{
	public interface IGarnetCacheService
	{
		Task<string> GetStringAsync(string key, CommandFlags flags = CommandFlags.None);
		Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null, When when = When.Always);
	}
}
