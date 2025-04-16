using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Linq;
using System.Reflection;

namespace Infrastructure.WebApi.Telemetry
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApiOpenTelemetry(this IServiceCollection services)
		{
			var entryProjectName = Assembly.GetEntryAssembly().FullName.Split('.').Last();

			services.AddOpenTelemetry()
				.ConfigureResource(resource => resource.AddService(entryProjectName))
				.WithTracing(tracing =>
				{
					tracing
						.AddHttpClientInstrumentation()
						.AddAspNetCoreInstrumentation();

					tracing.AddOtlpExporter();
				});
			return services;
		}

		public static IServiceCollection AddApiOpenTelemetry(this IServiceCollection services, string serviceName)
		{
			services.AddOpenTelemetry()
				.ConfigureResource(resource => resource.AddService(serviceName))
				.WithTracing(tracing =>
				{
					tracing
						.AddHttpClientInstrumentation()
						.AddAspNetCoreInstrumentation();

					tracing.AddOtlpExporter();
				});
			return services;
		}
	}
}
