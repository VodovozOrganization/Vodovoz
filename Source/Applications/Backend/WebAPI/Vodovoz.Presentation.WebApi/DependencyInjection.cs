using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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
				//.AddConfiguredJwtBearer();

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

		//public static AuthenticationBuilder AddConfiguredJwtBearer(this AuthenticationBuilder builder)
		//{
		//	builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>(
		//		sp =>
		//		{
		//			JwtBearerPostConfigureOptions
		//			sp.GetRequiredService<ConfigureJwtBearerOptions>();
		//		}));
		//	return builder.AddScheme<JwtBearerOptions, JwtBearerHandler>(JwtBearerDefaults.AuthenticationScheme, typeof(JwtBearerHandler).Name, configureOptions);
		//}
	}
}
