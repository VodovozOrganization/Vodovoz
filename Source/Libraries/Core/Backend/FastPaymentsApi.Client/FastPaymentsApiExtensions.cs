using FastPaymentsApi.Contracts.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace FastPaymentsApi.Client
{
	public static class FastPaymentsApiExtensions
	{
		public static IServiceCollection AddFastPaymentsApiClient(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<FastPaymentsOptions>(
				configuration.GetSection("FastPayments"));

			services.AddHttpClient<IFastPaymentsApiClient, FastPaymentsApiClient>((sp, client) =>
			{
				var settings = sp.GetRequiredService<IOptions<FastPaymentsOptions>>().Value;

				client.BaseAddress = new Uri(settings.ApiUrl);
				client.Timeout = TimeSpan.FromSeconds(30);
			});

			return services;
		}
	}
}
