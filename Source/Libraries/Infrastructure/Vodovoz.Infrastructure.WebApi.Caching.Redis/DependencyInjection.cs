using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Presentation.WebApi.Caching;
using Vodovoz.Presentation.WebApi.Caching.Idempotency;

namespace Vodovoz.Infrastructure.WebApi.Caching.Redis
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
			=> services
				.AddRedisConnectionString(configuration)
				.AddCaching()
				.AddIdempotencyCaching();

		public static IServiceCollection AddRedisConnectionString(this IServiceCollection services, IConfiguration configuration)
			=> services
				.AddStackExchangeRedisCache(redisOptions =>
				{
					var connection = configuration.GetConnectionString("Redis");
					redisOptions.Configuration = connection;
				});

		public static IServiceCollection AddCaching(this IServiceCollection services)
			=> services
				.AddScoped(typeof(IRequestCacheService<>), typeof(RequestCacheService<>));

		public static IServiceCollection AddIdempotencyCaching(this IServiceCollection services)
			=> services
				.AddScoped(typeof(IIdempotencyRequestCacheService<>), typeof(IdempotencyRequestCacheService<>));
	}
}
