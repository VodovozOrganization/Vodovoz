using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts.Requests;
using Vodovoz.Settings.FastPayments;

namespace FastPaymentsAPI.Library.ApiClients
{
	public class SiteClient : IDisposable
	{
		private readonly ILogger<SiteClient> _logger;
		private readonly ISiteSettings _siteSettings;
		private readonly HttpClient _httpClient;

		public SiteClient(ILogger<SiteClient> logger, ISiteSettings siteSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_siteSettings = siteSettings ?? throw new ArgumentNullException(nameof(siteSettings));

			_httpClient = new HttpClient();
			_httpClient.BaseAddress = new Uri(_siteSettings.BaseUrl);
			_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
		}

		public async Task<bool> NotifyPaymentStatusChangedAsync(FastPaymentStatusChangeNotificationDto paymentNotificationDto)
		{
			var uri = _siteSettings.NotifyOfFastPaymentStatusChangedUri;
			var content = JsonSerializer.Serialize(paymentNotificationDto);
			var response = await _httpClient.PostAsJsonAsync(uri, content);

			if(response.IsSuccessStatusCode)
			{
				return true;
			}

			_logger.LogInformation(response.ReasonPhrase);
			return false;
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}
