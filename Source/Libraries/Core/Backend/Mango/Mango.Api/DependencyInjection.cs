using Mango.Api.Validators;
using Mango.CallsPublishing;
using Mango.Core.Sign;
using Microsoft.Extensions.DependencyInjection;

namespace Mango.Api
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddMangoApi(this IServiceCollection services)
		{
			services.AddControllers();

			services
				.AddScoped<ISignGenerator, SignGenerator>()
				.AddScoped<IDefaultSignGenerator, DefaultSignGenerator>()
				.AddScoped<SignValidator>()
				.AddScoped<KeyValidator>()
				;

			services.AddCallsPublishing();

			return services;
		}
	}
}
