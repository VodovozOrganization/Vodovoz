using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Text;
using YooKassaApi.Library.Configs;

namespace YooKassaApi.Client
{
	public static class YooKassaApiExtensions
	{
		public static IServiceCollection AddYooKassaApiClient(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<YooKassaOptions>(
				configuration.GetSection("YooKassa"));

			services.AddHttpClient<IYooKassaApiClient, YooKassaApiClient>((sp, client) =>
			{
				var settings = sp.GetRequiredService<IOptions<YooKassaOptions>>().Value;

				client.BaseAddress = new Uri(settings.ApiUrl);

				var authToken = Convert.ToBase64String(
					Encoding.ASCII.GetBytes($"{settings.ShopId}:{settings.SecretKey}"));

				client.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue("Basic", authToken);

				client.DefaultRequestHeaders.Accept.Add(
					new MediaTypeWithQualityHeaderValue("application/json"));

				client.Timeout = TimeSpan.FromSeconds(30);
			});

			return services;
		}
	}
}
