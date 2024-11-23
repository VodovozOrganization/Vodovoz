using MassTransit.Internals;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using System.Linq;
using System.Reflection;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.Presentation.WebApi.Options;
using Vodovoz.Presentation.WebApi.Security;
using Vodovoz.Presentation.WebApi.Security.OnlyOneSession;

namespace Vodovoz.Presentation.WebApi
{
	public static class DependencyInjection
	{
		private static readonly string _securityOptionsConfigurationKey = typeof(SecurityOptions).Name.Replace("Options", "");

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
			var callingAssembly = Assembly.GetEntryAssembly();

			var controllersVersions = callingAssembly
				.GetTypes()
				.Where(t => t.IsClass
					&& !t.IsAbstract
					&& t.Assembly == callingAssembly
					&& typeof(ApiControllerBase).IsAssignableFrom(t)
					&& t.GetAttribute<ApiVersionAttribute>().Any()
					&& !t.GetAttribute<ApiVersionAttribute>().Any(ava => ava.Deprecated))
				.SelectMany(c => c.GetAttribute<ApiVersionAttribute>())
				.SelectMany(ava => ava.Versions)
				.Distinct();

			var maxVersion = controllersVersions
				.OrderBy(av => av.MajorVersion)
				.ThenBy(av => av.MinorVersion)
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

			services.AddSwaggerGen();

			services.ConfigureOptions<ConfigureSwaggerOptions>();

			return services;
		}

		public static IServiceCollection AddFeatureFlags(this IServiceCollection services)
		{
			services.AddFeatureManagement();
			return services;
		}

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
