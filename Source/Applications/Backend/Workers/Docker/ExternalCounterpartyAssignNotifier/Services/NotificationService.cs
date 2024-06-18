using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using Microsoft.Extensions.Configuration;
using Vodovoz.Domain.Client;

namespace ExternalCounterpartyAssignNotifier.Services
{
	public class NotificationService : INotificationService
	{
		private readonly HttpClient _httpClient;
		private readonly JsonSerializerOptions _serializerOptions;
		private readonly IConfigurationSection _mobileAppSection;
		private readonly IConfigurationSection _vodovozSiteSection;

		public NotificationService(HttpClient client, IConfiguration configuration, JsonSerializerOptions serializerOptions)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));

			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}
			
			_mobileAppSection = configuration.GetSection("MobileApp");
			_vodovozSiteSection = configuration.GetSection("VodovozSite");
		}

		public async Task<int> NotifyOfCounterpartyAssignAsync(
			RegisteredNaturalCounterpartyDto counterpartyDto, CounterpartyFrom counterpartyFrom)
		{
			var response = await _httpClient.PostAsJsonAsync(GetUriString(counterpartyFrom), counterpartyDto, _serializerOptions);
			return (int)response.StatusCode;
		}

		private string GetUriString(CounterpartyFrom counterpartyFrom)
		{
			switch(counterpartyFrom)
			{
				case CounterpartyFrom.MobileApp:
					return $"{_mobileAppSection["BaseUrl"]}{_mobileAppSection["NotificationAddress"]}";
				case CounterpartyFrom.WebSite:
					return $"{_vodovozSiteSection["BaseUrl"]}{_vodovozSiteSection["NotificationAddress"]}";
				default:
					throw new ArgumentOutOfRangeException(nameof(counterpartyFrom), counterpartyFrom, null);
			}
		}
	}
}
