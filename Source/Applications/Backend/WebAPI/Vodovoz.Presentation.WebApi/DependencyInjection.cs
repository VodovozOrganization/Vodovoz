using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Presentation.WebApi.Security;

namespace Vodovoz.Presentation.WebApi
{
	public static class DependencyInjection
	{
		private static readonly string _securityOptionsConfigurationKey = typeof(SecurityOptions).Name.Replace("Options", "");

		public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
			=> services.Configure<SecurityOptions>(so =>
			{
				configuration.Bind(_securityOptionsConfigurationKey, so);
			})
			.ConfigureOptions<ConfigureJwtBearerOptions>();
	}
}
