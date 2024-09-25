using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaxcomEdo.Client.Configs;

namespace TaxcomEdo.Client
{
	public static class TaxcomEdoClientExtensions
	{
		public static IServiceCollection AddTaxcomClient(this IServiceCollection services, IConfiguration configuration)
		{
			return services
				.AddScoped<ITaxcomApiClient, TaxcomApiClient>()
				.Configure<TaxcomApiOptions>(c => configuration.GetSection(TaxcomApiOptions.Path))
				.AddSingleton(c => c.GetService<IOptions<TaxcomApiOptions>>().Value);
		}
	}
}
