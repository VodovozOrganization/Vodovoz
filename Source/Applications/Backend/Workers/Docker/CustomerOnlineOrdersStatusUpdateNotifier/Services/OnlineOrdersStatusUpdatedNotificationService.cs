using System;
using System.Net.Http;
using System.Threading.Tasks;
using CustomerOnlineOrdersStatusUpdateNotifier.Contracts;
using Microsoft.Extensions.Configuration;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Services
{
	public class OnlineOrdersStatusUpdatedNotificationService : IOnlineOrdersStatusUpdatedNotificationService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfigurationSection _mobileAppSection;
		private readonly IConfigurationSection _vodovozSiteSection;

		public OnlineOrdersStatusUpdatedNotificationService(
			HttpClient client,
			IConfiguration configuration)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));

			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}
			
			_mobileAppSection = configuration.GetSection("MobileApp");
			_vodovozSiteSection = configuration.GetSection("VodovozSite");
		}

		public async Task<int> NotifyOfOnlineOrderStatusUpdatedAsync(OnlineOrderStatusUpdatedDto statusUpdatedDto, Source source)
		{
			var response = await _httpClient.PostAsJsonAsync(GetUriString(source), statusUpdatedDto);
			return (int)response.StatusCode;
		}

		private string GetUriString(Source source)
		{
			switch(source)
			{
				case Source.MobileApp:
					return $"{_mobileAppSection["BaseUrl"]}{_mobileAppSection["NotificationAddress"]}";
				case Source.VodovozWebSite:
					return $"{_vodovozSiteSection["BaseUrl"]}{_vodovozSiteSection["NotificationAddress"]}";
				default:
					throw new ArgumentOutOfRangeException(nameof(Source), source, null);
			}
		}
	}
}
