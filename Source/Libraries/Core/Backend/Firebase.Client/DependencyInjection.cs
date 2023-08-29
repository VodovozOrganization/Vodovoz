using Firebase.Client.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Firebase.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddFirebaseClient(this IServiceCollection serviceCollection, IConfiguration configuration)
		{
			serviceCollection.Configure<FirebaseSettings>((options) =>
			{
				configuration.GetSection(nameof(FirebaseSettings)).Bind(options);
			});

			serviceCollection
				.AddHttpClient<IFirebaseService, FirebaseService>((serviceProvider, httpClient) =>
				{
					var serttings = serviceProvider.GetService<IOptions<FirebaseSettings>>().Value;

					httpClient.BaseAddress = new Uri(serttings.ApiBase);
					httpClient.DefaultRequestHeaders.Accept.Clear();
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + serttings.AccessToken);
				})
				.ConfigurePrimaryHttpMessageHandler(() =>
				{
					return new HttpClientHandler();
				});

			return serviceCollection;
		}
	}
}
