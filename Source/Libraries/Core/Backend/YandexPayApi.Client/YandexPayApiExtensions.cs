using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using YandexPayApi.Library.Configs;

namespace YandexPayApi.Client
{
	public static class YandexPayApiExtensions
	{
		public static IServiceCollection AddYandexPayApiClient(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<YandexPayOptions>(
				configuration.GetSection("YandexPay"));

			services.AddHttpClient<IYandexPayApiClient, YandexPayApiClient>((sp, client) =>
			{
				var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<YandexPayOptions>>();
				var settings = optionsMonitor.CurrentValue;

				client.BaseAddress = new Uri(settings.ApiUrl);
				client.DefaultRequestHeaders.Add("Authorization", $"Api-Key {settings.ApiKey}");
				client.DefaultRequestHeaders.Add("Accept", "application/json");

				client.Timeout = TimeSpan.FromSeconds(30);
			});

			return services;
		}
	}
}
