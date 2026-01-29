using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CustomerAppNotifier.Options;
using CustomerAppsApi.Library.Dto.Counterparties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Accounts.Events;
using Vodovoz.Domain.Client;

namespace CustomerAppNotifier.Services
{
	public class NotificationService : INotificationService
	{
		private readonly HttpClient _httpClient;
		private readonly JsonSerializerOptions _serializerOptions;
		private readonly MobileAppOptions _mobileAppOptions;
		private readonly VodovozWebSiteOptions _vodovozSiteOptions;

		public NotificationService(
			HttpClient client,
			IConfiguration configuration,
			JsonSerializerOptions serializerOptions,
			IOptionsSnapshot<MobileAppOptions> mobileAppOptions,
			IOptionsSnapshot<VodovozWebSiteOptions> vodovozWebSiteOptions
			)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));

			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_mobileAppOptions = mobileAppOptions.Value;
			_vodovozSiteOptions = vodovozWebSiteOptions.Value;
		}

		public async Task<int> NotifyOfCounterpartyAssignAsync(
			RegisteredNaturalCounterpartyDto counterpartyDto, CounterpartyFrom counterpartyFrom)
		{
			var response = await _httpClient.PostAsJsonAsync(GetUriString(counterpartyFrom), counterpartyDto, _serializerOptions);
			return (int)response.StatusCode;
		}
		
		public async Task<bool> SendLogoutEventAsync(LogoutLegalAccountEvent logoutEvent, Source source)
		{
			var response = await _httpClient.PostAsJsonAsync(GetUriString(source), logoutEvent, _serializerOptions);
			return response.IsSuccessStatusCode;
		}

		private string GetUriString(CounterpartyFrom counterpartyFrom)
		{
			var mobileAppAssignNotification = _mobileAppOptions.CounterpartyAssignNotification;
			var siteAssignNotification = _vodovozSiteOptions.CounterpartyAssignNotification;
			
			switch(counterpartyFrom)
			{
				case CounterpartyFrom.MobileApp:
					return $"{mobileAppAssignNotification.BaseUrl}{mobileAppAssignNotification.Address}";
				case CounterpartyFrom.WebSite:
					return $"{siteAssignNotification.BaseUrl}{siteAssignNotification.Address}";
				default:
					throw new ArgumentOutOfRangeException(nameof(counterpartyFrom), counterpartyFrom, null);
			}
		}
		
		private string GetUriString(Source source)
		{
			var mobileAppLogoutNotification = _mobileAppOptions.LogoutLegalAccountEvent;
			var siteLogoutNotification = _vodovozSiteOptions.LogoutLegalAccountEvent;
			
			switch(source)
			{
				case Source.MobileApp:
					return $"{mobileAppLogoutNotification.BaseUrl}{mobileAppLogoutNotification.Address}";
				case Source.VodovozWebSite:
					return $"{siteLogoutNotification.BaseUrl}{siteLogoutNotification.Address}";
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, null);
			}
		}
	}
}
