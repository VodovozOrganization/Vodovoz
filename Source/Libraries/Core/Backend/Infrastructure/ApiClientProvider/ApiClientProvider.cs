using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ApiClientProvider
{
	public class ApiClientProvider : IApiClientProvider, IDisposable
	{
		protected HttpClient _client;
		private readonly string _apiBaseParameter = "BaseUri";

		public ApiClientProvider(IConfigurationSection apiConfiguration)
		{
			InitializeClient(apiConfiguration ?? throw new ArgumentNullException(nameof(apiConfiguration)));
		}

		public ApiClientProvider(IConfigurationSection apiConfiguration, HttpClient сlient)
		{
			_client = сlient;
			InitializeClient(apiConfiguration ?? throw new ArgumentNullException(nameof(apiConfiguration)));
		}

		public HttpClient Client =>_client;

		protected virtual void InitializeClient(IConfigurationSection apiConfiguration)
		{
			if(_client == null)
			{
				_client = new HttpClient();
			}

			_client.BaseAddress = new Uri(apiConfiguration.GetValue<string>(_apiBaseParameter));
			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public void Dispose()
		{
			_client?.Dispose();
		}
	}
}
