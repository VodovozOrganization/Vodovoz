using System;
using System.Net.Http;
using System.Text.Json;
using TaxcomEdo.Client;
using TaxcomEdo.Client.Configs;

namespace Taxcom.Docflow.Utility
{
	public class TaxcomApiFactory : ITaxcomApiFactory
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public TaxcomApiFactory(
			IHttpClientFactory httpClientFactory,
			JsonSerializerOptions jsonSerializerOptions
			)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
		}

		public ITaxcomApiClient Create(TaxcomApiOptions taxcomApiOptions)
		{
			return new TaxcomApiClient(_httpClientFactory, taxcomApiOptions, _jsonSerializerOptions);
		}
	}
}
