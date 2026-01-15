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
		private const string TagName = "vodovoz"; // фильтр (только кастомный хелзчек)

		public static IApplicationBuilder UseVodovozHealthCheck(
			this IApplicationBuilder app,
			IResponseWriter responseWriter = null)
		{
			var options = new HealthCheckOptions
			{
				ResponseWriter = (responseWriter ?? new JsonResponseWriter()).WriteResponse,
				AllowCachingResponses = false,
				Predicate = check => check.Tags.Contains(TagName)
			};

			app.UseHealthChecks("/health", options);

			return app;
		}

		public static IServiceCollection ConfigureHealthCheckService<T>(
			this IServiceCollection serviceCollection,
			bool needRegisterAsSingleton = false)
			where T : class, IHealthCheck
		{
			_healthCheckBuilder ??= serviceCollection.AddHealthChecks();

			_healthCheckBuilder.AddCheck<T>(nameof(T), tags: new[] { TagName });

			if(needRegisterAsSingleton)
			{
				serviceCollection.AddSingleton<T>();
			}

			return serviceCollection;
		}
	}
}
