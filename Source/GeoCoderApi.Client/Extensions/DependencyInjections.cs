using GeoCoderApi.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;

namespace GeoCoderApi.Client.Extensions
{
	public static class DependencyInjections
	{
		public static IServiceCollection AddGeoCoderClient(this IServiceCollection services)
		{
			services.AddHttpClient<IGeoCoderApiClient, GeoCoderApiClient>((serviceProvider, client) =>
			{
				var geoCoderApiOptionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<GeoCoderApiOptions>>();

				client.BaseAddress = new Uri(geoCoderApiOptionsSnapshot.Value.BaseUri);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", geoCoderApiOptionsSnapshot.Value.ApiToken);
			});

			return services;
		}
	}
}
