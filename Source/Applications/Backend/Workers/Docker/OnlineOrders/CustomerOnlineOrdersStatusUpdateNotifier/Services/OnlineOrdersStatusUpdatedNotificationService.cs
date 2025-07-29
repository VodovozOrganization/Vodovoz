using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CustomerOnlineOrdersStatusUpdateNotifier.Contracts;
using Microsoft.Extensions.Configuration;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Services
{
	public class OnlineOrdersStatusUpdatedNotificationService : IOnlineOrdersStatusUpdatedNotificationService
	{
		private readonly HttpClient _httpClient;
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private readonly IConfigurationSection _mobileAppSection;
		private readonly IConfigurationSection _vodovozSiteSection;

		public OnlineOrdersStatusUpdatedNotificationService(
			HttpClient client,
			IConfiguration configuration,
			JsonSerializerOptions jsonSerializerOptions)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));

			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}
			
			_mobileAppSection = configuration.GetSection("MobileApp");
			_vodovozSiteSection = configuration.GetSection("VodovozSite");
		}

		public async Task<int> NotifyOfOnlineOrderStatusUpdatedAsync(
			OnlineOrderStatusUpdatedDto statusUpdatedDto, Source source, CancellationToken cancellationToken = default)
		{
			var content = JsonContent.Create(statusUpdatedDto, mediaType: null, _jsonSerializerOptions);
			var response = await _httpClient.PutAsync(GetUriString(source), content, cancellationToken);
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
