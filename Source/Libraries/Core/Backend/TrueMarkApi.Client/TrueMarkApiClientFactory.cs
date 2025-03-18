using System;
using System.Net.Http;
using Vodovoz.Settings.Edo;

namespace TrueMarkApi.Client
{
	public class TrueMarkApiClientFactory : ITrueMarkApiClientFactory
	{
		private readonly IEdoSettings _edoSettings;
		private readonly HttpClient _httpClient;

		public TrueMarkApiClientFactory(IEdoSettings edoSettings, HttpClient   httpClient)
		{
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		public ITrueMarkApiClient GetClient()
		{
			return new TrueMarkApiClient(_httpClient, _edoSettings.TrueMarkApiBaseUrl, _edoSettings.TrueMarkApiToken);
		}
	}
}
