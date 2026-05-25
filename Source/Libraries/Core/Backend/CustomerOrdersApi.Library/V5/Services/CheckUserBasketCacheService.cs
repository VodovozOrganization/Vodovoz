using System;
using System.Text.Json;
using System.Threading.Tasks;
using CustomerOrders.Contracts.V5.Carts;
using CustomerOrdersApi.Library.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vodovoz.Infrastructure.WebApi.Caching.Redis;

namespace CustomerOrdersApi.Library.V5.Services
{
	/// <inheritdoc/>
	public class CheckUserBasketCacheService : ICheckUserBasketCacheService
	{
		private readonly ILogger<CheckUserBasketCacheService> _logger;
		private readonly IGarnetCacheService _garnetCacheService;
		private readonly IOptionsSnapshot<CacheOptions> _cacheOptions;

		public CheckUserBasketCacheService(
			ILogger<CheckUserBasketCacheService> logger,
			IGarnetCacheService garnetCacheService,
			IOptionsSnapshot<CacheOptions> cacheOptions)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_garnetCacheService = garnetCacheService ?? throw new ArgumentNullException(nameof(garnetCacheService));
			_cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
		}
		
		/// <inheritdoc/>
		public async Task<bool> TryCacheVerificationAsync(CheckUsersBasketCachedValue data)
		{
			_logger.LogInformation("Пишем в кэш проверку в корзине {CheckId}", data.CheckId);
			
			try
			{
				var stringData = JsonSerializer.Serialize(data);
				return await _garnetCacheService.SetStringAsync(
					data.CheckId.ToString(),
					stringData,
					TimeSpan.FromMinutes(_cacheOptions.Value.UserBasketCheckExpireTimeInMinutes));
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Произошла ошибка при записи в кэш результатов проверки");
			}
			
			return false;
		}
		
		/// <inheritdoc/>
		public async Task<CheckUsersBasketCachedValue> GetCachedVerificationAsync(Guid? checkId)
		{
			if(checkId is null)
			{
				return null;
			}
			
			_logger.LogInformation("Достаем из кэша проверку в корзине {CheckId}", checkId);
			
			try
			{
				var stringData = await _garnetCacheService.GetStringAsync(checkId.Value.ToString());
				return string.IsNullOrEmpty(stringData) ? null : JsonSerializer.Deserialize<CheckUsersBasketCachedValue>(stringData);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Произошла ошибка при получении результатов проверки {CheckId} из кэша", checkId);
			}
			
			return null;
		}
	}
}
