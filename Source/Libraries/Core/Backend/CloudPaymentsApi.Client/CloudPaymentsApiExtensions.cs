using CloudPaymentsApi.Library.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Text;

namespace CloudPaymentsApi.Client
{
	public static class CloudPaymentsApiExtensions
	{
		public static IServiceCollection AddCloudPaymentsApiClient(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<CloudPaymentsOptions>(
				configuration.GetSection("CloudPayments"));

			services.AddHttpClient<ICloudPaymentsApiClient, CloudPaymentsApiClient>((sp, client) =>
			{
				var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<CloudPaymentsOptions>>();
				var settings = optionsMonitor.CurrentValue;

				client.BaseAddress = new Uri(settings.ApiUrl);

				var authToken = Convert.ToBase64String(
					Encoding.ASCII.GetBytes($"{settings.PublicId}:{settings.ApiSecret}"));

				client.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue("Basic", authToken);

				client.Timeout = TimeSpan.FromSeconds(30);
			});

			return services;
		}
	}
}
