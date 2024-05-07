using ApiAuthentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace ApiAuthentication
{
	public static class DependencyInjection
    {
		public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services)
		{
			services.AddScoped<IAuthenticationHandler, ApiKeyAuthenticationHandler>();
			services.AddScoped<AuthenticationSchemeOptions, ApiKeyAuthenticationOptions>();

			services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
				.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, null);

			return services;
		}
    }
}
