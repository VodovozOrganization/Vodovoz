using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CustomerOnlineOrdersStatusUpdateNotifier.Configs;
using CustomerOnlineOrdersStatusUpdateNotifier.Contracts;
using Microsoft.Extensions.Options;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Services
{
	public class OnlineOrdersStatusUpdatedNotificationService : IOnlineOrdersStatusUpdatedNotificationService
	{
		private readonly HttpClient _httpClient;
		private readonly NotifierOptions _options;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public OnlineOrdersStatusUpdatedNotificationService(
			HttpClient client,
			IOptionsSnapshot<NotifierOptions> options,
			JsonSerializerOptions jsonSerializerOptions)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
			_jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
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
					return $"{_options.MobileAppUriOptions.BaseUrl}{_options.MobileAppUriOptions.NotificationAddress}";
				case Source.VodovozWebSite:
					return $"{_options.VodovozWebSiteUriOptions.BaseUrl}{_options.VodovozWebSiteUriOptions.NotificationAddress}";
				default:
					throw new ArgumentOutOfRangeException(nameof(Source), source, null);
			}
		}
	}
}
