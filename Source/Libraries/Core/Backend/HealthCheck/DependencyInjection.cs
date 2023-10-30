using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace VodovozHealthCheck
{
	public static class DependencyInjection
	{
		private static IHealthChecksBuilder _healthCheckBuilder;
		
		public  static IApplicationBuilder ConfigureHealthCheckApplicationBuilder(this IApplicationBuilder app)
		{
			app.UseHealthChecks("/health", new HealthCheckOptions
			{
				ResponseWriter = JsonResponseWriter.WriteResponse,
				AllowCachingResponses = false
			});

			return app;
		}

		public static IServiceCollection ConfigureHealthCheckService<T>(this IServiceCollection serviceCollection, bool needRegisterAsSingletone) where T : class, IHealthCheck
		{
			_healthCheckBuilder ??= serviceCollection.AddHealthChecks();

			_healthCheckBuilder.AddCheck<T>(nameof(T));

			if(needRegisterAsSingletone)
			{
				serviceCollection.AddSingleton<T>();
			}

			return serviceCollection;
		}
	}
}
