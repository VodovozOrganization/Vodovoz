using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VodovozHealthCheck.ResponseWriter;

namespace VodovozHealthCheck
{
	public static class DependencyInjection
	{
		private static IHealthChecksBuilder _healthCheckBuilder;

		public static IApplicationBuilder ConfigureHealthCheckApplicationBuilder(this IApplicationBuilder app, IResponseWriter responseWriter = null)
		{
			var options = new HealthCheckOptions
			{
				ResponseWriter = (responseWriter ?? new JsonResponseWriter()).WriteResponse,
				AllowCachingResponses = false
			};

			app.UseHealthChecks("/health", options);

			return app;
		}

		public static IServiceCollection ConfigureHealthCheckService<T>(this IServiceCollection serviceCollection, bool needRegisterAsSingleton = false)
			where T : class, IHealthCheck
		{
			_healthCheckBuilder ??= serviceCollection.AddHealthChecks();

			_healthCheckBuilder.AddCheck<T>(nameof(T));

			if(needRegisterAsSingleton)
			{
				serviceCollection.AddSingleton<T>();
			}

			return serviceCollection;
		}
	}
}
