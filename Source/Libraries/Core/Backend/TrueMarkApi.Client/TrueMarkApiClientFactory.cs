using System;
using System.Net.Http;
using Vodovoz.Settings.Edo;

namespace TrueMarkApi.Client
{
	public class TrueMarkApiClientFactory : ITrueMarkApiClientFactory
	{
		private readonly IEdoSettings _edoSettings;
		private readonly IHttpClientFactory _httpClientFactory;

		public TrueMarkApiClientFactory(IEdoSettings edoSettings, IHttpClientFactory  httpClientFactory)
		{
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		}

		public ITrueMarkApiClient GetClient()
		{
			return new TrueMarkApiClient(_httpClientFactory, _edoSettings.TrueMarkApiBaseUrl, _edoSettings.TrueMarkApiToken);
		}
	}
}
