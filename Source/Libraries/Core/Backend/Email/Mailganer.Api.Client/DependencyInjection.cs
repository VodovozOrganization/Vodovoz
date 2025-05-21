using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;


namespace Mailganer.Api.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddMailganerApiClient(this IServiceCollection services)
		{
			services.AddOptions<MailganerSettings>().Configure<IConfiguration>((options, config) =>
			{
				config.GetSection("MailganerSettings").Bind(options);
			});

			services.AddHttpClient<MailganerClientV2>((sp, httpClient) =>
			{
				var mailganerSettings = sp.GetRequiredService<IOptions<MailganerSettings>>();
				var apiKey = mailganerSettings.Value.ApiKey;
				
				httpClient.BaseAddress = new Uri("https://api.samotpravil.ru/api/v2/");
				httpClient.DefaultRequestHeaders.Clear();
				httpClient.DefaultRequestHeaders.Add("Authorization", $"{apiKey}");
			});

			services.AddHttpClient<MailganerClientV1>((sp, httpClient) =>
			{
				httpClient.BaseAddress = new Uri("https://api.samotpravil.ru/api/v1/");
			});

			return services;
		}
	}
}
