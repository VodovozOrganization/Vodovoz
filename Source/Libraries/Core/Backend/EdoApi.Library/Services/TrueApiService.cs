using EdoApi.Library.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EdoApi.Library.Services
{
	public class TrueApiService : ITrueApiService
	{
		private readonly IEdoLogger _edoLogger;
		private readonly HttpClient _httpClient;
		private readonly IConfigurationSection _trueApiSection;

		public TrueApiService(
			ILogger<TrueApiService> logger,
			HttpClient client,
			IConfiguration configuration)
		{
			_edoLogger = new EdoLogger<ITrueApiService>(logger ?? throw new ArgumentNullException(nameof(logger)));
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_trueApiSection = (configuration ?? throw new ArgumentNullException(nameof(configuration))).GetSection("TrueApiService");
		}
		public async Task<bool> GetCounterpartyRegisteredInTrueApi(string inn, string productGroup)//7729076804
		{
			var uri = $"{_trueApiSection.GetValue<string>("CheckCounterpartyByInnEndpointURI")}?inns={inn}";
			var request = new HttpRequestMessage(HttpMethod.Get, uri);
			var response = await _httpClient.SendAsync(request);

			if(response.IsSuccessStatusCode)
			{
				string responseBody = await response.Content.ReadAsStringAsync();
				var registrationResult = JsonSerializer.Deserialize<IEnumerable<TrueApiRegistrationDto>>(responseBody);

				if(registrationResult != null)
				{
					var registration = registrationResult.FirstOrDefault();

					var isRegisteredForProductGroup = registration != null
											   && registration.IsRegistered
											   && registration.ProductGroups.Contains(productGroup);

					return isRegisteredForProductGroup;
				}
			}

			_edoLogger.LogError(response);

			return false;
		}
	}
}
