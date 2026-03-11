using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NLog.Config;
using VodovozHealthCheck.Logging;
using VodovozHealthCheck.Providers;
using VodovozHealthCheck.ResponseWriter;

namespace VodovozHealthCheck
{
	public static class DependencyInjection
	{
		private static IHealthChecksBuilder _healthCheckBuilder;
		private const string TagName = "vodovoz"; // фильтр (только кастомный хелзчек)

		/// <summary>
		/// Должен быть зарегистрирован в пайплайне до <c>UseEndpoints</c>,
		/// иначе настройки логгирования не будут распространятся на ендпоинты
		/// </summary>
		/// <param name="app"></param>
		/// <param name="responseWriter"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseVodovozHealthCheck(
			this IApplicationBuilder app,
			IResponseWriter responseWriter = null)
		{
			app.UseMiddleware<LoggingMiddleware>();

			var options = new HealthCheckOptions
			{
				ResponseWriter = async (ctx, report) =>
				{
					var writer = responseWriter ?? ctx.RequestServices.GetRequiredService<JsonResponseWriter>();
					await writer.WriteResponse(ctx, report);
				},
				AllowCachingResponses = false,
				Predicate = check => check.Tags.Contains(TagName)
			};

			app.UseHealthChecks("/health", options);

			return app;
		}
		public static IServiceCollection ConfigureHealthCheckService<THealthCheck, TServiceInfoProvider>(
			this IServiceCollection serviceCollection,
			bool needRegisterAsSingleton = false)
			where THealthCheck : class, IHealthCheck
			where TServiceInfoProvider : class, IHealthCheckServiceInfoProvider
		{
			_healthCheckBuilder ??= serviceCollection.AddHealthChecks();

			_healthCheckBuilder.AddCheck<THealthCheck>(nameof(THealthCheck), tags: new[] { TagName });

			if(needRegisterAsSingleton)
			{
				serviceCollection.AddSingleton<THealthCheck>();
			}

			serviceCollection
				.AddSingleton<JsonResponseWriter>()
				.AddSingleton<IHealthCheckServiceInfoProvider, TServiceInfoProvider>();

			ConfigurationItemFactory
				.Default
				.Filters
				.RegisterDefinition(LoggingConstants.HealthCheckSuppressLogFilterName, typeof(LoggingFilter));

			return serviceCollection;
		}
	}
}
