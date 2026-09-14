using System;
using System.Net.Http;
using System.Text.Json;
using Taxcom.Docflow.Utility;
using TaxcomEdo.Client.Configs;
using Vodovoz.Settings.Edo;

namespace TaxcomEdo.Client
{
	public class TaxcomApiFactory : ITaxcomApiFactory
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private readonly IEdoSettings _edoSettings;

		public TaxcomApiFactory(
			IHttpClientFactory httpClientFactory,
			JsonSerializerOptions jsonSerializerOptions,
			IEdoSettings edoSettings
			)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
		}

		public ITaxcomApiClient Create(TaxcomApiOptions taxcomApiOptions)
		{
			return new TaxcomApiClient(_httpClientFactory, taxcomApiOptions, _jsonSerializerOptions);
		}

		public ITaxcomApiClient Create(int organizationId, string edoAccount)
		{
			if(organizationId <= 0)
			{
				throw new ArgumentException("ID организации должен быть больше 0", nameof(organizationId));
			}

			if(string.IsNullOrEmpty(edoAccount))
			{
				throw new ArgumentException("EdoAccount не может быть пустым", nameof(edoAccount));
			}

			if(!_edoSettings.TaxcomOrganizationBaseAddresses.TryGetValue(organizationId, out var baseAddress))
			{
				throw new InvalidOperationException(
					$"Не найдена настройка TaxcomBaseAddress для организации с ID {organizationId}");
			}

			var taxcomApiOptions = new TaxcomApiOptions
			{
				MainEdoAccountBaseAddress = baseAddress,
				GetDocflowStatusEndpoint = _edoSettings.TaxcomGetDocflowStatusEndpoint,
				EdoAccountBaseAddresses = new[] 
				{
					new EdoAccountBaseAddress
					{ 
						EdoAccountId = edoAccount,
						BaseAddress = baseAddress 
					}
				}
			};

			return new TaxcomApiClient(_httpClientFactory, taxcomApiOptions, _jsonSerializerOptions);
		}
	}
}
