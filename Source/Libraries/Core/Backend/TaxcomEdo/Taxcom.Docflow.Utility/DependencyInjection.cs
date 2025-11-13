using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System.Text.Json;
using TaxcomEdo.Client;

namespace Taxcom.Docflow.Utility
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddTaxcomRehandleService(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<IConfigureOptions<TaxcomOrganizationsSettings>>(sp =>
			{
				var config = sp.GetRequiredService<IConfiguration>();

				return new ConfigureOptions<TaxcomOrganizationsSettings>(options =>
				{
					config.GetSection("TaxcomOrganizations").Bind(options);
				});
			});
			services.TryAddScoped(sp => new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			services.TryAddScoped<ITaxcomApiFactory, TaxcomApiFactory>();
			services.TryAddScoped<DocflowStatusesService>();
			services.TryAddScoped<ITaxcomApiClient, TaxcomApiClient>();

			return services;
		}
	}
}
