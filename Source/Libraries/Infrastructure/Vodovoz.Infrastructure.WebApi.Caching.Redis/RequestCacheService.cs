using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Presentation.WebApi.Caching;

namespace Vodovoz.Infrastructure.WebApi.Caching.Redis
{
	internal class RequestCacheService<T> : IRequestCacheService<T>
		where T : class
	{
		private readonly IDistributedCache _distributedCache;

		public RequestCacheService(IDistributedCache distributedCache)
		{
			_distributedCache = distributedCache
				?? throw new ArgumentNullException(nameof(distributedCache));
		}

		public async Task<ResponseInfo<T>> GetCachedResponse(string path, CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
			}

			var responseInfoSerializad = await _distributedCache.GetStringAsync(path, cancellationToken);

			if(string.IsNullOrWhiteSpace(responseInfoSerializad))
			{
				return null;
			}

			return JsonSerializer.Deserialize<ResponseInfo<T>>(responseInfoSerializad);
		}

		public async Task CacheResponse(string path, ResponseInfo<T> response, TimeSpan expirationTime, CancellationToken cancellationToken)
			=> await _distributedCache.SetStringAsync(
				path,
				JsonSerializer.Serialize(response),
				new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = expirationTime
				},
				cancellationToken);
	}
}
