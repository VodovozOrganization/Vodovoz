using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using TrueMark.Api.Controllers;
using TrueMark.Api.Options;
using TrueMark.Api.Services.Authorization;
using Microsoft.Extensions.Options;

namespace TrueMark.Api;

public static class DependencyInjection
{
	public static IServiceCollection AddTrueMarkApi(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddTrueMarkApiSwagger();

		services.AddControllers();

		services
			.AddHttpClient<TrueMarkApiController>((serviceProvider, client) =>
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
			.AddTrueMarkApiOpenTelemetry(configuration)
			.AddMassTransit();

		return services;
	}

	public static IServiceCollection AddTrueMarkApiOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
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

				tracing.AddOtlpExporter(exporter =>
				{
					exporter.Endpoint = new Uri(configuration.GetSection("OtlpExporter").GetValue<string>("Endpoint"));
					exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
				});
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
}
