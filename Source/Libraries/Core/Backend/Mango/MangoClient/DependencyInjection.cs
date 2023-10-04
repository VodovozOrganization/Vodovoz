using Mango.Core.Settings;
using Mango.Core.Sign;
using Mango.Api.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Mango.Api
{
	public static class DependencyInjection
	{
		public static IServiceCollection ConfigureMangoServices(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddControllers();

			serviceCollection
				.AddScoped<IMangoSettings, ConfigurationMangoSettings>()
				.AddScoped<ISignGenerator, SignGenerator>()
				.AddScoped<IDefaultSignGenerator, DefaultSignGenerator>()
				.AddScoped<IValidator, SignValidator>()
				.AddScoped<IValidator, KeyValidator>()
				.AddScoped<IRequestValidator, RequestValidator>()
				;

			return serviceCollection;
		}
	}
}
