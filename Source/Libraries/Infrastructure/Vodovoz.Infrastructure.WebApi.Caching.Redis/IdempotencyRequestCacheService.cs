using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Presentation.WebApi.Caching;
using Vodovoz.Presentation.WebApi.Caching.Idempotency;

namespace Vodovoz.Infrastructure.WebApi.Caching.Redis
{
	internal class IdempotencyRequestCacheService<T> : IIdempotencyRequestCacheService<T>
		where T : class
	{
		private readonly IDistributedCache _distributedCache;

		public IdempotencyRequestCacheService(IDistributedCache distributedCache)
		{
			_distributedCache = distributedCache
				?? throw new ArgumentNullException(nameof(distributedCache));
		}

		public async Task<ResponseInfo<T>> GetCachedResponse(string path, Guid requestId, CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
			}

			var responseInfoSerializad = await _distributedCache.GetStringAsync($"{path}-{requestId}", cancellationToken);

			if(string.IsNullOrWhiteSpace(responseInfoSerializad))
			{
				return null;
			}

			return JsonSerializer.Deserialize<ResponseInfo<T>>(responseInfoSerializad);
		}

		public async Task CacheResponse(string path, Guid requestId, ResponseInfo<T> response, TimeSpan expirationTime, CancellationToken cancellationToken)
			=> await _distributedCache.SetStringAsync(
				$"{path}-{requestId}",
				JsonSerializer.Serialize(response),
				new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = expirationTime
				},
				cancellationToken);
	}
}
