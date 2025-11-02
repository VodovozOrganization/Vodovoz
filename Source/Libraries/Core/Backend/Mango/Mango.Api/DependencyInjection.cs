using Mango.Api.Validators;
using Mango.CallsPublishing;
using Mango.Core.Sign;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Mango.Api
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddMangoApi(this IServiceCollection services)
		{
			services.AddControllers();

			services
				.AddScoped<ISignGenerator, SignGenerator>()
				.AddScoped<IDefaultSignGenerator, DefaultSignGenerator>()
				.AddScoped<SignValidator>()
				.AddScoped<KeyValidator>()
				.AddMangoApiOpenTelemetry();

			services.AddCallsPublishing();

			return services;
		}

		public static IServiceCollection AddMangoApiOpenTelemetry(this IServiceCollection services)
		{
			services.AddOpenTelemetry()
				.ConfigureResource(resource => resource.AddService("mango.api.service"))
				.WithTracing(tracing =>
				{
					tracing
						.AddHttpClientInstrumentation()
						.AddAspNetCoreInstrumentation()
						.AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);

					tracing.AddOtlpExporter();
				});
			return services;
		}
	}
}
