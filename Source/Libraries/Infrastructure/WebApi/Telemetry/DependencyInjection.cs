using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Infrastructure.WebApi.Telemetry
{
	public static class DependencyInjection
	{
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
