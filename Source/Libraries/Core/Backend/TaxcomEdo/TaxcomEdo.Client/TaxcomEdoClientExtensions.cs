using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaxcomEdo.Client.Configs;

namespace TaxcomEdo.Client
{
	public static class TaxcomEdoClientExtensions
	{
		public static IServiceCollection AddTaxcomClient(this IServiceCollection services)
		{
			return services
				.AddScoped<ITaxcomApiClient, TaxcomApiClient>()
				.AddSingleton<TaxcomApiOptions>(c =>
				{
					var configuration = c.GetService<IConfiguration>();
					var taxcomApiOptions = new TaxcomApiOptions();
					configuration.Bind(TaxcomApiOptions.Path, taxcomApiOptions);
					
					return taxcomApiOptions;
				})
				.AddSingleton(_ => new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});
		}
	}
}
