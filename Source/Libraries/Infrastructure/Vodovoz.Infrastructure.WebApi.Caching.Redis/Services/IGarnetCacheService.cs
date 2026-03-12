using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Vodovoz.Infrastructure.WebApi.Caching.Redis.Services
{
	/// <summary>
	/// Контракт кэш сервиса, использующий команды redis/garnet
	/// </summary>
	public interface IGarnetCacheService
	{
		/// <summary>
		/// Get the value of key. If the key does not exist the special value nil is returned.
		/// An error is returned if the value stored at key is not a string, because GET only handles string values.
		/// </summary>
		/// <param name="key">The key of the string.</param>
		/// <param name="flags">The flags to use for this operation.</param>
		/// <returns>The value of key, or nil when key does not exist.</returns>
		/// <remarks>https://redis.io/commands/get</remarks>
		Task<string> GetStringAsync(string key, CommandFlags flags = CommandFlags.None);
		/// <summary>
		/// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type.
		/// </summary>
		/// <param name="key">The key of the string.</param>
		/// <param name="value">The value to set.</param>
		/// <param name="expiry">The expiry to set.</param>
		/// <param name="when">Which condition to set the value under (detaults to always).</param>
		/// <returns>True if the string was set, false otherwise.</returns>
		/// <remarks>https://redis.io/commands/set</remarks>
		Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null, When when = When.Always);
	}
}
