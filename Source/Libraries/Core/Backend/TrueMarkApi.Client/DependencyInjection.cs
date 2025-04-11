using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System;
using Vodovoz.Settings.Edo;

namespace TrueMarkApi.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddTrueMarkApiClient(this IServiceCollection services)
		{
			services
				.AddHttpClient<ITrueMarkApiClient, TrueMarkApiClient>((sp, client) =>
				{
					var edoSettings = sp.GetRequiredService<IEdoSettings>();
					client.BaseAddress = new Uri(edoSettings.TrueMarkApiBaseUrl);
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", edoSettings.TrueMarkApiToken);
				})
				.SetHandlerLifetime(TimeSpan.FromMinutes(5));

			return services;
		}
	}
}
