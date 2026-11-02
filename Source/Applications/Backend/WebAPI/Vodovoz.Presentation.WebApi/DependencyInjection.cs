using MassTransit.Internals;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QS.Project.DB;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Reflection;
using Vodovoz.Presentation.WebApi.Caching.Idempotency;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.Presentation.WebApi.Idempotency;
using Vodovoz.Presentation.WebApi.Options;
using Vodovoz.Presentation.WebApi.Security;
using Vodovoz.Presentation.WebApi.Security.OnlyOneSession;
using Vodovoz.Settings.WebApi;

namespace Vodovoz.Presentation.WebApi
{
	public static class DependencyInjection
	{
		private static readonly string _securityOptionsConfigurationKey = "Security";

		private static bool _authorizationAdded = false;

		public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.Configure<SecurityOptions>(so =>
				{
					configuration.Bind(_securityOptionsConfigurationKey, so);
				})
				.ConfigureOptions<ConfigureJwtBearerOptions>()
				.ConfigureOptions<ConfigureIdentityOptions>();

			var builder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer();

			return services;
		}

		public static IServiceCollection AddOnlyOneSessionRestriction(this IServiceCollection services)
			=> services
				.AddAuthorizationIfNeeded()
				.AddSingleton<IAuthorizationHandler, OnlyOneSessionAuthorizationHandler>()
				.AddSingleton<IAuthorizationPolicyProvider, OnlyOneSessionAuthorizationPolicyProvider>();

		public static IServiceCollection AddAuthorizationIfNeeded(this IServiceCollection services)
		{
			if(!_authorizationAdded)
			{
				services.AddAuthorization();
				_authorizationAdded = true;
			}

			return services;
		}

		public static IMvcBuilder AddSharedControllers(this IMvcBuilder mvcBuilder)
		{
			return mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);
		}

		public static IServiceCollection AddVersioning(this IServiceCollection services)
		{
			var entryAssembly = Assembly.GetEntryAssembly();

			var maxVersion = entryAssembly
				.GetTypes()
				.Where(t => t.IsClass
					&& !t.IsAbstract
					&& t.Assembly == entryAssembly
					&& typeof(ApiControllerBase).IsAssignableFrom(t)
					&& t.GetAttribute<ApiVersionAttribute>().Any()
					&& !t.GetAttribute<ApiVersionAttribute>().Any(ava => ava.Deprecated))
				.SelectMany(c => c.GetAttribute<ApiVersionAttribute>())
				.SelectMany(ava => ava.Versions)
				.Distinct()
				.OrderByDescending(av => av.MajorVersion)
				.ThenByDescending(av => av.MinorVersion)
				.FirstOrDefault() ?? new ApiVersion(1, 0);
			
			services.AddApiVersioning(config =>
			{
				config.DefaultApiVersion = maxVersion;
				config.AssumeDefaultVersionWhenUnspecified = true;
				config.ReportApiVersions = true;
				config.ApiVersionReader = new UrlSegmentApiVersionReader();
			});

			services.AddVersionedApiExplorer(config =>
			{
				config.GroupNameFormat = "'v'VVV";
				config.SubstituteApiVersionInUrl = true;
			});

			services.AddSwaggerGen(c =>
			{
				c.CustomSchemaIds(type => type.FullName);
			});

			services.ConfigureOptions<ConfigureSwaggerOptions>();

			return services;
		}

		/// <summary>
		/// Регистрация идемпотентности запросов к методам, отмеченным <see cref="IdempotentAttribute"/>:
		/// чтение заголовка времени действия (<see cref="IActionTimeUtcProvider"/>), построение ключа, кэш сроков из настроек,
		/// описание заголовков в Swagger. Сам middleware подключается через <see cref="UseIdempotency"/>.<br/>
		/// Требует зарегистрированных <see cref="IIdempotencyStore"/> (например, <c>AddGarnetIdempotencyStore</c>
		/// вместе с <c>AddWebApiGarnetConnection</c>), <c>IFeatureManager</c>, <see cref="IDatabaseConnectionSettings"/>
		/// и <see cref="IApiIdempotencySettings"/>
		/// </summary>
		/// <param name="services">Коллекция сервисов</param>
		/// <param name="apiName">Имя API - часть ключа записи, разводит записи разных API в общем хранилище</param>
		/// <param name="timingsSelector">Выбор сроков идемпотентности этого API из настроек</param>
		/// <returns>Коллекция сервисов</returns>
		public static IServiceCollection AddIdempotency(
			this IServiceCollection services,
			string apiName,
			Func<IApiIdempotencySettings, IdempotencyTimings> timingsSelector)
		{
			if(timingsSelector is null)
			{
				throw new ArgumentNullException(nameof(timingsSelector));
			}

			services
				.AddHttpContextAccessor()
				.AddScoped<IActionTimeUtcProvider, ActionTimeUtcProvider>()
				.AddSingleton(sp => new IdempotencyKeyBuilder(
					sp.GetRequiredService<IDatabaseConnectionSettings>(),
					apiName))
				.AddSingleton(sp => new IdempotencyTimingsProvider(
					sp.GetRequiredService<ILogger<IdempotencyTimingsProvider>>(),
					() => timingsSelector(sp.GetRequiredService<IApiIdempotencySettings>())))
				.Configure<SwaggerGenOptions>(options => options.OperationFilter<IdempotencyHeadersOperationFilter>());

			return services;
		}

		/// <summary>
		/// Подключение middleware идемпотентности. Вызывать после <c>UseAuthorization</c>:
		/// middleware использует пользователя запроса и метаданные выбранного метода.
		/// Требует регистрации через <see cref="AddIdempotency"/>
		/// </summary>
		/// <param name="app">Конвейер обработки запросов</param>
		/// <returns>Конвейер обработки запросов</returns>
		public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app) =>
			app.UseMiddleware<IdempotencyMiddleware>();

		public static void ConfigureJsonSourcesAutoReload(this IConfigurationBuilder configurationBuilder)
		{
			var jsonSources = configurationBuilder.Sources
				.Where(cs => cs is JsonConfigurationSource)
				.Select(cs => cs as JsonConfigurationSource);

			foreach(var jsonSource in jsonSources)
			{
				jsonSource.ReloadOnChange = true;
			}
		}
	}
}
