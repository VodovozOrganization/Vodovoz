using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace ApiClientProvider
{
	public class ApiClientProvider : IApiClientProvider, IDisposable
	{
		private HttpClient _customHttpClient;
		private readonly string _apiBaseParameter = "BaseUri";
		private readonly ICustomHttpClientFactory _customHttpClientFactory;
		private readonly HttpClient _serviceHttpClient;

		public ApiClientProvider(IConfigurationSection apiConfiguration, ICustomHttpClientFactory customHttpClientFactory)
		{
			_customHttpClientFactory = customHttpClientFactory ?? throw new ArgumentNullException(nameof(customHttpClientFactory));
			InitializeClient(apiConfiguration ?? throw new ArgumentNullException(nameof(apiConfiguration)));
		}

		public ApiClientProvider(IConfigurationSection apiConfiguration, HttpClient servicetHttpClient)
		{
			_serviceHttpClient = servicetHttpClient ?? throw new ArgumentNullException(nameof(servicetHttpClient));
			InitializeClient(apiConfiguration ?? throw new ArgumentNullException(nameof(apiConfiguration)));
		}

		protected virtual void InitializeClient(IConfigurationSection apiConfiguration)
		{
			if(_customHttpClientFactory != null)
			{
				_customHttpClient = _customHttpClientFactory.CreateClient();
			}

			if(Client.BaseAddress == null)
			{
				var baseUri = new Uri(apiConfiguration.GetValue<string>(_apiBaseParameter));
				Client.ConfigureHttpClient(baseUri);
			}
		}

		public HttpClient Client => _customHttpClient ?? _serviceHttpClient;

		public void Dispose()
		{
			Client?.Dispose();
		}
	}
}
