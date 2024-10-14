using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text;
using TrueMark.Api.Options;
using TrueMark.Api.Services.Authorization;

namespace TrueMark.Api;

public static class DependencyInjection
{
	public static IServiceCollection AddTrueMarkApi(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddTrueMarkApiSwagger();

		services.AddControllers();

		services.AddHttpClient()
			.AddAuthorization(configuration)
			.ConfigureTrueMarkApi(configuration);

		services.AddTrueMarkApiOpenTelemetry();

		services.AddMassTransit();

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
}
