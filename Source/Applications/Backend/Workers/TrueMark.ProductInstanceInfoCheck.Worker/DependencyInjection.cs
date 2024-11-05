using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Net.Http.Headers;

namespace TrueMark.ProductInstanceInfoCheck.Worker;
public static class DependencyInjection
{
	public static IServiceCollection AddProductInstanceInfoCheckWorker(this IServiceCollection services)
	{
		services.AddTrueMarkWorkerMassTransit();

		services
			.AddHttpClient<ProductInstanceInfoRequestConsumer>((serviceProvider, client) =>
			{
				var configuration = serviceProvider.GetRequiredService<IConfiguration>();
				var baseUri = configuration.GetValue<string>("ExternalTrueMarkBaseUrl")
					?? throw new InvalidOperationException("Не найдена настройка \"ExternalTrueMarkBaseUrl\" в конфигурации");

				client.BaseAddress = new Uri(baseUri);
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			})
			.ConfigurePrimaryHttpMessageHandler(() =>
			{
				return new SocketsHttpHandler
				{
					PooledConnectionLifetime = TimeSpan.FromMinutes(5)
				};
			})
			.SetHandlerLifetime(Timeout.InfiniteTimeSpan);

		services.AddTrueMarkWorkerOpenTelemetry();

		return services;
	}

	public static IServiceCollection AddTrueMarkWorkerOpenTelemetry(this IServiceCollection services)
	{
		services
			.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService("TrueMark.Api"))
			.WithTracing(tracing =>
			{
				tracing
					.AddHttpClientInstrumentation()
					.AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);

				tracing.AddOtlpExporter();
			});

		return services;
	}

	public static IServiceCollection AddTrueMarkWorkerMassTransit(this IServiceCollection services)
	{
		services.AddMassTransit(configuration =>
		{
			configuration.AddConsumers(typeof(DependencyInjection).Assembly);

			configuration.SetKebabCaseEndpointNameFormatter();

			configuration.UsingRabbitMq((busContext, configurator) =>
			{
				var appConfiguration = busContext.GetRequiredService<IConfiguration>();

				configurator.Host(
					appConfiguration.GetValue("RabbitMQ:Host", ""),
					port: 5671,
					virtualHost: appConfiguration.GetValue("RabbitMQ:VirtualHost", ""),
					h =>
					{
						h.Username(appConfiguration.GetValue("RabbitMQ:UserName", "")!);
						h.Password(appConfiguration.GetValue("RabbitMQ:Password", "")!);
						h.UseSsl(configureSsl =>
						{
							configureSsl.AllowPolicyErrors(System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch);
						});
					});

				configurator.ConfigureEndpoints(busContext);
			});
		});

		return services;
	}
}
