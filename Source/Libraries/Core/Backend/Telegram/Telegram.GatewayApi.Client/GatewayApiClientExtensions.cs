using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.GatewayApi.Client.Configs;

namespace Telegram.GatewayApi.Client
{
	public static class GatewayApiClientExtensions
	{
		public static IServiceCollection AddTelegramGatewayApiClient(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.Configure<TelegramGatewayApiOptions>(options =>
				{
					configuration.Bind(TelegramGatewayApiOptions.Path, options);
				})
				.AddHttpClient<ITelegramGatewayApiClient, TelegramGatewayApiClient>(
					(sp, client) =>
					{
						var options = sp.GetRequiredService<IOptions<TelegramGatewayApiOptions>>().Value;
					
						client.BaseAddress = new Uri(options.BaseUrl);
						client.DefaultRequestHeaders.Accept.Clear();
						client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
						client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.GatewayApiToken);
					});
			
			return services;
		}
	}
}
