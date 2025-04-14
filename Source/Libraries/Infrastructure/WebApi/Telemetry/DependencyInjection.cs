using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Linq;
using System.Reflection;

namespace Telemetry
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
	}
}
