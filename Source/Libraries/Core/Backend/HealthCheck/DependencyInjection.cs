using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VodovozHealthCheck.Utils.ResponseWriter;

namespace VodovozHealthCheck
{
	public static class DependencyInjection
	{
		private static IHealthChecksBuilder _healthCheckBuilder;
		private static HealthCheckOptions _healthCheckOptions;
		//private static HealthCheckOptions _options;
		//private static IResponseWriter _responseWriter;



		public static IApplicationBuilder ConfigureHealthCheckApplicationBuilder(this IApplicationBuilder app, IResponseWriter responseWriter = null)
		{
			//_responseWriter = responseWriter;
			var options = new HealthCheckOptions
			{
				ResponseWriter = (responseWriter ?? new JsonResponseWriter()).WriteResponse,
				AllowCachingResponses = false
			};

			/*var v =*/
			app.UseHealthChecks("/health", options);

			//var v1 = app.ApplicationServices.GetService(typeof(HealthCheckService));



			return app;
		}

		//public static void Test2Model(IOptions<HealthCheckOptions> options)
		//{
		//	var v = options.Value;
		//}


		public static IServiceCollection ConfigureHealthCheckService<T>(this IServiceCollection serviceCollection, bool needRegisterAsSingletone = false) where T : class, IHealthCheck
		{
			_healthCheckBuilder ??= serviceCollection.AddHealthChecks();

			_healthCheckBuilder.AddCheck<T>(nameof(T));

			//_healthCheckBuilder.Services.AddSingleton<T>();

			if(needRegisterAsSingletone)
			{
				serviceCollection.AddSingleton<T>();
			}

			//serviceCollection.AddSingleton<UrlExistsChecker>();
			//serviceCollection.AddSingleton<JsonResponseWriter>();

			return serviceCollection;
		}

		//public static IServiceCollection ConfigureUrlExistsChecker<T>(this IServiceCollection serviceCollection) where T : class, IHealthCheck
		//{
		//	serviceCollection.AddSingleton<UrlExistsChecker>();

		//	return serviceCollection;
		//}

		//public static IServiceCollection ConfigureUrlExistsChecker<T>(this IServiceCollection serviceCollection) where T : class, IHealthCheck
		//{
		//	serviceCollection.AddSingleton<UrlExistsChecker>();

		//	return serviceCollection;
		//}

		//public static IHealthChecksBuilder UseUrlExistsChecker<T>(this IHealthChecksBuilder serviceCollection) where T : class
		//{

		//}
	}
}
