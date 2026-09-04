using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Taxcom.Docflow.Utility;
using TaxcomEdo.Client.Configs;
using Vodovoz.Settings.Database.Edo;
using Vodovoz.Settings.Edo;

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

		public static IServiceCollection AddTaxcomApiClientFactory(this IServiceCollection services)
		{
			services.AddScoped<IEdoSettings, EdoSettings>();
			services.AddScoped<ITaxcomApiFactory, TaxcomApiFactory>();
			services.AddScoped(sp => new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			return services;
		}
	}
}
