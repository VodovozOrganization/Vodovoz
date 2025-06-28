using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using TrueMark.Api.Options;
using TrueMark.Api.Services.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.HttpLogging;

namespace TrueMark.Api;

public static class DependencyInjection
{
	public static IServiceCollection AddTrueMarkApi(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddHttpLogging(logging =>
		{
			logging.LoggingFields = HttpLoggingFields.All;
			logging.MediaTypeOptions.AddText("application/json", Encoding.UTF8);
			logging.RequestBodyLogLimit = 4096;
			logging.ResponseBodyLogLimit = 4096;
		});

		services.AddTrueMarkApiSwagger();

		services.AddControllers();

		services
			.AddHttpClient("truemark-external", (serviceProvider, client) =>
			{
				var trueMarkOptions = serviceProvider.GetRequiredService<IOptions<TrueMarkApiOptions>>();

				client.BaseAddress = new Uri(trueMarkOptions.Value.ExternalTrueMarkBaseUrl);
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

		services
			.AddAuthorization(configuration)
			.ConfigureTrueMarkApi(configuration)
			.AddTrueMarkApiOpenTelemetry()
			.AddTrueMarkApiMassTransit();

		return services;
	}

	public static IServiceCollection AddTrueMarkApiOpenTelemetry(this IServiceCollection services)
	{
		services
			.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService("TrueMark.Api"))
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

	public static IServiceCollection AddTrueMarkApiSwagger(this IServiceCollection services) => services
		.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo { Title = $"{nameof(TrueMark)}.{nameof(Api)}", Version = "v1" });
		});

	public static IServiceCollection AddAuthorization(this IServiceCollection services, IConfiguration configuration)
	{
		services
			.AddSingleton<IAuthorizationService, AuthorizationService>()
			.AddAuthorization()
			.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateLifetime = false,
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection(nameof(TrueMarkApiOptions)).GetValue<string>("InternalSecurityKey")))
				};
			});

		return services;
	}

	public static IServiceCollection ConfigureTrueMarkApi(this IServiceCollection services, IConfiguration configuration) => services
		.Configure<TrueMarkApiOptions>(configuration.GetSection(nameof(TrueMarkApiOptions)));

	public static IServiceCollection AddTrueMarkApiMassTransit(this IServiceCollection services)
	{
		services.AddMassTransit(configuration =>
		{
			configuration.UsingRabbitMq((busContext, rabbitMqBusConfig) =>
			{
				var appConfiguration = busContext.GetRequiredService<IConfiguration>();

				rabbitMqBusConfig.Host(
					host: appConfiguration.GetValue("RabbitMQ:Host", ""),
					port: 5671,
					virtualHost: appConfiguration.GetValue("RabbitMQ:VirtualHost", ""),
					rabbitMqHostConfig =>
					{
						rabbitMqHostConfig.Username(appConfiguration.GetValue("RabbitMQ:UserName", "")!);
						rabbitMqHostConfig.Password(appConfiguration.GetValue("RabbitMQ:Password", "")!);
						rabbitMqHostConfig.UseSsl(configureSsl =>
						{
							configureSsl.AllowPolicyErrors(System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch);
						});
					});
				rabbitMqBusConfig.ConfigureEndpoints(busContext);
			});
		});

		return services;
	}
}
